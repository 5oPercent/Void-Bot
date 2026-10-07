using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>队伍状态，不包含主账号 ID 或成员凭证。</summary>
public class EraTeamDto
{
    public Guid TeamId { get; set; }
    /// <summary>玩家分享和加入使用的八位队伍码。</summary>
    public string TeamCode { get; set; } = string.Empty;
    public Guid HostPlayerId { get; set; }
    public long AuthorityEpoch { get; set; }
    public bool IsMigrating { get; set; }
    public DateTimeOffset MigrationDeadline { get; set; }
    public int HeartbeatIntervalSeconds { get; set; }
    public int MemberLifetimeSeconds { get; set; }
    public int MaxMembers { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public List<EraTeamMemberDto> Members { get; set; } = [];
}

public class EraTeamMemberDto
{
    public Guid MemberId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid CharacterId { get; set; }
    /// <summary>来自已认证主账号的显示名称；后缀单独返回用于悬停提示。</summary>
    public string UserName { get; set; } = string.Empty;
    public string Suffix { get; set; } = string.Empty;

    public bool CanHost { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}

/// <summary>仅返回本人短期登记凭证，通过原 ENet Socket 登记。</summary>
public class EraPunchTicketDto
{
    public Guid TeamId { get; set; }
    public Guid MemberId { get; set; }
    public Guid AttemptId { get; set; }
    public long AuthorityEpoch { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public string UdpHost { get; set; } = string.Empty;
    public int UdpPort { get; set; }
}

public class EraPunchPeersDto
{
    public Guid TeamId { get; set; }
    public Guid MemberId { get; set; }
    public Guid AttemptId { get; set; }
    public long AuthorityEpoch { get; set; }
    public EraPunchState State { get; set; }
    public DateTimeOffset TicketExpiresAt { get; set; }
    public List<EraPunchPeerDto> Peers { get; set; } = [];
}

public class EraPunchPeerDto
{
    public Guid MemberId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid AttemptId { get; set; }
    public long AuthorityEpoch { get; set; }
    public bool IsHost { get; set; }
    /// <summary>实际游戏 UDP Socket 的局域网 IPv4 候选地址，不包含公网地址。</summary>
    public List<string> LocalAddresses { get; set; } = [];
    public int LocalPort { get; set; }

    public string Address { get; set; } = string.Empty;
    public int Port { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
