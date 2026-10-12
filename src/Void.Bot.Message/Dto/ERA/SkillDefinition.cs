namespace Void.Bot.Message.Dto.ERA;

/// <summary>
/// 通过稳定技能 ID 引用的技能定义；同一技能可提供多个槽位释放方案。
/// </summary>
public sealed class SkillDefinition
{
    /// <summary>
    /// 稳定且唯一的定义 ID，不随数值或客户端表现调整改变。
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 基本显示名称；图标、颜色和其他样式由客户端映射。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 基本文字说明，不携带客户端表现资源。
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 技能支持的槽位方案；每个槽位最多提供一个方案。
    /// </summary>
    public List<SkillVariant> Variants { get; set; } = new List<SkillVariant>();
}
