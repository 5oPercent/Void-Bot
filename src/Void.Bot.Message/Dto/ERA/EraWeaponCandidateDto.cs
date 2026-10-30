namespace Void.Bot.Message.Dto.ERA;

/// <summary>武器实例生成后固定的一个改装候选；候选身份与公共改装定义身份分别保存。</summary>
public sealed class EraWeaponCandidateDto
{
    /// <summary>此候选的唯一标识，在生成时分配；供同一实例槽位的选择记录引用。</summary>
    public Guid CandidateId { get; set; }
    /// <summary>公共改装定义标识；候选不复制改装属性规则。</summary>
    public Guid ModDefinitionId { get; set; }
    /// <summary>此候选在所属实例槽位内的显示顺序。</summary>
    public int DisplayOrder { get; set; }
}
