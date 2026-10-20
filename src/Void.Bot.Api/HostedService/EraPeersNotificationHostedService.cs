using System.Text.Json;
using System.Threading.Channels;
using Void.Bot.Api.Hubs;
using Void.Bot.Core.Services.ERA.Social;
using Void.Bot.Core.Services.ERA.Team;
using Void.Bot.Core.Services.Social;
using Void.Bot.Core.Settings.ERA;

namespace Void.Bot.Api.HostedService;

/// <summary>原子发件箱发布到 Redis，各实例只向本机已认证的队伍会话发送失效提示。</summary>
public sealed class EraPeersNotificationHostedService : BackgroundService
{
    private readonly IEraTeamDataProvider _teams;
    private readonly IServiceScopeFactory _scopes;
    private readonly ISocialNotificationTransport _transport;
    private readonly EraNetworkingSetting _setting;
    private readonly ILogger<EraPeersNotificationHostedService> _logger;
    private readonly Dictionary<Guid, (long Epoch, long Version, DateTimeOffset SeenAt)> _delivered = new();

    public EraPeersNotificationHostedService(IEraTeamDataProvider teams, IServiceScopeFactory scopes,
        ISocialNotificationTransport transport, EraNetworkingSetting setting,
        ILogger<EraPeersNotificationHostedService> logger)
    {
        _teams = teams;
        _scopes = scopes;
        _transport = transport;
        _setting = setting;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_setting.Enabled) return;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var subscription = await _teams.SubscribePeersNotificationsAsync(stoppingToken).ConfigureAwait(false);
                var incoming = Channel.CreateBounded<EraPeersNotification>(new BoundedChannelOptions(1024)
                {
                    FullMode = BoundedChannelFullMode.DropOldest,
                    SingleReader = true,
                    SingleWriter = false
                });
                subscription.OnMessage(message =>
                {
                    try
                    {
                        var value = JsonSerializer.Deserialize<EraPeersNotification>(message.Message.ToString());
                        if (value != null) incoming.Writer.TryWrite(value);
                    }
                    catch (JsonException) { SocialNotificationMetrics.NotificationFailed(); }
                });
                using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                var receive = ReceiveAsync(incoming.Reader, lifetime.Token);
                var dispatch = DispatchAsync(lifetime.Token);
                try
                {
                    await Task.WhenAny(receive, dispatch).ConfigureAwait(false);
                    lifetime.Cancel();
                    await Task.WhenAll(receive, dispatch).ConfigureAwait(false);
                }
                finally { await subscription.UnsubscribeAsync().ConfigureAwait(false); }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "ERA peers notification subscription failed; retrying.");
                try { await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false); }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            }
        }
    }

    private async Task DispatchAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(250));
        while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
        {
            foreach (var value in await _teams.GetDuePeersNotificationsAsync(cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    if (value.ExpiresAtUnixMilliseconds > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                    {
                        await _teams.PublishPeersNotificationAsync(value, cancellationToken).ConfigureAwait(false);
                    }
                    await _teams.CompletePeersNotificationAsync(value, false, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
                {
                    SocialNotificationMetrics.NotificationFailed();
                    _logger.LogWarning(exception, "ERA peers notification publish failed for team {TeamId}.", value.TeamId);
                    await _teams.CompletePeersNotificationAsync(value, true, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }

    private async Task ReceiveAsync(ChannelReader<EraPeersNotification> incoming, CancellationToken cancellationToken)
    {
        await foreach (var value in incoming.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            try
            {
                var team = await _teams.GetTeamAsync(value.TeamId, cancellationToken).ConfigureAwait(false);
                var now = DateTimeOffset.UtcNow;
                if (_delivered.TryGetValue(value.TeamId, out var previous) &&
                    previous.Epoch == value.Notification.AuthorityEpoch && previous.Version >= value.Notification.ResourceVersion) continue;
                if (team == null || team.ExpiresAt <= now || team.AuthorityEpoch != value.Notification.AuthorityEpoch ||
                    value.ExpiresAtUnixMilliseconds <= now.ToUnixTimeMilliseconds()) continue;
                using var scope = _scopes.CreateScope();
                var data = scope.ServiceProvider.GetRequiredService<IEraSocialDataProvider>();
                var players = team.Members.Where(member => member.ExpiresAt > now).Select(member => member.PlayerId).Distinct().ToArray();
                var accounts = await data.GetAccountIdsAsync(players, cancellationToken).ConfigureAwait(false);
                foreach (var account in accounts.Values.Distinct())
                    await _transport.SendAsync(account, true, value.Notification, cancellationToken).ConfigureAwait(false);
                _delivered[value.TeamId] = (team.AuthorityEpoch, value.Notification.ResourceVersion, now);
                if (_delivered.Count > 1024)
                {
                    foreach (var id in _delivered.Where(item => item.Value.SeenAt < now.AddMinutes(-2)).Select(item => item.Key).ToArray())
                        _delivered.Remove(id);
                    if (_delivered.Count > 10000) _delivered.Clear();
                }
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                SocialNotificationMetrics.NotificationFailed();
                _logger.LogWarning(exception, "ERA peers local delivery failed for team {TeamId}; clients can resync.", value.TeamId);
            }
        }
    }
}
