using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 沙盒特性定义，描述游戏中武器、装备和能力的特殊效果或属性
/// </summary>
public class Destiny2ManifestSandboxPerkDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 特性标识符，特性的唯一识别字符串
    /// </summary>
    public string PerkIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// 是否可显示，决定特性是否在界面中对玩家可见
    /// </summary>
    public bool IsDiaplayable { get; set; }

    /// <summary>
    /// 伤害类型，特性关联的伤害元素类型
    /// </summary>
    public Destiny2DamageTypeEnum DamageType { get; set; }

    /// <summary>
    /// 伤害类型哈希，关联到伤害类型定义
    /// </summary>
    public uint DamageTypeHash { get; set; }
}

/// <summary>
/// 沙盒特性清单，存储所有沙盒特性定义的字典集合
/// </summary>
public class Destiny2ManifestSandboxPerkDto : Dictionary<string, Destiny2ManifestSandboxPerkDefinitionDto>
{
}