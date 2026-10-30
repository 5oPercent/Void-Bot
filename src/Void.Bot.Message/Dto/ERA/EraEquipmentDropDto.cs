using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>仅向有资格的玩家返回的当前可拾取私人或分享装备。</summary>
public sealed class EraEquipmentDropDto
{
    public Guid DropId { get; set; }
    public long DropVersion { get; set; }
    public Guid EquipmentInstanceId { get; set; }
    public long EquipmentVersion { get; set; }
    public Guid? PrivateOwnerPlayerId { get; set; }
    public Guid? DroppedByPlayerId { get; set; }
    /// <summary>原分享期限，届满后分享条目仅本次丢出者可见；私人资格不受期限影响。</summary>
    public DateTimeOffset? ShareUntil { get; set; }
    /// <summary>生成时固定的分享期限和原名单，名称仅供展示，不表示当前地面拾取资格。</summary>
    public EraEquipmentShareDto? Share { get; set; }
    public Guid TemplateId { get; set; }
    public long GenerationManifestVersion { get; set; }
    public EraArmorTemplateDto? ArmorTemplate { get; set; }
    /// <summary>装备品阶；护甲用于确定随机六维预算，武器暂无品阶专属生成规则。</summary>
    public EraEquipmentGrade Grade { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int GenerationRuleVersion { get; set; }
    public EraArmorAttributesDto ArmorAttributes { get; set; } = new EraArmorAttributesDto();
    public EraEquipmentDropPlacementDto Placement { get; set; } = new EraEquipmentDropPlacementDto();
    /// <summary>装备本体类型，决定使用护甲或武器实例载荷。</summary>
    public EraEquipmentType EquipmentType { get; set; } = EraEquipmentType.Armor;
    public EraArmorInstanceDto? ArmorModifications { get; set; }
    public EraArmorAttributesDto? ArmorEffectiveAttributes { get; set; }
    public EraWeaponInstanceDto? Weapon { get; set; }
    public EraWeaponTemplateDto? WeaponTemplate { get; set; }
}
