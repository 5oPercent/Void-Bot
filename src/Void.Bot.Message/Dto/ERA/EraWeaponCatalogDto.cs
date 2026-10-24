using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponCatalogDto
{
    public List<EraWeaponStatMappingDto> StatMappings { get; set; } = new List<EraWeaponStatMappingDto>();
    public List<EraWeaponTemplateDto> Templates { get; set; } = new List<EraWeaponTemplateDto>();
    public List<EraWeaponModDefinitionDto> Modifications { get; set; } = new List<EraWeaponModDefinitionDto>();
}
