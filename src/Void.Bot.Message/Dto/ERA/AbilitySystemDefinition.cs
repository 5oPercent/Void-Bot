namespace Void.Bot.Message.Dto.ERA;

/// <summary>
/// 能力体系定义，提供特殊、主要、次要和体系专属战术技能池。
/// </summary>
public sealed class AbilitySystemDefinition
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
    /// 当前体系允许装入特殊槽的技能 ID。
    /// </summary>
    public List<string> SpecialSkills { get; set; } = new List<string>();

    /// <summary>
    /// 当前体系允许装入主要槽的技能 ID。
    /// </summary>
    public List<string> PrimarySkills { get; set; } = new List<string>();

    /// <summary>
    /// 当前体系允许装入次要槽的技能 ID。
    /// </summary>
    public List<string> SecondarySkills { get; set; } = new List<string>();

    /// <summary>
    /// 当前体系专属战术技能 ID，实际可选范围还包含通用战术技能池。
    /// </summary>
    public List<string> TacticalSkills { get; set; } = new List<string>();
}
