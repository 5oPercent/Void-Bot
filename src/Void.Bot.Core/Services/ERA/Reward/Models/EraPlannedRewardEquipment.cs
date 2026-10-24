using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Reward.Models;

public sealed class EraPlannedRewardEquipment
{
    public Guid EquipmentId { get; set; }
    public Guid DefinitionId { get; set; }
    public EraEquipmentGrade Grade { get; set; }
    public int GenerationRuleVersion { get; set; }
    public EraArmorAttributesDto Attributes { get; set; } = new EraArmorAttributesDto();
    public EraEquipmentType EquipmentType { get; set; } = EraEquipmentType.Armor;
    public EraWeaponInstanceDto? Weapon { get; set; }
}
