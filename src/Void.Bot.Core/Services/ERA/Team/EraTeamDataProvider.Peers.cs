using System.Text.Json;
using StackExchange.Redis;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Dto.Social;

namespace Void.Bot.Core.Services.ERA.Team;

/// <summary>Redis 对端通知发件箱记录，包含队伍标识、分发重试状态和客户端失效提示。</summary>
public sealed class EraPeersNotification
{
    /// <summary>发生对端变化的队伍 ID，同时作为发件箱合并键，每队仅保留最新通知。</summary>
    public Guid TeamId { get; set; }
    /// <summary>已结束的分发尝试次数；重新安排失败重试前递增。</summary>
    public int Attempts { get; set; }
    /// <summary>发件箱记录的绝对截止时间，单位为 Unix 毫秒；与队伍或端点租约无关。</summary>
    public long ExpiresAtUnixMilliseconds { get; set; }
    /// <summary>发给客户端的变化提示，不包含对端地址或打洞凭证。</summary>
    public SocialNotificationDto Notification { get; set; } = new();
}

public partial interface IEraTeamDataProvider
{
    /// <summary>读取已到分发时间的通知，每批最多 128 条；读取不领取或移除记录。</summary>
    Task<List<EraPeersNotification>> GetDuePeersNotificationsAsync(CancellationToken cancellationToken);
    /// <summary>按事件 ID 确认移除记录或安排失败重试，避免覆盖同队伍并发写入的新通知。</summary>
    /// <param name="notification">本次分发所读取的发件箱记录。</param>
    /// <param name="retry">是否请求重试；达到尝试上限或截止时间时仍移除记录。</param>
    /// <param name="cancellationToken">取消本次 Redis 操作的令牌。</param>
    Task CompletePeersNotificationAsync(EraPeersNotification notification, bool retry, CancellationToken cancellationToken);
    /// <summary>将通知发布到按 Redis 数据库隔离的频道，供各 API 实例分发；不代表客户端已收到。</summary>
    Task PublishPeersNotificationAsync(EraPeersNotification notification, CancellationToken cancellationToken);
    /// <summary>订阅当前 Redis 数据库的对端通知频道；调用方负责消费消息并取消订阅。</summary>
    Task<ChannelMessageQueue> SubscribePeersNotificationsAsync(CancellationToken cancellationToken);
}

public partial class EraTeamDataProvider
{
    private const string PeersDueKey = "era:peers:outbox:due:v1";
    private const string PeersPayloadKey = "era:peers:outbox:payload:v1";
    private const string PeersChannel = "era:peers:notifications:v1";
    // 普通队伍提交使用 ARGV[6] 入队；原子切队从各队伍描述中读取通知。
    // 两种路径均在成功提交后入队，每队只保留最新失效提示，纯续期不入队。
    private const string PeersCommitFunctions = """
        local function queuePeers(id, message, now)
            if message == '' then return end
            local due = 'era:peers:outbox:due:v1'
            local payload = 'era:peers:outbox:payload:v1'
            local metrics = 'era:peers:metrics:v1'
            if redis.call('HEXISTS', payload, id) == 1 then
                redis.call('HINCRBY', metrics, 'coalesced', 1)
            elseif redis.call('ZCARD', due) >= 10000 then
                local oldest = redis.call('ZRANGE', due, 0, 0)
                if #oldest > 0 then
                    redis.call('ZREM', due, oldest[1])
                    redis.call('HDEL', payload, oldest[1])
                    redis.call('HINCRBY', metrics, 'evicted', 1)
                end
            end
            redis.call('HSET', payload, id, message)
            local scheduled = tonumber(redis.call('ZSCORE', due, id))
            redis.call('ZADD', due, math.min(scheduled or (now + 250), now + 250), id)
            redis.call('HINCRBY', metrics, 'queued', 1)
        end
        local function publishPeers(now)
            queuePeers(ARGV[3], ARGV[6], now)
        end
        """;

    /// <summary>创建两分钟内有效的发件箱记录，使用队伍当前对端版本和主机代次。</summary>
    private static string CreatePeersNotification(EraTeam team)
    {
        var now = DateTimeOffset.UtcNow;
        return JsonSerializer.Serialize(new EraPeersNotification
        {
            TeamId = team.TeamId,
            ExpiresAtUnixMilliseconds = now.AddMinutes(2).ToUnixTimeMilliseconds(),
            Notification = new SocialNotificationDto
            {
                EventId = Guid.NewGuid(),
                EventType = "era.team_peers_changed",
                ResourceId = team.TeamId,
                ResourceVersion = team.PeersVersion,
                AuthorityEpoch = team.AuthorityEpoch,
                OccurredAt = now
            }
        });
    }

    /// <inheritdoc />
    public async Task<List<EraPeersNotification>> GetDuePeersNotificationsAsync(CancellationToken cancellationToken)
    {
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        var ids = await database.SortedSetRangeByScoreAsync(PeersDueKey, double.NegativeInfinity,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), take: 128).WaitAsync(cancellationToken).ConfigureAwait(false);
        if (ids.Length == 0) return new();
        var values = await database.HashGetAsync(PeersPayloadKey, ids).WaitAsync(cancellationToken).ConfigureAwait(false);
        var result = new List<EraPeersNotification>();
        foreach (var value in values)
        {
            if (!value.HasValue) continue;
            var notification = JsonSerializer.Deserialize<EraPeersNotification>(value.ToString());
            if (notification != null) result.Add(notification);
        }
        return result;
    }

    /// <inheritdoc />
    public async Task CompletePeersNotificationAsync(EraPeersNotification notification, bool retry,
        CancellationToken cancellationToken)
    {
        notification.Attempts++;
        bool keep = retry && notification.Attempts < 8 &&
            notification.ExpiresAtUnixMilliseconds > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        const string script = """
            local stored = redis.call('HGET', KEYS[2], ARGV[1])
            if not stored or cjson.decode(stored).Notification.EventId ~= ARGV[2] then return 0 end
            if ARGV[3] == '' then
                redis.call('ZREM', KEYS[1], ARGV[1])
                redis.call('HDEL', KEYS[2], ARGV[1])
            else
                redis.call('HSET', KEYS[2], ARGV[1], ARGV[3])
                redis.call('ZADD', KEYS[1], ARGV[4], ARGV[1])
            end
            return 1
            """;
        await database.ScriptEvaluateAsync(script, new RedisKey[] { PeersDueKey, PeersPayloadKey },
            new RedisValue[] { notification.TeamId.ToString("D"), notification.Notification.EventId.ToString("D"),
                keep ? JsonSerializer.Serialize(notification) : string.Empty,
                DateTimeOffset.UtcNow.AddSeconds(Math.Min(30, Math.Pow(2, notification.Attempts))).ToUnixTimeMilliseconds() })
            .WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task PublishPeersNotificationAsync(EraPeersNotification notification, CancellationToken cancellationToken)
    {
        var connection = await _connection.Value.WaitAsync(cancellationToken).ConfigureAwait(false);
        await connection.GetSubscriber().PublishAsync(RedisChannel.Literal(PeersChannel + ":" + _cacheSetting.RedisDatabaseIndex),
            JsonSerializer.Serialize(notification)).WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ChannelMessageQueue> SubscribePeersNotificationsAsync(CancellationToken cancellationToken)
    {
        var connection = await _connection.Value.WaitAsync(cancellationToken).ConfigureAwait(false);
        return await connection.GetSubscriber().SubscribeAsync(RedisChannel.Literal(PeersChannel + ":" + _cacheSetting.RedisDatabaseIndex))
            .WaitAsync(cancellationToken).ConfigureAwait(false);
    }
}
