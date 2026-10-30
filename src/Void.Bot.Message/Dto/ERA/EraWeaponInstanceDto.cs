namespace Void.Bot.Message.Dto.ERA;

/// <summary>生成后固定的候选集合及当前选择；不保存基础值或最终属性。</summary>
public sealed class EraWeaponInstanceDto
{
    /// <summary>此武器实例生成时固定的槽位、候选集合及当前选择；后续修改方案不重抽已有候选。</summary>
    public List<EraWeaponInstanceSlotDto> Slots { get; set; } = new List<EraWeaponInstanceSlotDto>();
}
