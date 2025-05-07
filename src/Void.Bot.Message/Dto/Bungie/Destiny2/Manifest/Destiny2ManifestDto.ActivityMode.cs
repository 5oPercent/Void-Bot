using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 活动模式定义
/// </summary>
public class Destiny2ManifestActivityModeDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 背景图路径
    /// </summary>
    public string PgcrImage { get; set; } = string.Empty;

    /// <summary>
    /// 模式类型
    /// </summary>
    public Destiny2ActivityModeTypeEnum ModeType { get; set; }

    /// <summary>
    /// 模式分类
    /// </summary>
    public Destiny2ActivityModeCategoryEnum ActivityModeCategory { get; set; }

    /// <summary>
    /// 是否为团队模式
    /// </summary>
    public bool IsTeamBased { get; set; }

    /// <summary>
    /// 层级
    /// </summary>
    public Destiny2TierTypeEnum Tier { get; set; }
    
    /// <summary>
    /// 是否为聚合模式
    /// </summary>
    public bool IsAggregateMode { get; set; }

    /// <summary>
    /// 当前模式的父级模式哈希列表
    /// </summary>
    public List<uint> ParentHashes { get; set; } = [];

    /// <summary>
    /// 有好名称
    /// </summary>
    public string FriendlyName { get; set; } = string.Empty;

    /// <summary>
    /// 活动模式映射
    /// </summary>
    public Dictionary<uint, uint> ActivityModeMappings { get; set; } = [];

    /// <summary>
    /// 显示
    /// </summary>
    public bool Display { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Order { get; set; }
}

/// <summary>
/// 活动模式清单
/// </summary>
public class Destiny2ManifestActivityModeDto : Dictionary<string, Destiny2ManifestActivityModeDefinitionDto>
{
}