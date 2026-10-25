using System.Text.Json.Serialization;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponTemplateDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public bool IsGenerationEnabled { get; set; } = true;
    public string PresentationKey { get; set; } = string.Empty;
    public Guid StatMappingId { get; set; }
    public EraWeaponStatsDto BaseStats { get; set; } = new EraWeaponStatsDto();
    public EraWeaponBehaviorDto Behavior { get; set; } = new EraWeaponBehaviorDto();
    public List<EraEquipmentSlot> CompatibleSlots { get; set; } = new List<EraEquipmentSlot>();
    public Guid SlotConfigurationId { get; set; }
}
