using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>与队伍一起进行 Redis CAS 提交；不是数据库实体。</summary>
public class EraTeamInvitation
{
    public Guid Id { get; set; }
    public Guid FriendshipId { get; set; }
    public Guid RequestId { get; set; }
    public Guid SenderAccountId { get; set; }
    public Guid RecipientAccountId { get; set; }
    public Guid SenderMemberId { get; set; }
    public Guid AcceptedMemberId { get; set; }
    public Guid AcceptedCharacterId { get; set; }
    public EraTeamInvitationStatus Status { get; set; }
    public long Version { get; set; } = 1;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public long ExpiresAtUnixMilliseconds
    {
        get
        {
            return ExpiresAt.ToUnixTimeMilliseconds();
        }
    }
}
