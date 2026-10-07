using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA;

/// <summary>纯状态转换，仅在 Redis CAS 回调中调用；不能执行网络或数据库操作。</summary>
public static class EraTeamLifecycle
{
    public static bool Advance(EraTeam team, DateTimeOffset now, int timeoutSeconds)
    {
        team.Members.RemoveAll(member => member.ExpiresAt <= now);
        if (team.Members.Count == 0)
        {
            return false;
        }
        var host = team.Members.SingleOrDefault(member => member.PlayerId == team.HostPlayerId);
        if (host != null && (!team.IsMigrating || team.MigrationDeadline > now))
        {
            return true;
        }
        return SelectNext(team, now, timeoutSeconds);
    }

    public static bool SelectNext(EraTeam team, DateTimeOffset now, int timeoutSeconds)
    {
        var previous = team.Members.SingleOrDefault(member => member.PlayerId == team.HostPlayerId);
        if (previous != null && !team.FailedHostMembers.Contains(previous.MemberId))
        {
            team.FailedHostMembers.Add(previous.MemberId);
        }
        var candidate = team.Members
            .Where(member => member.ExpiresAt > now && member.CanHost && !team.FailedHostMembers.Contains(member.MemberId))
            .OrderBy(member => member.JoinedAt)
            .ThenBy(member => member.MemberId)
            .FirstOrDefault();
        if (candidate == null)
        {
            // 所有候补均不可接管或已经失败，整个队伍解散。
            return false;
        }
        team.HostPlayerId = candidate.PlayerId;
        team.AuthorityEpoch = checked(team.AuthorityEpoch + 1);
        team.IsMigrating = true;
        team.MigrationDeadline = now.AddSeconds(timeoutSeconds);
        foreach (var member in team.Members)
        {
            member.AttemptId = Guid.Empty;
            member.TokenHash = Array.Empty<byte>();
            member.TicketExpiresAt = default;
            member.Address = null;
            member.Port = 0;
            member.EndpointExpiresAt = default;
        }
        return true;
    }
}
