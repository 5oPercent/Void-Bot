using System.Security.Cryptography;
using System.Text.Json;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA;

/// <summary>对比连接依据和时间导致的有效性转换，排除纯续期及凭证内容。</summary>
public static class EraPeersState
{
    /// <summary>重新计算对端状态摘要；发生变化时更新摘要并递增版本，不执行持久化或通知投递。</summary>
    /// <param name="team">需要更新对端状态摘要的队伍对象。</param>
    /// <param name="now">判断成员、票据及端点有效性的服务端时间。</param>
    /// <returns>摘要发生变化返回 true；只有租约续期且有效性不变时返回 false。</returns>
    public static bool Refresh(EraTeam team, DateTimeOffset now)
    {
        var state = new
        {
            team.HostPlayerId,
            team.AuthorityEpoch,
            team.IsMigrating,
            Members = team.Members.Where(member => member.ExpiresAt > now)
                .OrderBy(member => member.MemberId).Select(member =>
                {
                    bool ticketValid = member.AttemptId != Guid.Empty && member.TicketExpiresAt > now;
                    bool endpointValid = ticketValid && member.Address != null && member.EndpointExpiresAt > now;
                    return new
                    {
                        member.MemberId,
                        member.PlayerId,
                        member.AttemptId,
                        TicketValid = ticketValid,
                        EndpointValid = endpointValid,
                        Address = endpointValid ? member.Address : null,
                        Port = endpointValid ? member.Port : 0,
                        LocalAddresses = member.LocalAddresses.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                        member.LocalPort
                    };
                }).ToArray()
        };
        var fingerprint = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(state)));
        if (team.PeersFingerprint == fingerprint) return false;
        team.PeersFingerprint = fingerprint;
        team.PeersVersion = checked(team.PeersVersion + 1);
        return true;
    }
}
