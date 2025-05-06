using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 命运2里程碑定义，描述游戏中的里程碑事件和活动
/// </summary>
public class Destiny2ManifestMilestoneDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含里程碑的名称、描述和图标等基本显示信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 显示偏好，控制里程碑信息的展示方式（里程碑定义、当前任务步骤或当前活动挑战）
    /// </summary>
    public Destiny2MilestoneDisplayPreferenceEnum DisplayPreference { get; set; }
    
    /// <summary>
    /// 里程碑图像路径，用于自定义显示的背景或标志图像
    /// </summary>
    public string Image { get; set; } = string.Empty;
    
    /// <summary>
    /// 里程碑类型，定义里程碑的重置周期（一次性、每周、每日或特殊）
    /// </summary>
    public Destiny2MilestoneTypeEnum MilestoneType { get; set; }
    
    /// <summary>
    /// 是否可招募，控制玩家是否可以邀请其他玩家参与此里程碑活动
    /// </summary>
    public bool Recruitable { get; set; }
    
    /// <summary>
    /// 友好名称，用于内部识别的简短名称标识符
    /// </summary>
    public string FriendlyName { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否在探索器中显示，控制此里程碑是否出现在目标探索界面中
    /// </summary>
    public bool ShowInExplorer { get; set; }
    
    /// <summary>
    /// 是否在里程碑页面显示，控制此里程碑是否出现在主里程碑界面中
    /// </summary>
    public bool ShowInMilestones { get; set; }
    
    /// <summary>
    /// 探索界面优先显示活动图像，决定是否在探索界面优先使用活动图像而非里程碑图像
    /// </summary>
    public bool ExplorePrioritizesActivityImage { get; set; }
    
    /// <summary>
    /// 是否有可预测日期，标识里程碑是否按照固定日程出现
    /// </summary>
    public bool HasPredictableDates { get; set; }
    
    /// <summary>
    /// 任务列表，包含里程碑关联的所有任务及其定义
    /// </summary>
    public Dictionary<uint, Destiny2ManifestMilestoneQuestDefinitionDto> Quests { get; set; } = [];
    
    /// <summary>
    /// 奖励类别列表，包含里程碑完成后可获得的所有奖励分类
    /// </summary>
    public Dictionary<uint, Destiny2ManifestMilestoneRewardCategoryDefinitionDto> Rewards { get; set; } = [];
    
    /// <summary>
    /// 商人显示标题，用于标识与此里程碑关联的商人区域
    /// </summary>
    public string VendorsDisplayTitle { get; set; } = string.Empty;
    
    /// <summary>
    /// 商人列表，与此里程碑关联的所有商人定义
    /// </summary>
    public List<Destiny2ManifestMilestoneVendorDefinitionDto> Vendors { get; set; } = [];
    
    /// <summary>
    /// 值字典，里程碑的各种自定义值集合，用于特殊配置
    /// </summary>
    public Dictionary<string, Destiny2ManifestMilestoneValueDefinitionDto> Values { get; set; } = new();
    
    /// <summary>
    /// 是否为游戏内里程碑，区分真正在游戏内显示的里程碑与只用于API的里程碑
    /// </summary>
    public bool IsInGameMilestone { get; set; }
    
    /// <summary>
    /// 挑战活动列表，包含里程碑关联的所有活动及其挑战
    /// </summary>
    public List<Destiny2ManifestMilestoneChallengeActivityDefinitionDto> Activities { get; set; } = [];
    
    /// <summary>
    /// 默认排序顺序，控制里程碑在列表中的显示顺序
    /// </summary>
    public int DefaultOrder { get; set; }
}

/// <summary>
/// 命运2里程碑任务定义，定义里程碑关联的具体任务内容
/// </summary>
public class Destiny2ManifestMilestoneQuestDefinitionDto
{
    /// <summary>
    /// 任务物品哈希，关联到具体的任务物品定义
    /// </summary>
    public uint QuestItemHash { get; set; }
    
    /// <summary>
    /// 显示属性，包含任务的名称、描述和图标等基本显示信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 覆盖图像路径，用于替换默认任务图像
    /// </summary>
    public string OverrideImage { get; set; } = string.Empty;
    
    /// <summary>
    /// 任务奖励定义，包含完成任务后可获得的所有奖励
    /// </summary>
    public Destiny2ManifestMilestoneQuestRewardsDefinitionDto QuestRewards { get; set; } = new();

    /// <summary>
    /// 活动字典，包含此任务相关的所有活动及其定义
    /// </summary>
    public Dictionary<uint, Destiny2ManifestMilestoneActivityDefinitionDto> Activities { get; set; } = [];
    
    /// <summary>
    /// 目的地哈希，关联到任务所在的位置定义
    /// </summary>
    public uint DestinationHash { get; set; }
}

/// <summary>
/// 命运2里程碑任务奖励定义，包含任务完成后的奖励信息
/// </summary>
public class Destiny2ManifestMilestoneQuestRewardsDefinitionDto
{
    /// <summary>
    /// 奖励物品列表，包含所有可获得的奖励物品详情
    /// </summary>
    public List<Destiny2ManifestMilestoneQuestRewardItemDto> Items { get; set; } = [];
}

/// <summary>
/// 命运2里程碑任务奖励物品，定义单个奖励物品的详细信息
/// </summary>
public class Destiny2ManifestMilestoneQuestRewardItemDto
{
    /// <summary>
    /// 商人哈希，关联到提供该奖励的商人
    /// </summary>
    public uint VendorHash { get; set; }
    
    /// <summary>
    /// 商人物品索引，在商人库存中的位置索引
    /// </summary>
    public int VendorItemIndex { get; set; }
    
    /// <summary>
    /// 物品哈希，关联到具体奖励物品的定义
    /// </summary>
    public uint ItemHash { get; set; }
    
    /// <summary>
    /// 物品实例ID，用于标识特定实例的物品
    /// </summary>
    public long ItemInstanceId { get; set; }
    
    /// <summary>
    /// 数量，获得的奖励物品数量
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// 是否具有条件可见性，控制奖励是否在特定条件下可见
    /// </summary>
    public bool HasConditionalVisibility { get; set; }
}

/// <summary>
/// 命运2里程碑活动定义，定义里程碑关联的活动信息
/// </summary>
public class Destiny2ManifestMilestoneActivityDefinitionDto
{
    /// <summary>
    /// 概念性活动哈希，关联到活动的基本概念定义
    /// </summary>
    public uint ConceptualActivityHash { get; set; }

    /// <summary>
    /// 活动变体字典，包含此活动的所有变体版本
    /// </summary>
    public Dictionary<uint, Destiny2ManifestMilestoneActivityVariantDefinitionDto> Variants { get; set; } = new();
}

/// <summary>
/// 命运2里程碑活动变体定义，定义活动的不同难度或变种版本
/// </summary>
public class Destiny2ManifestMilestoneActivityVariantDefinitionDto
{
    /// <summary>
    /// 活动哈希，关联到具体的活动变体定义
    /// </summary>
    public uint ActivityHash { get; set; }
    
    /// <summary>
    /// 排序顺序，控制变体在列表中的显示顺序
    /// </summary>
    public int Order { get; set; }
}

/// <summary>
/// 命运2里程碑奖励类别定义，定义奖励的分类信息
/// </summary>
public class Destiny2ManifestMilestoneRewardCategoryDefinitionDto
{
    /// <summary>
    /// 类别哈希，用于唯一标识此奖励类别
    /// </summary>
    public uint CategoryHash { get; set; }
    
    /// <summary>
    /// 类别标识符，用于内部识别的字符串标识
    /// </summary>
    public string CategoryIdentifier { get; set; } = string.Empty;
    
    /// <summary>
    /// 显示属性，包含此奖励类别的名称、描述和图标
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 奖励条目字典，包含此类别下所有可能的奖励
    /// </summary>
    public Dictionary<uint, Destiny2ManifestMilestoneRewardEntryDefinitionDto> RewardEntries { get; set; } = [];
    
    /// <summary>
    /// 排序顺序，控制类别在列表中的显示顺序
    /// </summary>
    public int Order { get; set; }
}

/// <summary>
/// 命运2里程碑奖励条目定义，定义具体的奖励项目
/// </summary>
public class Destiny2ManifestMilestoneRewardEntryDefinitionDto
{
    /// <summary>
    /// 奖励条目哈希，用于唯一标识此奖励条目
    /// </summary>
    public uint RewardEntryHash { get; set; }
    
    /// <summary>
    /// 奖励条目标识符，用于内部识别的字符串标识
    /// </summary>
    public string RewardEntryIdentifier { get; set; } = string.Empty;
    
    /// <summary>
    /// 物品列表，包含此奖励条目提供的所有物品及其数量
    /// </summary>
    public List<Destiny2ManifestItemQuantityDto> Items { get; set; } = [];
    
    /// <summary>
    /// 商人哈希，关联到提供此奖励的商人
    /// </summary>
    public uint VendorHash { get; set; }
    
    /// <summary>
    /// 显示属性，包含此奖励条目的名称、描述和图标
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 排序顺序，控制条目在列表中的显示顺序
    /// </summary>
    public int Order { get; set; }
}

/// <summary>
/// 命运2里程碑商人定义，定义与里程碑关联的商人
/// </summary>
public class Destiny2ManifestMilestoneVendorDefinitionDto
{
    /// <summary>
    /// 商人哈希，关联到具体的商人定义
    /// </summary>
    public uint VendorHash { get; set; }
}

/// <summary>
/// 命运2里程碑值定义，定义里程碑的自定义数值或配置
/// </summary>
public class Destiny2ManifestMilestoneValueDefinitionDto
{
    /// <summary>
    /// 键名，用于识别此值的唯一标识符
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// 显示属性，包含此值的名称、描述和图标
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
}

/// <summary>
/// 命运2里程碑挑战活动定义，定义里程碑关联的挑战活动
/// </summary>
public class Destiny2ManifestMilestoneChallengeActivityDefinitionDto
{
    /// <summary>
    /// 活动哈希，关联到具体的活动定义
    /// </summary>
    public uint ActivityHash { get; set; }
    
    /// <summary>
    /// 挑战列表，包含此活动中所有的挑战定义
    /// </summary>
    public List<Destiny2ManifestMilestoneChallengeDefinitionDto> Challenges { get; set; } = [];
    
    /// <summary>
    /// 活动图节点列表，包含此活动在活动图中的所有节点
    /// </summary>
    public List<Destiny2ManifestMilestoneChallengeActivityGraphNodeEntryDto> ActivityGraphNodes { get; set; } = [];
    
    /// <summary>
    /// 阶段列表，包含此活动的所有阶段定义
    /// </summary>
    public List<Destiny2ManifestMilestoneChallengeActivityPhaseDto> Phases { get; set; } = [];
}

/// <summary>
/// 命运2里程碑挑战定义，定义单个挑战的目标要求
/// </summary>
public class Destiny2ManifestMilestoneChallengeDefinitionDto
{
    /// <summary>
    /// 挑战目标哈希，关联到此挑战的具体目标定义
    /// </summary>
    public uint ChallengeObjectiveHash { get; set; }
}

/// <summary>
/// 命运2里程碑挑战活动图节点条目，定义活动图中的节点关联
/// </summary>
public class Destiny2ManifestMilestoneChallengeActivityGraphNodeEntryDto
{
    /// <summary>
    /// 活动图哈希，关联到活动图的定义
    /// </summary>
    public uint ActivityGraphHash { get; set; }
    
    /// <summary>
    /// 活动图节点哈希，关联到活动图中的特定节点
    /// </summary>
    public uint ActivityGraphNodeHash { get; set; }
}

/// <summary>
/// 命运2里程碑挑战活动阶段，定义活动中的特定阶段
/// </summary>
public class Destiny2ManifestMilestoneChallengeActivityPhaseDto
{
    /// <summary>
    /// 阶段哈希，关联到具体阶段的定义
    /// </summary>
    public uint PhaseHash { get; set; }
}

/// <summary>
/// 里程碑清单
/// </summary>
public class Destiny2ManifestMilestoneDto : Dictionary<string, Destiny2ManifestMilestoneDefinitionDto>
{
}