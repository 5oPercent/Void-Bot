namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 沙盒模式定义，描述游戏武器系统中的模式模板和属性
/// </summary>
public class Destiny2ManifestSandboxPatternDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 模式哈希，唯一标识此沙盒模式
    /// </summary>
    public uint PatternHash { get; set; }

    /// <summary>
    /// 模式全局标签ID哈希，关联到模式的全局标识符
    /// </summary>
    public uint PatternGlobalTagIdHash { get; set; }

    /// <summary>
    /// 武器内容组哈希，关联到此模式适用的武器内容集合
    /// </summary>
    public uint WeaponContentGroupHash { get; set; }

    /// <summary>
    /// 武器翻译组哈希，关联到此模式的本地化信息
    /// </summary>
    public uint WeaponTranslationGroupHash { get; set; }

    /// <summary>
    /// 武器类型哈希，关联到此模式适用的武器类型定义
    /// </summary>
    public uint WeaponTypeHash { get; set; }

    /// <summary>
    /// 武器类型，表示模式适用的武器类型枚举值
    /// </summary>
    public int WeaponType { get; set; }

    /// <summary>
    /// 过滤器列表，定义模式应用的区域和条件过滤器
    /// </summary>
    public List<Destiny2ManifestArrangementRegionFilterDefinitionDto> Filters { get; set; } = [];
}

/// <summary>
/// 沙盒模式清单，存储所有沙盒模式定义的字典集合
/// </summary>
public class Destiny2ManifestSandboxPatternDto : Dictionary<string, Destiny2ManifestSandboxPatternDefinitionDto>
{
}