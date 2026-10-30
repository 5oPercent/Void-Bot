using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_equipment_drop")]
[Index(nameof(ActiveEquipmentInstanceId), IsUnique = true, Name = "ux_era_equipment_drop_active")]
[Index(nameof(WorldInstanceId), nameof(PrivateOwnerPlayerId), nameof(Status), Name = "ix_era_equipment_drop_visible")]
public sealed class EraEquipmentDrop : IEntity<Guid>, IEntityCreated, IEntityModified
{
    [Key]
    public Guid Id { get; set; }
    public Guid EquipmentInstanceId { get; set; }
    /// <summary>仅可拾取状态填写，并与 EquipmentInstanceId 相同；终态清空以保留历史。</summary>
    public Guid? ActiveEquipmentInstanceId { get; set; }
    public Guid WorldInstanceId { get; set; }
    public Guid? PrivateOwnerPlayerId { get; set; }
    public Guid? DroppedByPlayerId { get; set; }
    public EraEquipmentDropStatus Status { get; set; }
    [ConcurrencyCheck]
    public long Version { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double YawRadians { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
