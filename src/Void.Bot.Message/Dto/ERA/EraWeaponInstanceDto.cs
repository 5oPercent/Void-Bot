namespace Void.Bot.Message.Dto.ERA;

/// <summary>生成后固定的候选集合及当前选择；不保存基础值或最终属性。</summary>
public sealed class EraWeaponInstanceDto
{
    public List<EraWeaponInstanceSlotDto> Slots { get; set; } = new List<EraWeaponInstanceSlotDto>();
}
