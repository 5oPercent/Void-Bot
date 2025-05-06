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
    /// 是否被隐藏
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
    public Destiny2ManifestInterpolationPointDefinitionDto<int> DisplayInterpolation { get; set; } = new();
}

/// <summary>
/// 命运2清单插值点定义
/// </summary>
public class Destiny2ManifestInterpolationPointDefinitionDto<T> where T : struct
{
    /// <summary>
    /// 值
    /// </summary>
    public T Value { get; set; }
    
    /// <summary>
    /// 权重
    /// </summary>
    public T Weight { get; set; }
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
    public uint ColorHash { get; set; }
    
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
public class Destiny2ManifestBubbleDefinitionDto
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

/// <summary>
/// 命运2进度奖励定义，描述完成进度时获得的奖励
/// </summary>
public class Destiny2ManifestProgressionRewardDefinitionDto
{
    /// <summary>
    /// 进度映射哈希，关联到特定的进度系统
    /// </summary>
    public uint ProgressionMappingHash { get; set; }

    /// <summary>
    /// 奖励数量
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// 是否应用节流限制
    /// </summary>
    public bool ApplyThrottles { get; set; }
}

/// <summary>
/// 命运2装备块定义，描述物品的装备属性和限制
/// </summary>
public class Destiny2ManifestEquippingBlockDefinitionDto
{
    /// <summary>
    /// 装备组物品哈希，关联到此装备所属的套装
    /// </summary>
    public uint GearsetItemHash { get; set; }

    /// <summary>
    /// 唯一标签，用于标识装备的唯一性
    /// </summary>
    public string UniqueLabel { get; set; } = string.Empty;

    /// <summary>
    /// 唯一标签哈希
    /// </summary>
    public uint UniqueLabelHash { get; set; }

    /// <summary>
    /// 装备槽类型哈希，定义物品可装备的位置
    /// </summary>
    public uint EquipmentSlotTypeHash { get; set; }

    /// <summary>
    /// 装备属性标识
    /// </summary>
    public int Attributes { get; set; }
    
    /// <summary>
    /// 弹药类型
    /// </summary>
    public int AmmoType { get; set; }

    /// <summary>
    /// 显示字符串列表
    /// </summary>
    public List<string> DisplayStrings { get; set; } = [];
}

/// <summary>
/// 命运2染色引用定义，描述装备的染色通道和染料
/// </summary>
public class Destiny2ManifestDyeReferenceDto
{
    /// <summary>
    /// 染色通道哈希
    /// </summary>
    public uint ChannelHash { get; set; }
    
    /// <summary>
    /// 染料哈希
    /// </summary>
    public uint DyeHash { get; set; }
}

/// <summary>
/// 命运2装备艺术排列引用，关联装备到特定职业的视觉外观
/// </summary>
public class Destiny2ManifestGearArtArrangementReferenceDto
{
    /// <summary>
    /// 职业哈希
    /// </summary>
    public uint ClassHash { get; set; }
    
    /// <summary>
    /// 艺术排列哈希
    /// </summary>
    public uint ArtArrangementHash { get; set; }
}

/// <summary>
/// 命运2衍生物品分类定义，描述从主物品衍生的物品类别
/// </summary>
public class Destiny2ManifestDerivedItemCategoryDefinitionDto
{
    /// <summary>
    /// 分类描述
    /// </summary>
    public string CategoryDescription { get; set; } = string.Empty;
    
    /// <summary>
    /// 分类中的物品列表
    /// </summary>
    public List<Destiny2ManifestDerivedItemDefinitionDto> Items { get; set; } = [];
}

/// <summary>
/// 命运2衍生物品定义，描述从主物品衍生的单个物品
/// </summary>
public class Destiny2ManifestDerivedItemDefinitionDto
{
    /// <summary>
    /// 物品哈希
    /// </summary>
    public uint ItemHash { get; set; }

    /// <summary>
    /// 物品名称
    /// </summary>
    public string ItemName { get; set; } = string.Empty;

    /// <summary>
    /// 物品详情
    /// </summary>
    public string ItemDetail { get; set; } = string.Empty;
    
    /// <summary>
    /// 物品描述
    /// </summary>
    public string ItemDescription { get; set; } = string.Empty;
    
    /// <summary>
    /// 图标路径
    /// </summary>
    public string IconPath { get; set; } = string.Empty;
    
    /// <summary>
    /// 商人物品索引
    /// </summary>
    public int VendorItemIndex { get; set; }
}

/// <summary>
/// 命运2目标显示属性，控制目标在界面中的显示方式
/// </summary>
public class Destiny2ManifestObjectiveDisplayPropertiesDto
{
    /// <summary>
    /// 关联的活动哈希
    /// </summary>
    public uint ActivityHash { get; set; }

    /// <summary>
    /// 是否在物品预览屏幕上显示
    /// </summary>
    public bool DisplayOnItemPreviewScreen { get; set; }
}

/// <summary>
/// 命运2动画引用，描述物品关联的动画资源
/// </summary>
public class Destiny2ManifestAnimationReferenceDto
{
    /// <summary>
    /// 动画名称
    /// </summary>
    public string AnimName { get; set; } = string.Empty;
    
    /// <summary>
    /// 动画标识符
    /// </summary>
    public string AnimIdentifier { get; set; } = string.Empty;
    
    /// <summary>
    /// 动画资源路径
    /// </summary>
    public string Path { get; set; } = string.Empty;
}

/// <summary>
/// 命运2超链接引用，描述游戏内的超链接内容
/// </summary>
public class Destiny2ManifestHyperlinkReferenceDto
{
    /// <summary>
    /// 超链接标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 超链接URL
    /// </summary>
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// 命运2显示字符串，描述界面上显示的文本及其样式
/// </summary>
public class Destiny2ManifestDisplayStringDto
{
    /// <summary>
    /// 显示的字符串内容
    /// </summary>
    public string DisplayString { get; set; } = string.Empty;
    
    /// <summary>
    /// 显示样式
    /// </summary>
    public string DisplayStyle { get; set; } = string.Empty;
}

/// <summary>
/// 命运2艺术排列区域过滤器定义，描述模型外观区域的过滤和变化规则
/// </summary>
public class Destiny2ManifestArrangementRegionFilterDefinitionDto
{
    /// <summary>
    /// 艺术排列区域哈希，唯一标识一个模型的特定区域
    /// </summary>
    public uint ArtArrangementRegionHash { get; set; }
    
    /// <summary>
    /// 艺术排列区域索引，指定区域在模型中的位置序号
    /// </summary>
    public int ArtArrangementRegionIndex { get; set; }
    
    /// <summary>
    /// 统计数据哈希，关联到控制此区域外观变化的统计属性
    /// </summary>
    public int StatHash { get; set; }

    /// <summary>
    /// 按统计值映射的排列索引，根据不同的统计值确定使用哪个外观变体
    /// </summary>
    public Dictionary<int, int> ArrangementIndexByStatValue { get; set; } = new();
}

/// <summary>
/// 命运2日期范围定义，描述游戏内容的有效时间范围
/// </summary>
public class Destiny2ManifestDateRangeDefinitionDto
{
    /// <summary>
    /// 开始时间，内容可用的起始日期和时间
    /// </summary>
    public DateTime Start { get; set; }
    
    /// <summary>
    /// 结束时间，内容可用的截止日期和时间
    /// </summary>
    public DateTime End { get; set; }
}