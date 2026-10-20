using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Models;

public sealed class EraPlannedRewardEquipment
{
    public Guid EquipmentId { get; set; }
    public string DefinitionId { get; set; } = string.Empty;
    public EraArmorGrade Grade { get; set; }
    public EraArmorSlot InventorySlot { get; set; }
    public int GenerationRuleVersion { get; set; }
    public EraArmorAttributesDto Attributes { get; set; } = new EraArmorAttributesDto();
}
