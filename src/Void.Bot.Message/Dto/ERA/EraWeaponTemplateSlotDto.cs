using System.Text.Json.Serialization;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponTemplateSlotDto
{
    public EraWeaponModSlot Slot { get; set; }
    /// <summary>新实例此槽必须生成的不同候选数，1～16；不影响已有实例。</summary>
    public int CandidateCount { get; set; }
    public int DisplayOrder { get; set; }
    /// <summary>只用于新生成的加权候选池，不能用它重建实例候选。</summary>
    public List<EraWeaponModPoolEntryDto> Pool { get; set; } = new List<EraWeaponModPoolEntryDto>();
}
