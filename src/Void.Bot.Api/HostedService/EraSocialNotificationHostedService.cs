using Void.Bot.Api.Hubs;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Core.Services.Social;
using Void.Bot.Core.Settings.ERA;
using Void.Bot.Message.Dto.Social;

namespace Void.Bot.Api.HostedService;

/// <summary>分发 Redis 邀请通知，并分批比较有效游戏会话的公开状态；不比较心跳有效期。</summary>
public sealed class EraSocialNotificationHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IEraTeamDataProvider _teams;
    private readonly ISocialNotificationTransport _transport;
    private readonly EraNetworkingSetting _setting;
    private readonly ILogger<EraSocialNotificationHostedService> _logger;
    private readonly Dictionary<Guid, string> _presence;

    public EraSocialNotificationHostedService(IServiceScopeFactory scopes, IEraTeamDataProvider teams,
        ISocialNotificationTransport transport, EraNetworkingSetting setting,
        ILogger<EraSocialNotificationHostedService> logger)
    {
        _scopes = scopes;
        _teams = teams;
        _transport = transport;
        _setting = setting;
        _logger = logger;
        _presence = new Dictionary<Guid, string>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_setting.Enabled)
        {
            return;
        }
        var nextPresence = DateTimeOffset.UtcNow;
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                await DispatchInvitationsAsync(stoppingToken).ConfigureAwait(false);
                if (DateTimeOffset.UtcNow >= nextPresence)
                {
                    await PublishPresenceAsync(stoppingToken).ConfigureAwait(false);
                    nextPresence = DateTimeOffset.UtcNow.AddSeconds(5);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "ERA social notification dispatcher failed.");
            }
        }
    }

    private async Task DispatchInvitationsAsync(CancellationToken cancellationToken)
    {
        var notifications = await _teams.GetDueSocialNotificationsAsync(cancellationToken).ConfigureAwait(false);
        foreach (var notification in notifications)
        {
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(TimeSpan.FromSeconds(5));
                await _transport.SendAsync(notification.RecipientAccountId, true, notification.Notification,
                    timeout.Token).ConfigureAwait(false);
                await _teams.CompleteSocialNotificationAsync(notification.QueueId, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                SocialNotificationMetrics.NotificationFailed();
                _logger.LogWarning(exception, "ERA notification {QueueId} failed at attempt {Attempt}.",
                    notification.QueueId, notification.Attempts + 1);
                await _teams.RetrySocialNotificationAsync(notification, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task PublishPresenceAsync(CancellationToken cancellationToken)
    {
        var online = await _teams.GetOnlineAccountIdsAsync(cancellationToken).ConfigureAwait(false);
        var candidates = online.Concat(_presence.Keys).Distinct().ToArray();
        using var scope = _scopes.CreateScope();
        var data = scope.ServiceProvider.GetRequiredService<IEraSocialDataProvider>();
        foreach (var batch in candidates.Chunk(128))
        {
            var players = await data.GetPlayerIdsAsync(batch, cancellationToken).ConfigureAwait(false);
            var snapshots = await _teams.GetPresenceSnapshotsAsync(batch, players, cancellationToken).ConfigureAwait(false);
            var changed = snapshots.Where(value => !_presence.TryGetValue(value.AccountId, out var previous) ||
                previous != value.Fingerprint()).ToArray();
            if (changed.Length == 0)
            {
                continue;
            }
            var edges = await data.GetFriendEdgesAsync(changed.Select(value => value.AccountId).ToArray(),
                cancellationToken).ConfigureAwait(false);
            foreach (var snapshot in changed)
            {
                var notification = new SocialNotificationDto
                {
                    EventId = Guid.NewGuid(),
                    EventType = "era.friend_presence_changed",
                    ResourceId = snapshot.AccountId,
                    ResourceVersion = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    OccurredAt = DateTimeOffset.UtcNow
                };
                var recipients = edges.Where(value => value.FirstAccountId == snapshot.AccountId || value.SecondAccountId == snapshot.AccountId)
                    .Select(value => value.FirstAccountId == snapshot.AccountId ? value.SecondAccountId : value.FirstAccountId).Distinct();
                foreach (var recipient in recipients)
                {
                    await _transport.SendAsync(recipient, true, notification, cancellationToken).ConfigureAwait(false);
                }
                if (snapshot.IsOnline)
                {
                    _presence[snapshot.AccountId] = snapshot.Fingerprint();
                }
                else
                {
                    _presence.Remove(snapshot.AccountId);
                }
            }
        }
    }
}
