using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Reward.Models;

public sealed class EraPlannedRewardEquipment
{
    public Guid EquipmentId { get; set; }
    public Guid DefinitionId { get; set; }
    /// <summary>装备品阶；护甲用于确定随机六维预算，武器暂无品阶专属生成规则。</summary>
    public EraEquipmentGrade Grade { get; set; }
    public int GenerationRuleVersion { get; set; }
    public EraArmorAttributesDto Attributes { get; set; } = new EraArmorAttributesDto();
    /// <summary>装备本体类型，决定使用护甲或武器实例载荷。</summary>
    public EraEquipmentType EquipmentType { get; set; } = EraEquipmentType.Armor;
    public EraArmorInstanceDto ArmorModifications { get; set; } = new();
    public EraWeaponInstanceDto? Weapon { get; set; }
}
