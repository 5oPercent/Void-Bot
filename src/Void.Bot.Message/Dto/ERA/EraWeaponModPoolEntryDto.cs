using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponModPoolEntryDto
{
    public Guid ModDefinitionId { get; set; }
    public int Weight { get; set; }
}
