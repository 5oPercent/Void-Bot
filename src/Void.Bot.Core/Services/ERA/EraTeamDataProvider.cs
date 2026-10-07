using System.Runtime.CompilerServices;
using System.Text.Json;
using StackExchange.Redis;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Settings.ERA;
using Void.Bot.Core.Settings.System;
using Void.Bot.Message.Constants.ERA;

namespace Void.Bot.Core.Services.ERA;

public enum EraTeamWriteStatus
{
    TeamCodeAlreadyUsed = -4,
    Expired = -3,
    PlayerAlreadyJoined = -2,
    Conflict = 0,
    Success = 1
}

public class EraTeamWriteException : Exception
{
    public EraTeamWriteStatus Status { get; }

    public EraTeamWriteException(EraTeamWriteStatus status)
        : base("ERA team atomic write rejected.")
    {
        Status = status;
    }
}

/// <summary>业务操作的提交意图。Changed 为 false 时只读，不产生 Redis 写入。</summary>
public class EraTeamChange<T>
{
    public T Result { get; set; } = default!;
    public bool Changed { get; set; }
    public EraTeam? Team { get; set; }
}

public partial interface IEraTeamDataProvider : ISingleton
{
    Task<EraTeam?> GetTeamAsync(Guid teamId, CancellationToken cancellationToken);
    Task<Guid?> GetTeamIdByCodeAsync(string teamCode, CancellationToken cancellationToken);
    Task<Guid?> GetPlayerTeamIdAsync(Guid playerId, CancellationToken cancellationToken);
    Task<EraTeamWriteStatus> TryCreateAsync(EraTeam team, CancellationToken cancellationToken);
    Task<T> ChangeAsync<T>(Guid teamId, Func<EraTeam?, EraTeamChange<T>> operation, CancellationToken cancellationToken);
    Task RemoveExpiredIndexAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<EraTeam> ReadTeamsForCleanupAsync(CancellationToken cancellationToken);
}

/// <summary>
/// 队伍和玩家所属关系存入共享 Redis。按队伍版本比较提交，Lua 同时更新队伍、玩家归属与有效期索引。
/// 不使用进程内锁或内存回退。业务回调可因并发重试，不能执行外部副作用。
/// </summary>
public partial class EraTeamDataProvider : IEraTeamDataProvider, IDisposable, IAsyncDisposable
{
    private const string CommitScript = """
        local stored = redis.call('GET', KEYS[2])
        if ARGV[1] == '' then
            if stored then
                return 0
            end
        else
            if not stored then
                return 0
            end
            local current = cjson.decode(stored)
            if current.StorageVersion ~= ARGV[1] then
                return 0
            end
        end
        local time = redis.call('TIME')
        local now = tonumber(time[1]) * 1000 + math.floor(tonumber(time[2]) / 1000)
        if ARGV[2] == '' then
            redis.call('DEL', KEYS[2])
            redis.call('ZREM', KEYS[1], ARGV[3])
            if redis.call('GET', KEYS[3]) == ARGV[3] then
                redis.call('DEL', KEYS[3])
            end
            for i = 4, #KEYS do
                if redis.call('GET', KEYS[i]) == ARGV[3] then
                    redis.call('DEL', KEYS[i])
                end
            end
            return 1
        end
        local teamExpiry = tonumber(ARGV[4])
        if teamExpiry <= now then
            return -3
        end
        if ARGV[1] == '' then
            redis.call('ZREMRANGEBYSCORE', KEYS[1], '-inf', now)
        end
        local codeOwner = redis.call('GET', KEYS[3])
        if codeOwner and codeOwner ~= ARGV[3] then
            return -4
        end
        local leases = cjson.decode(ARGV[5])
        for _, lease in ipairs(leases) do
            local expiry = math.min(lease.ExpiresAt, teamExpiry)
            if expiry > now then
                local previous = redis.call('GET', KEYS[lease.KeyIndex])
                if previous and previous ~= ARGV[3] then
                    return -2
                end
            end
        end
        redis.call('SET', KEYS[2], ARGV[2], 'PXAT', teamExpiry)
        redis.call('SET', KEYS[3], ARGV[3], 'PXAT', teamExpiry)
        redis.call('ZADD', KEYS[1], teamExpiry, ARGV[3])
        for i = 4, #KEYS do
            if redis.call('GET', KEYS[i]) == ARGV[3] then
                redis.call('DEL', KEYS[i])
            end
        end
        for _, lease in ipairs(leases) do
            local expiry = math.min(lease.ExpiresAt, teamExpiry)
            if expiry > now then
                redis.call('SET', KEYS[lease.KeyIndex], ARGV[3], 'PXAT', expiry)
            end
        end
        return 1
        """;

    private const string CleanupScript = """
        local time = redis.call('TIME')
        local now = tonumber(time[1]) * 1000 + math.floor(tonumber(time[2]) / 1000)
        return redis.call('ZREMRANGEBYSCORE', KEYS[1], '-inf', now)
        """;

    private readonly CacheSetting _cacheSetting;
    private readonly Lazy<Task<ConnectionMultiplexer>> _connection;
    private ConnectionMultiplexer? _multiplexer;
    private int _disposed;

    public EraTeamDataProvider(CacheSetting cacheSetting, EraNetworkingSetting networkingSetting)
    {
        _cacheSetting = cacheSetting;
        if (networkingSetting.Enabled && string.IsNullOrWhiteSpace(cacheSetting.RedisConnection))
        {
            throw new InvalidOperationException("ERA networking requires ConnectionStrings:Redis.");
        }

        // 每个后端进程复用一条 ERA 连接，关闭服务时不连接 Redis；与普通缓存的存储选择无关。
        _connection = new Lazy<Task<ConnectionMultiplexer>>(ConnectAsync);
    }

    private async Task<ConnectionMultiplexer> ConnectAsync()
    {
        var options = ConfigurationOptions.Parse(_cacheSetting.RedisConnection);
        // Redis 暂时离线时保留后台重连能力，不把第一次失败永久缓存为故障 Task。
        options.AbortOnConnectFail = false;
        var connection = await ConnectionMultiplexer.ConnectAsync(options).ConfigureAwait(false);
        _multiplexer = connection;
        if (Volatile.Read(ref _disposed) != 0)
        {
            Interlocked.Exchange(ref _multiplexer, null)?.Dispose();
            throw new ObjectDisposedException(nameof(EraTeamDataProvider));
        }

        return connection;
    }

    private async Task<IDatabase> GetDatabaseAsync(CancellationToken cancellationToken)
    {
        var connection = await _connection.Value.WaitAsync(cancellationToken).ConfigureAwait(false);
        return connection.GetDatabase(_cacheSetting.RedisDatabaseIndex);
    }

    private static RedisKey TeamKey(Guid teamId)
    {
        return EraNetworkingConstants.RedisTeamPrefix + teamId.ToString("D");
    }

    private static RedisKey PlayerKey(Guid playerId)
    {
        return EraNetworkingConstants.RedisPlayerPrefix + playerId.ToString("D");
    }

    public async Task<EraTeam?> GetTeamAsync(Guid teamId, CancellationToken cancellationToken)
    {
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        var value = await database.StringGetAsync(TeamKey(teamId)).WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!value.HasValue)
        {
            return null;
        }

        return JsonSerializer.Deserialize<EraTeam>(value.ToString());
    }

    public async Task<Guid?> GetTeamIdByCodeAsync(string teamCode, CancellationToken cancellationToken)
    {
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        var key = EraNetworkingConstants.RedisTeamCodePrefix + teamCode;
        var value = await database.StringGetAsync(key).WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!value.HasValue)
        {
            return null;
        }
        return Guid.Parse(value.ToString());
    }

    public async Task<Guid?> GetPlayerTeamIdAsync(Guid playerId, CancellationToken cancellationToken)
    {
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        var value = await database.StringGetAsync(PlayerKey(playerId)).WaitAsync(cancellationToken).ConfigureAwait(false);
        if (!value.HasValue)
        {
            return null;
        }

        return Guid.Parse(value.ToString());
    }

    public async Task<EraTeamWriteStatus> TryCreateAsync(EraTeam team, CancellationToken cancellationToken)
    {
        team.StorageVersion = Guid.NewGuid();
        return await CommitAsync(team.TeamId, team.TeamCode, Guid.Empty, team, Array.Empty<Guid>(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<T> ChangeAsync<T>(
        Guid teamId,
        Func<EraTeam?, EraTeamChange<T>> operation,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 8; attempt++)
        {
            var team = await GetTeamAsync(teamId, cancellationToken).ConfigureAwait(false);
            var version = Guid.Empty;
            var previousPlayers = Array.Empty<Guid>();
            if (team != null)
            {
                version = team.StorageVersion;
                previousPlayers = team.Members.Select(member => member.PlayerId).Distinct().ToArray();
            }

            var teamCode = team?.TeamCode ?? string.Empty;
            var change = operation(team);
            if (!change.Changed)
            {
                return change.Result;
            }

            if (team == null)
            {
                throw new InvalidOperationException("Team changes cannot create a missing team.");
            }

            if (change.Team != null)
            {
                change.Team.StorageVersion = Guid.NewGuid();
            }

            var status = await CommitAsync(teamId, teamCode, version, change.Team, previousPlayers, cancellationToken).ConfigureAwait(false);
            if (status == EraTeamWriteStatus.Success)
            {
                return change.Result;
            }

            if (status != EraTeamWriteStatus.Conflict)
            {
                throw new EraTeamWriteException(status);
            }

            await Task.Delay(Random.Shared.Next(5, 21), cancellationToken).ConfigureAwait(false);
        }

        throw new EraTeamWriteException(EraTeamWriteStatus.Conflict);
    }

    private async Task<EraTeamWriteStatus> CommitAsync(
        Guid teamId,
        string teamCode,
        Guid expectedVersion,
        EraTeam? team,
        IReadOnlyCollection<Guid> previousPlayers,
        CancellationToken cancellationToken)
    {
        var allPlayers = previousPlayers.ToList();
        if (team != null)
        {
            allPlayers.AddRange(team.Members.Select(member => member.PlayerId));
        }

        allPlayers = allPlayers.Distinct().ToList();
        var keys = new List<RedisKey>
        {
            EraNetworkingConstants.RedisTeamIndex,
            TeamKey(teamId),
            EraNetworkingConstants.RedisTeamCodePrefix + teamCode
        };
        keys.AddRange(allPlayers.Select(PlayerKey));
        var leases = new List<PlayerLease>();
        var teamExpiration = 0L;
        var json = string.Empty;
        if (team != null)
        {
            // 主机失效不删除仍有有效客户端的队伍；至少一位有效成员即可维持队伍 TTL。
            var expiration = team.ExpiresAt;
            var lastMemberExpiration = team.Members.Max(member => member.ExpiresAt);
            if (lastMemberExpiration < expiration)
            {
                expiration = lastMemberExpiration;
            }

            teamExpiration = expiration.ToUnixTimeMilliseconds();
            json = JsonSerializer.Serialize(team);
            foreach (var member in team.Members)
            {
                leases.Add(new PlayerLease
                {
                    KeyIndex = allPlayers.IndexOf(member.PlayerId) + 4,
                    ExpiresAt = member.ExpiresAt.ToUnixTimeMilliseconds()
                });
            }
        }

        var expected = string.Empty;
        if (expectedVersion != Guid.Empty)
        {
            expected = expectedVersion.ToString("D");
        }

        var values = new RedisValue[]
        {
            expected,
            json,
            teamId.ToString("D"),
            teamExpiration,
            JsonSerializer.Serialize(leases)
        };
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        // 发送后等待提交结果，不用取消等待把已执行的写入误判为未提交。
        var result = await database.ScriptEvaluateAsync(CommitScript, keys.ToArray(), values).ConfigureAwait(false);
        return (EraTeamWriteStatus)(int)result;
    }

    public async Task RemoveExpiredIndexAsync(CancellationToken cancellationToken)
    {
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        await database.ScriptEvaluateAsync(CleanupScript,
            new RedisKey[]
            {
                EraNetworkingConstants.RedisTeamIndex
            }, Array.Empty<RedisValue>()).ConfigureAwait(false);
    }

    /// <summary>分批只读扫描有效队伍；后台仅对发现过期成员的队伍执行原子修改。</summary>
    public async IAsyncEnumerable<EraTeam> ReadTeamsForCleanupAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const int batchSize = 128;
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        for (var offset = 0; ; offset += batchSize)
        {
            var ids = await database.SortedSetRangeByRankAsync(EraNetworkingConstants.RedisTeamIndex,
                offset, offset + batchSize - 1).WaitAsync(cancellationToken).ConfigureAwait(false);
            if (ids.Length == 0)
            {
                yield break;
            }

            var keys = ids.Select(id => TeamKey(Guid.Parse(id.ToString()))).ToArray();
            var values = await database.StringGetAsync(keys).WaitAsync(cancellationToken).ConfigureAwait(false);
            foreach (var value in values)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!value.HasValue)
                {
                    continue;
                }

                var team = JsonSerializer.Deserialize<EraTeam>(value.ToString());
                if (team != null)
                {
                    yield return team;
                }
            }

            if (ids.Length < batchSize)
            {
                yield break;
            }
        }
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref _disposed, 1);
        Interlocked.Exchange(ref _multiplexer, null)?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        Interlocked.Exchange(ref _disposed, 1);
        var connection = Interlocked.Exchange(ref _multiplexer, null);
        if (connection != null)
        {
            try
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
            finally
            {
                connection.Dispose();
            }
        }
    }

    private class PlayerLease
    {
        public int KeyIndex { get; set; }
        public long ExpiresAt { get; set; }
    }
}
