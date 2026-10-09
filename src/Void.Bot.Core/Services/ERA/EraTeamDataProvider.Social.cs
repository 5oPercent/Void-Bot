using System.Text.Json;
using StackExchange.Redis;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Dto.Social;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA;

public class EraQueuedNotification
{
    public string QueueId { get; set; } = string.Empty;
    public Guid RecipientAccountId { get; set; }
    public int Attempts { get; set; }
    public SocialNotificationDto Notification { get; set; } = new SocialNotificationDto();
}

public partial interface IEraTeamDataProvider
{
    Task<List<Guid>> GetInvitationTeamIdsAsync(Guid accountId, CancellationToken cancellationToken);
    Task<List<EraQueuedNotification>> GetDueSocialNotificationsAsync(CancellationToken cancellationToken);
    Task CompleteSocialNotificationAsync(string id, CancellationToken cancellationToken);
    Task RetrySocialNotificationAsync(EraQueuedNotification notification, CancellationToken cancellationToken);
}

public partial class EraTeamDataProvider
{
    // 与既有队伍写入脚本组合，邀请、索引和通知在同一次 Lua 提交中变更。
    // 当前部署为单 Redis 实例，不支持 Redis Cluster 的跨槽脚本。
    private const string SocialCommitFunctions = """
        local socialEventId = ARGV[#ARGV]
        local function validSocialTransition(before, after, now)
            local previous = {}
            if before and before.Invitations then
                for _, invitation in ipairs(before.Invitations) do
                    previous[invitation.Id] = invitation
                end
            end
            if after and after.Invitations then
                for _, invitation in ipairs(after.Invitations) do
                    local old = previous[invitation.Id]
                    if invitation.Status == 1 and (not old or old.Status ~= 1) then
                        if not old or old.Status ~= 0 or old.ExpiresAtUnixMilliseconds <= now then
                            return false
                        end
                    end
                end
            end
            return true
        end

        local function queueInvitation(invitation, teamId, recipient, status, now)
            local id = invitation.Id .. ':' .. tostring(invitation.Version) .. ':' .. recipient .. ':' .. tostring(status)
            local occurredAt = invitation.CreatedAt
            if type(invitation.ProcessedAt) == 'string' then
                occurredAt = invitation.ProcessedAt
            end
            local version = invitation.Version
            if status == 5 and invitation.Status == 0 then
                version = version + 1
            end
            local notification = {
                EventId = string.sub(socialEventId, 1, 8) .. string.sub(invitation.Id, 9),
                EventType = 'era.team_invitation_changed',
                ResourceId = invitation.Id,
                ParentResourceId = teamId,
                ResourceVersion = version,
                CorrelationId = invitation.RequestId,
                OccurredAt = ARGV[#ARGV - 1]
            }
            local envelope = {
                QueueId = id,
                RecipientAccountId = recipient,
                Attempts = 0,
                Notification = notification
            }
            local pending = 'social:era:outbox:due:v1'
            local payload = 'social:era:outbox:payload:v1'
            if redis.call('ZCARD', pending) >= 10000 then
                local oldest = redis.call('ZRANGE', pending, 0, 0)
                if #oldest > 0 then
                    redis.call('ZREM', pending, oldest[1])
                    redis.call('HDEL', payload, oldest[1])
                end
            end
            redis.call('HSET', payload, id, cjson.encode(envelope))
            redis.call('ZADD', pending, now, id)
            local index = 'social:era:invitations:v1:' .. recipient
            local score = invitation.ExpiresAtUnixMilliseconds + 600000
            local previousScore = redis.call('ZSCORE', index, teamId)
            if not previousScore or score > tonumber(previousScore) then
                redis.call('ZADD', index, score, teamId)
            end
            redis.call('EXPIRE', index, 720)
        end

        local function publishInvitations(before, after, teamId, now)
            local previous = {}
            if before and before.Invitations then
                for _, invitation in ipairs(before.Invitations) do
                    previous[invitation.Id] = invitation
                end
            end
            if after and after.Invitations then
                for _, invitation in ipairs(after.Invitations) do
                    local old = previous[invitation.Id]
                    if not old or old.Version ~= invitation.Version then
                        queueInvitation(invitation, teamId, invitation.RecipientAccountId, invitation.Status, now)
                        queueInvitation(invitation, teamId, invitation.SenderAccountId, invitation.Status, now)
                    end
                    previous[invitation.Id] = nil
                end
            end
            for _, invitation in pairs(previous) do
                if invitation.Status == 0 then
                    queueInvitation(invitation, teamId, invitation.RecipientAccountId, 5, now)
                    queueInvitation(invitation, teamId, invitation.SenderAccountId, 5, now)
                end
            end
        end
        """;

    private static void PrepareInvitations(EraTeam team)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var invitation in team.Invitations.Where(value => value.Status == EraTeamInvitationStatus.Pending))
        {
            var sender = team.Members.SingleOrDefault(value => value.MemberId == invitation.SenderMemberId && value.ExpiresAt > now);
            EraTeamInvitationStatus? status = null;
            if (invitation.ExpiresAt <= now)
            {
                status = EraTeamInvitationStatus.Expired;
            }
            else if (team.JoinMode == EraTeamJoinMode.Solo || sender == null ||
                (team.InvitationPermission == EraTeamInvitationPermission.HostOnly && sender.PlayerId != team.HostPlayerId))
            {
                status = EraTeamInvitationStatus.Invalidated;
            }
            if (status.HasValue)
            {
                invitation.Status = status.Value;
                invitation.ProcessedAt = now;
                invitation.Version++;
            }
        }
        team.Invitations.RemoveAll(value => value.ExpiresAt.AddMinutes(10) <= now);
    }

    public async Task<List<Guid>> GetInvitationTeamIdsAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        var key = "social:era:invitations:v1:" + accountId.ToString("D");
        await database.SortedSetRemoveRangeByScoreAsync(key, double.NegativeInfinity,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).WaitAsync(cancellationToken).ConfigureAwait(false);
        var ids = await database.SortedSetRangeByRankAsync(key, 0, 99, Order.Descending)
            .WaitAsync(cancellationToken).ConfigureAwait(false);
        return ids.Select(value => Guid.Parse(value.ToString())).Distinct().ToList();
    }

    public async Task<List<EraQueuedNotification>> GetDueSocialNotificationsAsync(CancellationToken cancellationToken)
    {
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        var ids = await database.SortedSetRangeByScoreAsync("social:era:outbox:due:v1",
            double.NegativeInfinity, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), take: 128)
            .WaitAsync(cancellationToken).ConfigureAwait(false);
        if (ids.Length == 0)
        {
            return new List<EraQueuedNotification>();
        }
        var values = await database.HashGetAsync("social:era:outbox:payload:v1", ids)
            .WaitAsync(cancellationToken).ConfigureAwait(false);
        var result = new List<EraQueuedNotification>();
        for (var index = 0; index < values.Length; index++)
        {
            if (!values[index].HasValue)
            {
                await CompleteSocialNotificationAsync(ids[index].ToString(), cancellationToken).ConfigureAwait(false);
                continue;
            }
            var notification = JsonSerializer.Deserialize<EraQueuedNotification>(values[index].ToString());
            if (notification != null)
            {
                result.Add(notification);
            }
        }
        return result;
    }

    public async Task CompleteSocialNotificationAsync(string id, CancellationToken cancellationToken)
    {
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        const string script = "redis.call('ZREM', KEYS[1], ARGV[1]); redis.call('HDEL', KEYS[2], ARGV[1]); return 1";
        await database.ScriptEvaluateAsync(script,
            new RedisKey[] { "social:era:outbox:due:v1", "social:era:outbox:payload:v1" },
            new RedisValue[] { id }).WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RetrySocialNotificationAsync(EraQueuedNotification notification, CancellationToken cancellationToken)
    {
        notification.Attempts++;
        if (notification.Attempts >= 8)
        {
            await CompleteSocialNotificationAsync(notification.QueueId, cancellationToken).ConfigureAwait(false);
            return;
        }
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        const string script = """
            if redis.call('HEXISTS', KEYS[2], ARGV[1]) == 1 then
                redis.call('HSET', KEYS[2], ARGV[1], ARGV[2])
                redis.call('ZADD', KEYS[1], ARGV[3], ARGV[1])
            end
            return 1
            """;
        var next = DateTimeOffset.UtcNow.AddSeconds(Math.Min(300, Math.Pow(2, notification.Attempts)));
        await database.ScriptEvaluateAsync(script,
            new RedisKey[] { "social:era:outbox:due:v1", "social:era:outbox:payload:v1" },
            new RedisValue[] { notification.QueueId, JsonSerializer.Serialize(notification), next.ToUnixTimeMilliseconds() })
            .WaitAsync(cancellationToken).ConfigureAwait(false);
    }
}
