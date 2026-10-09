using System.Text.Json;
using Void.Bot.Api.Hubs;
using Void.Bot.Core.Services.Social;
using Void.Bot.Message.Dto.Social;

namespace Void.Bot.Api.HostedService;

public sealed class SocialNotificationHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ISocialNotificationTransport _transport;
    private readonly ILogger<SocialNotificationHostedService> _logger;

    public SocialNotificationHostedService(IServiceScopeFactory scopes,
        ISocialNotificationTransport transport, ILogger<SocialNotificationHostedService> logger)
    {
        _scopes = scopes;
        _transport = transport;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var nextCleanup = DateTimeOffset.UtcNow;
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                using var scope = _scopes.CreateScope();
                var outbox = scope.ServiceProvider.GetRequiredService<ISocialOutboxDataProvider>();
                var rows = await outbox.GetDueAsync(stoppingToken).ConfigureAwait(false);
                foreach (var row in rows)
                {
                    try
                    {
                        var notification = JsonSerializer.Deserialize<SocialNotificationDto>(row.Payload);
                        if (notification == null)
                        {
                            throw new InvalidOperationException("Invalid social notification payload.");
                        }
                        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                        timeout.CancelAfter(TimeSpan.FromSeconds(5));
                        await _transport.SendAsync(row.RecipientAccountId, row.EraOnly, notification,
                            timeout.Token).ConfigureAwait(false);
                        await outbox.CompleteAsync(row.Id, stoppingToken).ConfigureAwait(false);
                    }
                    catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
                    {
                        SocialNotificationMetrics.NotificationFailed();
                        _logger.LogWarning(exception, "Social notification {EventId} failed at attempt {Attempt}.",
                            row.Id, row.Attempts + 1);
                        await outbox.RetryAsync(row.Id, row.Attempts + 1, stoppingToken).ConfigureAwait(false);
                    }
                }
                if (DateTimeOffset.UtcNow >= nextCleanup)
                {
                    await outbox.CleanupAsync(stoppingToken).ConfigureAwait(false);
                    nextCleanup = DateTimeOffset.UtcNow.AddMinutes(10);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Social notification dispatcher failed.");
            }
        }
    }
}
