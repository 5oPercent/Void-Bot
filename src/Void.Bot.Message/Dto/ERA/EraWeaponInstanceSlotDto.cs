using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

public sealed class EraWeaponInstanceSlotDto
{
    public EraWeaponModSlot Slot { get; set; }
    public Guid SelectedCandidateId { get; set; }
    public List<EraWeaponCandidateDto> Candidates { get; set; } = new List<EraWeaponCandidateDto>();
}
