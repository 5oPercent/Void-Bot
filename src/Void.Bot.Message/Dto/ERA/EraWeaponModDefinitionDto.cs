using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponModDefinitionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid SlotTypeId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public List<EraWeaponModifierDto> Modifiers { get; set; } = new List<EraWeaponModifierDto>();
}
