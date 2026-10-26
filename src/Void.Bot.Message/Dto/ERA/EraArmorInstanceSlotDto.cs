namespace Void.Bot.Message.Dto.ERA;

public sealed class EraArmorInstanceSlotDto
{
    public Guid SlotId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid SlotTypeId { get; set; }
    public int DisplayOrder { get; set; }
    public Guid SelectedCandidateId { get; set; }
    public List<EraArmorCandidateDto> Candidates { get; set; } = new List<EraArmorCandidateDto>();
}
