using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>唯一装备本体；位置、归属、角色、分区及已穿槽位必须由未来写操作一起更新。</summary>
[Table("era_equipment_instance")]
[Index(nameof(CharacterId), nameof(EquippedSlot), IsUnique = true, Name = "ux_era_equipment_equipped_slot")]
[Index(nameof(CharacterId), nameof(Location), nameof(InventorySlot), Name = "ix_era_equipment_character_inventory")]
[Index(nameof(OwnerPlayerId), nameof(Location), Name = "ix_era_equipment_owner_location")]
public sealed class EraEquipmentInstance : IEntity<Guid>, IEntityCreated, IEntityModified
{
    [Key]
    public Guid Id { get; set; }
    [StringLength(128)]
    public string DefinitionId { get; set; } = string.Empty;
    /// <summary>首次生成采用的目录版本，展示目录升级不改写该值。</summary>
    public long DefinitionVersion { get; set; }
    public EraEquipmentType EquipmentType { get; set; }
    public EraArmorGrade Grade { get; set; }
    public Guid? OwnerPlayerId { get; set; }
    public Guid? CharacterId { get; set; }
    public EraEquipmentLocation Location { get; set; }
    /// <summary>本件物品唯一背包分区，地面及已穿状态下也保留，不依赖 EquippedSlot 计数。</summary>
    public EraArmorSlot InventorySlot { get; set; }
    /// <summary>仅已穿位置有值，其他状态必须为空。</summary>
    public EraArmorSlot? EquippedSlot { get; set; }
    [ConcurrencyCheck]
    public long Version { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
