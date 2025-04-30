namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 命运2社交表彰节点定义，描述游戏中玩家表彰系统的分类节点
/// </summary>
public class Destiny2ManifestSocialCommendationNodeDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含表彰节点的名称、描述和图标等基本显示信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 颜色定义，用于表彰节点在界面上的颜色显示
    /// </summary>
    public Destiny2ManifestColorDefinitionDto Color { get; set; } = new();

    /// <summary>
    /// 着色图标，带有特定色调的表彰节点图标路径
    /// </summary>
    public string TintedIcon { get; set; } = string.Empty;

    /// <summary>
    /// 父表彰节点哈希，关联到此节点的上级分类节点
    /// </summary>
    public uint ParentCommendationNodeHash { get; set; }

    /// <summary>
    /// 子表彰节点哈希列表，包含此节点下所有子分类节点
    /// </summary>
    public List<uint> ChildCommendationNodeHashes { get; set; } = [];

    /// <summary>
    /// 子表彰哈希列表，包含此节点下所有具体表彰项目
    /// </summary>
    public List<uint> ChildCommendationHashes { get; set; } = [];
}

/// <summary>
/// 命运2社交表彰节点清单，包含所有表彰节点的定义集合
/// </summary>
public class Destiny2ManifestSocialCommendationNodeDto : Dictionary<string, Destiny2ManifestSocialCommendationNodeDefinitionDto>
{
}