using Void.Bot.Message.Constants.ERA;
using StackExchange.Redis;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Void.Bot.Core.Services.ERA.Team;
using Void.Bot.Core.Settings.ERA;

namespace Void.Bot.Api.HostedService;

/// <summary>与 HTTPS 共进程的 UDP 登记监听，不接收或转发 ENet 游戏包。</summary>
public class EraPunchHostedService : BackgroundService
{
    private static readonly byte[] Prefix = Encoding.UTF8.GetBytes(EraNetworkingConstants.RegistrationPrefix);
    private readonly IEraPunchCoordinator _coordinator;
    private readonly EraNetworkingSetting _setting;
    private readonly Serilog.ILogger _logger;
    private UdpClient? _udp;

    public EraPunchHostedService(
        IEraPunchCoordinator coordinator,
        EraNetworkingSetting setting,
        Serilog.ILogger logger)
    {
        _coordinator = coordinator;
        _setting = setting;
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        if (_setting.Enabled)
        {
            // 启动时绑定，端口冲突直接阻止启动，避免 HTTP 可用但 UDP 未监听。
            _udp = new UdpClient(new IPEndPoint(_setting.BindAddress, _setting.UdpPort));
            _logger.Information("ERA punch coordinator listening on UDP port {Port}", _setting.UdpPort);
        }

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_setting.Enabled || _udp == null)
        {
            return;
        }

        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        var receive = ReceiveAsync(lifetime.Token);
        var cleanup = CleanupAsync(lifetime.Token);
        try
        {
            await Task.WhenAny(receive, cleanup).ConfigureAwait(false);
            await lifetime.CancelAsync().ConfigureAwait(false);
            await Task.WhenAll(receive, cleanup).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // 正常关闭，不记录为失败。
        }
    }

    private async Task ReceiveAsync(CancellationToken cancellationToken)
    {
        var window = Stopwatch.StartNew();
        var packets = 0;
        var sources = new Dictionary<IPAddress, int>();
        var redisWarning = Stopwatch.StartNew();
        var packetWarning = Stopwatch.StartNew();
        void LogPacketRejected(string reason, IPEndPoint endpoint)
        {
            if (packetWarning.Elapsed.TotalSeconds < 5)
            {
                return;
            }
            packetWarning.Restart();
            _logger.Warning("ERA UDP packet rejected: reason {Reason}, endpoint {Endpoint}", reason, endpoint);
        }
        while (!cancellationToken.IsCancellationRequested)
        {
            var datagram = await _udp!.ReceiveAsync(cancellationToken).ConfigureAwait(false);
            if (window.Elapsed.TotalSeconds >= 1)
            {
                window.Restart();
                packets = 0;
                sources.Clear();
            }

            packets++;
            if (packets > _setting.PacketsPerSecond || datagram.Buffer.Length > EraNetworkingConstants.MaxDatagramBytes ||
                datagram.Buffer.Length <= Prefix.Length || !datagram.Buffer.AsSpan(0, Prefix.Length).SequenceEqual(Prefix))
            {
                LogPacketRejected("rate_limit_size_or_protocol_prefix", datagram.RemoteEndPoint);
                continue;
            }

            sources.TryGetValue(datagram.RemoteEndPoint.Address, out var count);
            if (count >= EraNetworkingConstants.PacketsPerSourcePerSecond ||
                (count == 0 && sources.Count >= EraNetworkingConstants.MaxSourcesPerWindow))
            {
                continue;
            }

            sources[datagram.RemoteEndPoint.Address] = count + 1;
            try
            {
                using var document = JsonDocument.Parse(datagram.Buffer.AsMemory(Prefix.Length),
                    new JsonDocumentOptions
                    {
                        MaxDepth = 4
                    });
                var data = document.RootElement;
                if (data.ValueKind != JsonValueKind.Object ||
                    !data.TryGetProperty("teamId", out var team) || !team.TryGetGuid(out var teamId) ||
                    !data.TryGetProperty("memberId", out var member) || !member.TryGetGuid(out var memberId) ||
                    !data.TryGetProperty("attemptId", out var attempt) || !attempt.TryGetGuid(out var attemptId) ||
                    !data.TryGetProperty("token", out var credential) || credential.ValueKind != JsonValueKind.String)
                {
                    LogPacketRejected("invalid_registration_fields", datagram.RemoteEndPoint);
                    continue;
                }

                var token = credential.GetString();
                if (token != null)
                {
                    await _coordinator.RegisterAsync(teamId, memberId, attemptId, token, datagram.RemoteEndPoint, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (RedisException)
            {
                if (redisWarning.Elapsed.TotalSeconds >= 5)
                {
                    _logger.Warning("ERA UDP registration suspended: shared Redis unavailable");
                    redisWarning.Restart();
                }
            }
            catch (JsonException)
            {
                LogPacketRejected("invalid_json", datagram.RemoteEndPoint);
                // 公网无效数据丢弃，不记录原始报文或凭证。
            }
            catch (InvalidOperationException)
            {
                LogPacketRejected("invalid_field_type", datagram.RemoteEndPoint);
                // JSON 字段类型不匹配，按无效登记处理。
            }
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
        {
            try
            {
                await _coordinator.RemoveExpiredAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (RedisException)
            {
                _logger.Warning("ERA team index cleanup delayed: shared Redis unavailable");
            }
        }
    }

    public override void Dispose()
    {
        _udp?.Dispose();
        base.Dispose();
    }
}
