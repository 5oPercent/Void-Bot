using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>世界成员记录独立于队伍租约；退出后保留记录，不能静默换角色恢复。</summary>
[Table("era_world_member")]
[PrimaryKey(nameof(WorldId), nameof(PlayerId))]
[Index(nameof(ActivePlayerId), IsUnique = true, Name = "ux_era_world_active_player")]
[Index(nameof(ActiveCharacterId), IsUnique = true, Name = "ux_era_world_active_character")]
public sealed class EraWorldMember : IEntityBase
{
    public Guid WorldId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid CharacterId { get; set; }
    public Guid MemberId { get; set; }
    public bool IsContinuing { get; set; }
    public Guid? ActivePlayerId { get; set; }
    public Guid? ActiveCharacterId { get; set; }
}
