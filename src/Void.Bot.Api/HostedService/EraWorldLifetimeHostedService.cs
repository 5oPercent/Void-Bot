using Mediator.Net;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Core.Settings.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Api.HostedService;

/// <summary>有界扫描超时候选；每个世界通过独立 Mediator 事务确认，单个失败不影响整批。</summary>
public sealed class EraWorldLifetimeHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly EraNetworkingSetting _networking;
    private readonly EraWorldLifetimeSetting _setting;
    private readonly TimeProvider _clock;
    private readonly ILogger<EraWorldLifetimeHostedService> _logger;

    public EraWorldLifetimeHostedService(IServiceScopeFactory scopes, EraNetworkingSetting networking,
        EraWorldLifetimeSetting setting, TimeProvider clock, ILogger<EraWorldLifetimeHostedService> logger)
    {
        _scopes = scopes;
        _networking = networking;
        _setting = setting;
        _clock = clock;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_networking.Enabled) return;
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_setting.ScanIntervalSeconds));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                try
                {
                    List<Guid> candidates;
                    await using (var scanScope = _scopes.CreateAsyncScope())
                    {
                        var data = scanScope.ServiceProvider.GetRequiredService<IEraWorldDataProvider>();
                        var now = _clock.GetUtcNow();
                        candidates = await data.GetTimeoutCandidatesAsync(now.AddSeconds(-_setting.InactivityTimeoutSeconds),
                            now.AddSeconds(-_setting.ScanIntervalSeconds), _setting.BatchSize, stoppingToken).ConfigureAwait(false);
                    }
                    var unavailable = 0;
                    foreach (var worldId in candidates)
                    {
                        try
                        {
                            await using var scope = _scopes.CreateAsyncScope();
                            var response = await scope.ServiceProvider.GetRequiredService<IMediator>()
                                .SendAsync<CheckEraWorldTimeoutCommand, CheckEraWorldTimeoutResponse>(
                                    new CheckEraWorldTimeoutCommand { WorldInstanceId = worldId }, stoppingToken).ConfigureAwait(false);
                            if (response.Status == EraWorldTimeoutStatus.Unavailable) unavailable++;
                        }
                        catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
                        {
                            _logger.LogWarning(exception, "ERA world timeout check failed for {WorldInstanceId}; retrying later.", worldId);
                        }
                    }
                    if (unavailable > 0)
                        _logger.LogWarning("ERA world timeout checks deferred for {Count} worlds because authority data is unavailable.", unavailable);
                }
                catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning(exception, "ERA world lifetime scan failed; retrying later.");
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }
}
