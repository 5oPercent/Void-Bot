using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponCatalogDto
{
    public List<EraWeaponStatEffectProfileDto> StatEffectProfiles { get; set; } = new List<EraWeaponStatEffectProfileDto>();
    public List<EraWeaponTemplateDto> Templates { get; set; } = new List<EraWeaponTemplateDto>();
    public List<EraWeaponModDefinitionDto> Modifications { get; set; } = new List<EraWeaponModDefinitionDto>();
}
