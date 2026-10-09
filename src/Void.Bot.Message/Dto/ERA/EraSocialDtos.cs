using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

public class EraFriendStatusDto
{
    public Guid AccountId { get; set; }
    public bool IsOnline { get; set; }
    public EraFriendTeamDto? Team { get; set; }
}

/// <summary>好友查询只返回摘要，不包含队伍码、端点和联机凭证。</summary>
public class EraFriendTeamDto
{
    public Guid TeamId { get; set; }
    public int MemberCount { get; set; }
    public int MaxMembers { get; set; }
    public EraTeamJoinMode JoinMode { get; set; }
    public bool CanJoin { get; set; }
    public string? UnavailableReason { get; set; }
}

public class EraTeamInvitationDto
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public Guid SenderAccountId { get; set; }
    public Guid RecipientAccountId { get; set; }
    public EraTeamInvitationStatus Status { get; set; }
    public long Version { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
