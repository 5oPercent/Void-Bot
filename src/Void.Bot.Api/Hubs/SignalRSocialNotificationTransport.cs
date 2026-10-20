using Microsoft.AspNetCore.SignalR;
using Void.Bot.Core.Services.Social;
using Void.Bot.Core.Services.ERA.Social;
using Void.Bot.Core.Services.ERA.Team;
using Void.Bot.Message.Dto.Social;

namespace Void.Bot.Api.Hubs;

public sealed class SignalRSocialNotificationTransport : ISocialNotificationTransport
{
    private readonly IHubContext<WebSocialHub> _web;
    private readonly IHubContext<EraSocialHub> _era;
    private readonly SocialConnectionRegistry _connections;
    private readonly IEraTeamDataProvider _sessions;
    private readonly IServiceScopeFactory _scopes;

    public SignalRSocialNotificationTransport(IHubContext<WebSocialHub> web,
        IHubContext<EraSocialHub> era, SocialConnectionRegistry connections, IEraTeamDataProvider sessions,
        IServiceScopeFactory scopes)
    {
        _web = web;
        _era = era;
        _connections = connections;
        _sessions = sessions;
        _scopes = scopes;
    }

    public async Task SendAsync(Guid accountId, bool eraOnly,
        SocialNotificationDto notification, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        cancellationToken = timeout.Token;
        var connections = _connections.Snapshot(accountId).Where(value =>
            value.AccountId == accountId && value.TokenExpiresAt > DateTimeOffset.UtcNow).ToArray();
        if (notification.EventType == "era.team_peers_changed")
        {
            using var scope = _scopes.CreateScope();
            var data = scope.ServiceProvider.GetRequiredService<IEraSocialDataProvider>();
            var players = await data.GetPlayerIdsAsync(new[] { accountId }, cancellationToken).ConfigureAwait(false);
            var team = await _sessions.GetTeamAsync(notification.ResourceId, cancellationToken).ConfigureAwait(false);
            if (!players.TryGetValue(accountId, out var playerId) || team == null ||
                !team.Members.Any(member => member.PlayerId == playerId && member.ExpiresAt > DateTimeOffset.UtcNow)) return;
            var active = new List<SocialConnection>();
            foreach (var connection in connections)
            {
                if (connection.EraClientId.HasValue && await _sessions.IsClientSessionActiveAsync(accountId,
                    connection.EraClientId.Value, cancellationToken).ConfigureAwait(false)) active.Add(connection);
            }
            connections = active.ToArray();
        }
        var era = connections.Where(value => value.EraClientId.HasValue)
            .Select(value => value.ConnectionId).ToArray();
        if (era.Length > 0)
        {
            await _era.Clients.Clients(era).SendAsync("SocialChanged", notification,
                cancellationToken).ConfigureAwait(false);
            SocialNotificationMetrics.NotificationSent("era", era.Length, notification.OccurredAt);
        }
        if (!eraOnly)
        {
            var web = connections.Where(value => !value.EraClientId.HasValue)
                .Select(value => value.ConnectionId).ToArray();
            if (web.Length > 0)
            {
                await _web.Clients.Clients(web).SendAsync("SocialChanged", notification,
                    cancellationToken).ConfigureAwait(false);
                SocialNotificationMetrics.NotificationSent("web", web.Length, notification.OccurredAt);
            }
        }
    }
}
