using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>仅向有资格的玩家返回的当前可拾取私人或分享装备。</summary>
public sealed class EraEquipmentDropDto
{
    public Guid DropId { get; set; }
    public long DropVersion { get; set; }
    public Guid EquipmentId { get; set; }
    public long EquipmentVersion { get; set; }
    public Guid? PrivateOwnerPlayerId { get; set; }
    public Guid? DroppedByPlayerId { get; set; }
    /// <summary>原分享期限，届满后分享条目仅本次丢出者可见；私人资格不受期限影响。</summary>
    public DateTimeOffset? ShareUntil { get; set; }
    public Guid DefinitionId { get; set; }
    public long DefinitionVersion { get; set; }
    public EraArmorDefinitionDto? Definition { get; set; }
    public EraEquipmentGrade Grade { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int GenerationRuleVersion { get; set; }
    public EraArmorAttributesDto ArmorAttributes { get; set; } = new EraArmorAttributesDto();
    public EraEquipmentDropPlacementDto Placement { get; set; } = new EraEquipmentDropPlacementDto();
    public EraEquipmentType EquipmentType { get; set; } = EraEquipmentType.Armor;
    public EraWeaponInstanceDto? Weapon { get; set; }
    public EraWeaponTemplateDto? WeaponTemplate { get; set; }
}
