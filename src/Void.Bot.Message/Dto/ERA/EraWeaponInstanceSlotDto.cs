namespace Void.Bot.Message.Dto.ERA;

/// <summary>武器实例的一个改装槽位，保存生成时的显示快照、固定候选及当前选择。</summary>
public sealed class EraWeaponInstanceSlotDto
{
    /// <summary>生成时的槽位标识；用于关联同一实例的候选与选择，不要求最新方案仍包含此槽位。</summary>
    public Guid SlotId { get; set; }
    /// <summary>生成时保存的槽位显示名称快照。</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>生成时保存的槽位类型标识，约束此槽候选的改装类型。</summary>
    public Guid SlotTypeId { get; set; }
    /// <summary>生成时保存的槽位显示顺序。</summary>
    public int DisplayOrder { get; set; }
    /// <summary>当前选中的候选标识，必须属于本实例的此槽位。</summary>
    public Guid SelectedCandidateId { get; set; }
    /// <summary>生成后固定的候选集合，每项引用一个公共改装定义；选择其他候选不会重新抽取。</summary>
    public List<EraWeaponCandidateDto> Candidates { get; set; } = new List<EraWeaponCandidateDto>();
}
