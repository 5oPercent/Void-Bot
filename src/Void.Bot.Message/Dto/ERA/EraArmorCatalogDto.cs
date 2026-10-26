using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraArmorCatalogDto
{
    public List<EraArmorSlotTypeDto> SlotTypes { get; set; } = new();
    public List<EraArmorSlotConfigurationDto> SlotConfigurations { get; set; } = new();
    public List<EraArmorTemplateDto> Templates { get; set; } = new List<EraArmorTemplateDto>();
    public List<EraArmorModDefinitionDto> Modifications { get; set; } = new List<EraArmorModDefinitionDto>();
}
