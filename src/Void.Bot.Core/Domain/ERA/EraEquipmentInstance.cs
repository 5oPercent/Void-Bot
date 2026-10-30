using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>唯一装备本体；位置、归属、角色及已穿槽位在同一资产事务内更新。</summary>
[Table("era_equipment_instance")]
public sealed class EraEquipmentInstance : IEntity<Guid>, IEntityCreated, IEntityModified
{
    [Key]
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    /// <summary>首次生成采用的目录版本，展示目录升级不改写该值。</summary>
    public long GenerationManifestVersion { get; set; }
    public EraEquipmentType EquipmentType { get; set; }
    public EraEquipmentGrade Grade { get; set; }
    public Guid? OwnerPlayerId { get; set; }
    public Guid? CharacterId { get; set; }
    public EraEquipmentLocation Location { get; set; }
    /// <summary>仅已穿位置有值，其他状态必须为空。</summary>
    public EraEquipmentSlot? EquippedSlot { get; set; }
    [ConcurrencyCheck]
    public long Version { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
