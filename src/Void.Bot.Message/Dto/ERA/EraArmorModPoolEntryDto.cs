using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraArmorModPoolEntryDto
{
    public Guid ModDefinitionId { get; set; }
    public int Weight { get; set; }
}
