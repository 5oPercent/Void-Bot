using System.Text.Json.Serialization;
using Void.Bot.Message.Converters;
using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 命运2清单基础类
/// </summary>
public class Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 哈希
    /// </summary>
    public uint Hash { get; set; }

    /// <summary>
    /// 索引
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// 是否已编辑
    /// </summary>
    public bool Redacted { get; set; }

    /// <summary>
    /// 是否处于黑名单列表
    /// </summary>
    public bool Blacklisted { get; set; }
}

/// <summary>
/// 命运2清单组件显示属性
/// </summary>
public class Destiny2ManifestDisplayPropertiesDefinitionDto
{
    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 图标路径
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 图标序列
    /// </summary>
    public Destiny2ManifestSequenceDefinitionDto IconSequences { get; set; } = new();
    
    /// <summary>
    /// 高清图标
    /// </summary>
    public string HighResIcon { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否有图标
    /// </summary>
    public bool HasIcon { get; set; }
}

/// <summary>
/// 命运2清单组件队列
/// </summary>
public class Destiny2ManifestSequenceDefinitionDto
{
    public List<string> Frames { get; set; } = [];
}

/// <summary>
/// 命运2清单组件奖励物品
/// </summary>
public class Destiny2ManifestRewardDefinitionDto
{
    /// <summary>
    /// 物品哈希
    /// </summary>
    public uint ItemHash { get; set; }
    
    /// <summary>
    /// 数量
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// 有条件可见性
    /// </summary>
    public bool HasConditionalVisibility { get; set; }
}

/// <summary>
/// 命运2清单组件修改器
/// </summary>
public class Destiny2ManifestModifyDefinitionDto
{
    /// <summary>
    /// 修改器哈希
    /// </summary>
    public uint ActivityModifierHash { get; set; }
}

/// <summary>
/// 命运2清单组件显示内容
/// </summary>
public class Destiny2ManifestDisplayStringDefinitionDto
{
    /// <summary>
    /// 展示内容
    /// </summary>
    public string DisplayString { get; set; } = string.Empty;
}

/// <summary>
/// 命运2清单统计数据显示定义
/// </summary>
public class Destiny2ManifestStatDisplayDefinitionDto
{
    /// <summary>
    /// 统计数据哈希
    /// </summary>
    public uint StatHash { get; set; }

    /// <summary>
    /// 最大值
    /// </summary>
    public int MaximumValue { get; set; }

    /// <summary>
    /// 是否显示为数字
    /// </summary>
    public bool DisplayAsNumeric { get; set; }

    /// <summary>
    /// 显示插值点
    /// </summary>
    public Destiny2ManifestInterpolationPointDefinitionDto DisplayInterpolation { get; set; } = new();
}

/// <summary>
/// 命运2清单插值点定义
/// </summary>
public class Destiny2ManifestInterpolationPointDefinitionDto
{
    /// <summary>
    /// 值
    /// </summary>
    public int Value { get; set; }
    
    /// <summary>
    /// 权重
    /// </summary>
    public int Weight { get; set; }
}

/// <summary>
/// 命运2清单统计数据覆盖定义
/// </summary>
public class Destiny2ManifestStatOverrideDefinitionDto
{
    /// <summary>
    /// 统计数据哈希
    /// </summary>
    public uint StatHash { get; set; }

    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
}

/// <summary>
/// 命运2清单颜色定义
/// </summary>
public class Destiny2ManifestColorDefinitionDto
{
    public byte Red { get; set; }
    
    public byte Green { get; set; }
    
    public byte Blue { get; set; }
    
    public byte Alpha { get; set; }
}

/// <summary>
/// 命运2清单位置定义
/// </summary>
public class Destiny2ManifestPositionDefinitionDto
{
    public int X { get; set; }

    public int Y { get; set; }

    public int Z { get; set; }
}

/// <summary>
/// 命运2关联图定义，用于描述活动图之间的关联关系
/// </summary>
public class Destiny2ManifestLinkedGraphDefinitionDto
{
    /// <summary>
    /// 图表描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// 图表名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 定义访问此图表的条件
    /// </summary>
    public Destiny2ManifestUnlockExpressionDefinitionDto UnlockExpression { get; set; } = new();

    /// <summary>
    /// 关联图表ID
    /// </summary>
    public uint LinkedGraphId { get; set; }

    /// <summary>
    /// 关联的图表列表
    /// </summary>
    public List<Destiny2ManifestLinkedGraphEntryDefinitionDto> LinkedGraphs { get; set; } = [];
    
    /// <summary>
    /// 图表概述
    /// </summary>
    public string Overview { get; set; } = string.Empty;
}

/// <summary>
/// 命运2关联图表条目定义
/// </summary>
public class Destiny2ManifestLinkedGraphEntryDefinitionDto
{
    /// <summary>
    /// 活动图哈希值，用于引用特定的活动图
    /// </summary>
    public uint ActivityGraphHash { get; set; }
}

/// <summary>
/// 命运2解锁表达式定义，描述内容解锁的条件和范围
/// </summary>
public class Destiny2ManifestUnlockExpressionDefinitionDto
{
    /// <summary>
    /// 作用域
    /// </summary>
    public Destiny2GatingScopeEnum Scope { get; set; }
}

/// <summary>
/// 命运2清单选项定义
/// </summary>
public class Destiny2ManifestOptionDefinitionDto
{
    /// <summary>
    /// 自定义类别哈希
    /// </summary>
    public uint CustomizationCategoryHash { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 选项列表
    /// </summary>
    public List<Destiny2ManifestOptionEntryDefinitionDto> Options { get; set; } = [];
}

/// <summary>
/// 命运2清单选项条目定义，描述单个选项的具体内容
/// </summary>
public class Destiny2ManifestOptionEntryDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 值列表，支持单个或数组格式
    /// </summary>
    [JsonConverter(typeof(SingleOrArrayConverter<uint>))]
    public List<uint> Value { get; set; } = [];
}

/// <summary>
/// 命运2清单展示节点需求定义，描述展示节点的访问条件
/// </summary>
public class Destiny2ManifestPresentationNodeRequirementsBlockDefinitionDto
{
    /// <summary>
    /// 权限不可用时的提示消息
    /// </summary>
    public string EntitlementUnavailableMessage { get; set; } = string.Empty;
}

/// <summary>
/// 命运2清单展示子节点定义，用于构建展示层次结构
/// </summary>
public class Destiny2ManifestPresentationChildBlockDefinitionDto
{
    /// <summary>
    /// 展示节点类型
    /// </summary>
    public Destiny2PresentationNodeTypeEnum PresentationNodeType { get; set; }

    /// <summary>
    /// 父展示节点哈希列表
    /// </summary>
    public List<uint> ParentPresentationNodeHashes { get; set; } = [];

    /// <summary>
    /// 显示样式
    /// </summary>
    public int DisplayStyle { get; set; }
}

/// <summary>
/// 命运2清单气泡区域定义，用于描述目的地中的特定可探索区域
/// </summary>
public class Destiny2ManifestBubbleDefinition
{
    /// <summary>
    /// 气泡区域的唯一哈希标识符
    /// </summary>
    public uint Hash { get; set; }
    
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
}