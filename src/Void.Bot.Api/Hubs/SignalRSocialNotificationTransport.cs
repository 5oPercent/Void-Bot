using Microsoft.AspNetCore.SignalR;
using Void.Bot.Core.Services.Social;
using Void.Bot.Message.Dto.Social;

namespace Void.Bot.Api.Hubs;

public sealed class SignalRSocialNotificationTransport : ISocialNotificationTransport
{
    private readonly IHubContext<WebSocialHub> _web;
    private readonly IHubContext<EraSocialHub> _era;
    private readonly SocialConnectionRegistry _connections;

    public SignalRSocialNotificationTransport(IHubContext<WebSocialHub> web,
        IHubContext<EraSocialHub> era, SocialConnectionRegistry connections)
    {
        _web = web;
        _era = era;
        _connections = connections;
    }

    public async Task SendAsync(Guid accountId, bool eraOnly,
        SocialNotificationDto notification, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        cancellationToken = timeout.Token;
        var connections = _connections.Snapshot(accountId).Where(value =>
            value.AccountId == accountId && value.TokenExpiresAt > DateTimeOffset.UtcNow).ToArray();
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
