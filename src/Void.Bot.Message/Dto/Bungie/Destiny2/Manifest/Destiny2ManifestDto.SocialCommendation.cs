namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 社交表彰定义，描述游戏中玩家之间的表彰和赞赏系统
/// </summary>
public class Destiny2ManifestSocialCommendationDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含表彰的名称、描述和图标
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 卡片图片路径，表彰卡片显示的图像
    /// </summary>
    public string CardImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 颜色定义，表彰在界面上显示的主题色
    /// </summary>
    public Destiny2ManifestColorDefinitionDto Color { get; set; } = new();

    /// <summary>
    /// 显示优先级，控制表彰在列表中的排序位置
    /// </summary>
    public int DisplayPriority { get; set; }

    /// <summary>
    /// 活动赠予限制，单个活动中可授予此表彰的次数上限
    /// </summary>
    public int ActivityGivingLimit { get; set; }

    /// <summary>
    /// 父级表彰节点哈希，关联到上层表彰分类
    /// </summary>
    public uint ParentCommendationNodeHash { get; set; }

    /// <summary>
    /// 活动显示属性，描述可以获得此表彰的活动信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayActivities { get; set; } = new();
}

/// <summary>
/// 社交表彰清单，存储所有社交表彰定义的字典集合
/// </summary>
public class Destiny2ManifestSocialCommendationDto : Dictionary<string, Destiny2ManifestSocialCommendationDefinitionDto>
{
    // 使用字符串键（通常是哈希值）映射到对应的社交表彰定义
}