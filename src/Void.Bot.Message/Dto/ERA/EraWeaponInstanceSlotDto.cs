namespace Void.Bot.Message.Dto.ERA;

public sealed class EraWeaponInstanceSlotDto
{
    public Guid SlotId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid SlotTypeId { get; set; }
    public int DisplayOrder { get; set; }
    public Guid SelectedCandidateId { get; set; }
    public List<EraWeaponCandidateDto> Candidates { get; set; } = new List<EraWeaponCandidateDto>();
}
