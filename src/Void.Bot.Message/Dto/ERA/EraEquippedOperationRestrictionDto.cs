namespace Void.Bot.Message.Dto.ERA;

/// <summary>当前角色所在世界的已穿装备限制；版本独立于角色装备版本。</summary>
public sealed class EraEquippedOperationRestrictionDto
{
    /// <summary>限制所属世界，跨世界不能只比较数值版本。</summary>
    public Guid WorldInstanceId { get; set; }
    /// <summary>读取限制时的世界状态版本。</summary>
    public long WorldVersion { get; set; }
    /// <summary>本世界限制值变化版本，迁移不清零。</summary>
    public long RestrictionVersion { get; set; }
    /// <summary>为真时禁止替换和卸下，仍允许空槽装配。</summary>
    public bool IsRestricted { get; set; }
}
