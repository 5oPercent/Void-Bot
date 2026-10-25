using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponConfigurationSlotDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid SlotTypeId { get; set; }
    public int CandidateCount { get; set; }
    public int DisplayOrder { get; set; }
    public List<EraWeaponModPoolEntryDto> Pool { get; set; } = new();
}
