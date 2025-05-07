using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;
/// <summary>
/// 展示节点定义，描述游戏界面中的信息展示层级结构
/// </summary>
public class Destiny2ManifestPresentationNodeDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含名称、描述和图标等信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 原始图标，节点的原始图标资源路径
    /// </summary>
    public string OriginalIcon { get; set; } = string.Empty;

    /// <summary>
    /// 根视图图标，在根视图中显示的图标资源路径
    /// </summary>
    public string RootViewIcon { get; set; } = string.Empty;

    /// <summary>
    /// 节点类型，定义展示节点的功能类型
    /// </summary>
    public Destiny2PresentationNodeTypeEnum NodeType { get; set; }

    /// <summary>
    /// 是否为赛季性内容，标识该节点是否与特定赛季相关
    /// </summary>
    public bool IsSeasonal { get; set; }

    /// <summary>
    /// 作用域，定义节点的可见范围（账号级或角色级）
    /// </summary>
    public Destiny2ScopeEnum Scope { get; set; }

    /// <summary>
    /// 目标哈希，关联到该节点相关的目标定义
    /// </summary>
    public uint ObjectiveHash { get; set; }

    /// <summary>
    /// 完成记录哈希，关联到标记节点完成状态的记录定义
    /// </summary>
    public uint CompletionRecordHash { get; set; }

    /// <summary>
    /// 子节点，包含该节点下的所有子级展示项
    /// </summary>
    public Destiny2ManifestPresentationNodeChildrenBlockDto Children { get; set; } = new();

    /// <summary>
    /// 显示样式，决定节点在界面中的展示方式
    /// </summary>
    public int DisplayStyle { get; set; }

    /// <summary>
    /// 屏幕样式，决定节点打开后的屏幕布局
    /// </summary>
    public int ScreenStyle { get; set; }

    /// <summary>
    /// 需求，访问节点所需的条件和权限
    /// </summary>
    public Destiny2ManifestPresentationNodeRequirementsBlockDto Requirements { get; set; } = new();

    /// <summary>
    /// 禁用子屏幕导航，设置是否阻止导航到子节点屏幕
    /// </summary>
    public bool DisableChildSubscreenNavigation { get; set; }

    /// <summary>
    /// 最大分类记录分数，该分类下可获得的最高记录积分总和
    /// </summary>
    public int MaxCategoryRecordScore { get; set; }

    /// <summary>
    /// 展示节点类型，节点的展示类型分类
    /// </summary>
    public Destiny2PresentationNodeTypeEnum PresentationNodeType { get; set; }

    /// <summary>
    /// 特性标识符列表，节点关联的特性字符串标识符集合
    /// </summary>
    public List<string> TraitIds { get; set; } = [];

    /// <summary>
    /// 特性哈希列表，节点关联的特性定义哈希集合
    /// </summary>
    public List<uint> TraitHashes { get; set; } = [];

    /// <summary>
    /// 父节点哈希列表，指向当前节点所有父级节点的哈希集合
    /// </summary>
    public List<uint> ParentNodeHashes { get; set; } = [];
}

/// <summary>
/// 展示节点子项块，包含不同类型的子级展示项，如子节点、收藏品、记录、指标和可制作物品
/// </summary>
public class Destiny2ManifestPresentationNodeChildrenBlockDto
{
    /// <summary>
    /// 子展示节点列表，包含该节点下的所有子级展示节点
    /// </summary>
    public List<Destiny2ManifestPresentationNodeChildEntryDto> PresentationNodes { get; set; } = [];

    /// <summary>
    /// 收藏品列表，包含该节点下的所有可收集物品
    /// </summary>
    public List<Destiny2ManifestPresentationNodeCollectibleChildEntryDto> Collectibles { get; set; } = [];

    /// <summary>
    /// 记录列表，包含该节点下的所有成就和进度记录
    /// </summary>
    public List<Destiny2ManifestPresentationNodeRecordChildEntryDto> Records { get; set; } = [];

    /// <summary>
    /// 指标列表，包含该节点下的所有统计数据和指标
    /// </summary>
    public List<Destiny2ManifestPresentationNodeMetricChildEntryDto> Metrics { get; set; } = [];

    /// <summary>
    /// 可制作物品列表，包含该节点下的所有可制作物品
    /// </summary>
    public List<Destiny2ManifestPresentationNodeCraftableChildEntryDto> Craftables { get; set; } = [];
}

/// <summary>
/// 展示节点子项基类，定义子项的基本属性
/// </summary>
public class Destiny2ManifestPresentationNodeChildEntryBaseDto
{
    /// <summary>
    /// 节点显示优先级，决定子项在界面中的排序顺序
    /// </summary>
    public uint NodeDisplayPriority { get; set; }
}

/// <summary>
/// 展示节点子项，指向另一个展示节点的引用
/// </summary>
public class Destiny2ManifestPresentationNodeChildEntryDto : Destiny2ManifestPresentationNodeChildEntryBaseDto
{
    /// <summary>
    /// 展示节点哈希，关联到子级展示节点定义
    /// </summary>
    public uint PresentationNodeHash { get; set; }
}

/// <summary>
/// 展示节点收藏品子项，指向一个收藏品的引用
/// </summary>
public class Destiny2ManifestPresentationNodeCollectibleChildEntryDto : Destiny2ManifestPresentationNodeChildEntryBaseDto
{
    /// <summary>
    /// 收藏品哈希，关联到特定收藏品定义
    /// </summary>
    public uint CollectibleHash { get; set; }
}

/// <summary>
/// 展示节点记录子项，指向一个成就或进度记录的引用
/// </summary>
public class Destiny2ManifestPresentationNodeRecordChildEntryDto : Destiny2ManifestPresentationNodeChildEntryBaseDto
{
    /// <summary>
    /// 记录哈希，关联到特定成就或进度记录定义
    /// </summary>
    public uint RecordHash { get; set; }
}

/// <summary>
/// 展示节点指标子项，指向一个统计数据或指标的引用
/// </summary>
public class Destiny2ManifestPresentationNodeMetricChildEntryDto : Destiny2ManifestPresentationNodeChildEntryBaseDto
{
    /// <summary>
    /// 指标哈希，关联到特定统计数据或指标定义
    /// </summary>
    public uint MetricHash { get; set; }
}

/// <summary>
/// 展示节点可制作物品子项，指向一个可制作物品的引用
/// </summary>
public class Destiny2ManifestPresentationNodeCraftableChildEntryDto : Destiny2ManifestPresentationNodeChildEntryBaseDto
{
    /// <summary>
    /// 可制作物品哈希，关联到特定可制作物品定义
    /// </summary>
    public uint CraftableItemHash { get; set; }
}

/// <summary>
/// 展示节点需求块，定义访问节点所需的条件和权限
/// </summary>
public class Destiny2ManifestPresentationNodeRequirementsBlockDto
{
    /// <summary>
    /// 权限不可用消息，当玩家没有访问权限时显示的提示文本
    /// </summary>
    public string EntitlementUnavailableMessage { get; set; } = string.Empty;
}

/// <summary>
/// 展示节点清单，存储所有展示节点定义的字典集合
/// </summary>
public class Destiny2ManifestPresentationNodeDto : Dictionary<string, Destiny2ManifestPresentationNodeDefinitionDto>
{
}