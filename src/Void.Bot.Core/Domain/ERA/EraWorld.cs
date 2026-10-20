using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_world")]
[Index(nameof(TeamId), nameof(CreationRequestId), IsUnique = true, Name = "ux_era_world_creation_request")]
[Index(nameof(State), nameof(LastLifetimeCheckAt), Name = "ix_era_world_lifetime_check")]
public sealed class EraWorld : IEntity<Guid>, IEntityCreated
{
    [Key]
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public Guid CreationRequestId { get; set; }
    public Guid CreatedByPlayerId { get; set; }
    public Guid CreationMemberId { get; set; }
    public long CreationAuthorityEpoch { get; set; }
    [StringLength(128)]
    public string WorldDefinitionId { get; set; } = string.Empty;
    public EraWorldState State { get; set; }
    public Guid HostPlayerId { get; set; }
    public long AuthorityEpoch { get; set; }
    [ConcurrencyCheck]
    public long Version { get; set; }
    public bool IsEquippedOperationsRestricted { get; set; }
    public long RestrictionVersion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    /// <summary>服务器最近确认活动；心跳及成功权限操作可续期，不递增业务版本。</summary>
    public DateTimeOffset LastActiveAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLifetimeCheckAt { get; set; }
    public DateTimeOffset? TimeoutObservedAt { get; set; }
    public EraWorldEndReason? EndReason { get; set; }
}
