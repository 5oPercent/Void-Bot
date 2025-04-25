namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 项目类别定义
/// </summary>
public class Destiny2ManifestItemCategoryDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 是否可见
    /// </summary>
    public bool Visible { get; set; }

    /// <summary>
    /// 是否已弃用
    /// </summary>
    public bool Deprecated { get; set; }

    /// <summary>
    /// 简短标题
    /// </summary>
    public string ShortTitle { get; set; } = string.Empty;

    /// <summary>
    /// 项目类型匹配的正则表达式
    /// </summary>
    public string ItemTypeRegex { get; set; } = string.Empty;
    
    /// <summary>
    /// 项目类型不匹配的正则表达式
    /// </summary>
    public string ItemTypeRegexNot { get; set; } = string.Empty;

    /// <summary>
    /// 原始仓库标识符
    /// </summary>
    public string OriginBucketIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// 破坏者?
    /// </summary>
    public int GrantDestinyBreakerType { get; set; }

    /// <summary>
    /// 物品类别
    /// </summary>
    public int GrantDestinyItemType { get; set; }

    /// <summary>
    /// 子类型
    /// </summary>
    public int GrantDestinySubType { get; set; }

    /// <summary>
    /// 职业类型
    /// </summary>
    public int GrantDestinyClass { get; set; }

    /// <summary>
    /// 分组类别哈希列表，包含与此类别分在一组的其他类别
    /// </summary>
    public List<uint> GroupedCategoryHashes { get; set; } = [];

    /// <summary>
    /// 父类别哈希列表，指向此类别所属的父级类别
    /// </summary>
    public List<uint> ParentCategoryHashes { get; set; } = [];

    /// <summary>
    /// 指示该类别是否为插件
    /// </summary>
    public bool IsPlug { get; set; }
    
    /// <summary>
    /// 插件类别标识符
    /// </summary>
    public string PlugCategoryIdentifier { get; set; } = string.Empty;
    
    /// <summary>
    /// 指示该类别是否仅用于分组
    /// </summary>
    public bool GroupCategoryOnly { get; set; }
    
    /// <summary>
    /// 特性标识符
    /// </summary>
    public string TraitId { get; set; } = string.Empty;

}

/// <summary>
/// 项目类别清单
/// </summary>
public class Destiny2ManifestItemCategoryDto : Dictionary<string, Destiny2ManifestItemCategoryDefinitionDto>
{
}