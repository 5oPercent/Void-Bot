namespace Void.Bot.Core.Domain.ERA;

/// <summary>Redis 临时会话，不作为数据库实体；通过版本比较原子提交。</summary>
public class EraTeam
{
    /// <summary>Redis 乐观并发版本，每次原子提交更新；不作为客户端业务版本。</summary>
    public Guid StorageVersion { get; set; }
    public Guid TeamId { get; set; }
    /// <summary>玩家分享和加入使用的八位队伍码。</summary>
    public string TeamCode { get; set; } = string.Empty;
    public Guid HostPlayerId { get; set; }
    public long AuthorityEpoch { get; set; } = 1;
    public bool IsMigrating { get; set; }
    public DateTimeOffset MigrationDeadline { get; set; }
    public List<Guid> FailedHostMembers { get; set; } = new List<Guid>();
    public Guid CreationRequestId { get; set; }
    public int MaxMembers { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public List<EraTeamMember> Members { get; set; } = new List<EraTeamMember>();
}

public class EraTeamMember
{
    public Guid MemberId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid CharacterId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
    /// <summary>客户端已保存可接管的完整状态后报告；新加入成员默认不可接管。</summary>
    public bool CanHost { get; set; }
    public long TicketAuthorityEpoch { get; set; }
    public Guid AttemptId { get; set; }
    // 只保存随机凭证的 SHA256，不保留或记录明文。
    public byte[] TokenHash { get; set; } = Array.Empty<byte>();
    public DateTimeOffset TicketExpiresAt { get; set; }
    public string? Address { get; set; }
    public int Port { get; set; }
    public DateTimeOffset EndpointExpiresAt { get; set; }
}
