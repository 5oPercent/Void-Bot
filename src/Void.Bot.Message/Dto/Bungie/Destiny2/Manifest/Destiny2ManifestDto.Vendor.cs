using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 命运2商人定义，包含游戏中商人的详细信息
/// </summary>
public class Destiny2ManifestVendorDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含商人的名称、描述和图标等基本显示信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 商人进度类型，定义商人的声望进度系统类型
    /// </summary>
    public Destiny2VendorProgressionTypeEnum VendorProgressionType { get; set; }
    
    /// <summary>
    /// 购买按钮文本
    /// </summary>
    public string BuyString { get; set; } = string.Empty;
    
    /// <summary>
    /// 出售按钮文本
    /// </summary>
    public string SellString { get; set; } = string.Empty;
    
    /// <summary>
    /// 商人展示物品的哈希值，用于在UI中表示该商人
    /// </summary>
    public uint DisplayItemHash { get; set; }
    
    /// <summary>
    /// 是否禁止购买，为true时玩家无法从该商人购买物品
    /// </summary>
    public bool InhibitBuying { get; set; }
    
    /// <summary>
    /// 是否禁止出售，为true时玩家无法向该商人出售物品
    /// </summary>
    public bool InhibitSelling { get; set; }
    
    /// <summary>
    /// 阵营哈希值，该商人所属阵营的引用
    /// </summary>
    public uint FactionHash { get; set; }
    
    /// <summary>
    /// 重置间隔（分钟），商人库存刷新的时间间隔
    /// </summary>
    public int ResetIntervalMinutes { get; set; }
    
    /// <summary>
    /// 重置偏移（分钟），商人库存刷新的时间偏移
    /// </summary>
    public int ResetOffsetMinutes { get; set; }
    
    /// <summary>
    /// 失败提示文本列表，玩家无法与商人交互时显示的消息
    /// </summary>
    public List<string> FailureStrings { get; set; } = [];

    /// <summary>
    /// 解锁时间范围列表，定义商人可用的时间段
    /// </summary>
    public List<Destiny2ManifestDateRangeDefinitionDto> UnlockRanges { get; set; } = [];
    
    /// <summary>
    /// 商人标识符，游戏内部使用的唯一标识
    /// </summary>
    public string VendorIdentifier { get; set; } = string.Empty;
    
    /// <summary>
    /// 商人肖像图片路径
    /// </summary>
    public string VendorPortrait { get; set; } = string.Empty;
    
    /// <summary>
    /// 商人横幅图片路径
    /// </summary>
    public string VendorBanner { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否启用，控制商人是否可用
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// 是否可见，控制商人是否在游戏UI中显示
    /// </summary>
    public bool Visible { get; set; }
    
    /// <summary>
    /// 商人子类别标识符
    /// </summary>
    public string VendorSubcategoryIdentifier { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否合并分类，控制商人界面中的分类显示方式
    /// </summary>
    public bool ConsolidateCategories { get; set; }
    
    /// <summary>
    /// 商人可执行操作列表
    /// </summary>
    public List<Destiny2ManifestVendorActionDefinitionDto> Actions { get; set; } = [];
    
    /// <summary>
    /// 商品分类列表，当前适用的分类
    /// </summary>
    public List<Destiny2ManifestVendorCategoryEntryDefinitionDto> Categories { get; set; } = [];
    
    /// <summary>
    /// 原始商品分类列表，商人的默认分类设置
    /// </summary>
    public List<Destiny2ManifestVendorCategoryEntryDefinitionDto> OriginalCategories { get; set; } = [];
    
    /// <summary>
    /// 显示分类列表，控制UI中如何展示商人的商品分类
    /// </summary>
    public List<Destiny2ManifestDisplayCategoryDefinitionDto> DisplayCategories { get; set; } = [];
    
    /// <summary>
    /// 交互列表，定义与商人的对话和交互选项
    /// </summary>
    public List<Destiny2ManifestVendorInteractionDefinitionDto> Interactions { get; set; } = [];
    
    /// <summary>
    /// 库存弹出窗口列表，定义商人特殊库存界面
    /// </summary>
    public List<Destiny2ManifestVendorInventoryFlyoutDefinitionDto> InventoryFlyouts { get; set; } = [];
    
    /// <summary>
    /// 物品列表，商人出售的所有物品
    /// </summary>
    public List<Destiny2ManifestVendorItemDefinitionDto> ItemList { get; set; } = [];
    
    /// <summary>
    /// 服务列表，商人提供的特殊服务
    /// </summary>
    public List<Destiny2ManifestVendorServiceDefinitionDto> Services { get; set; } = [];
    
    /// <summary>
    /// 接受物品列表，商人可接受交换的物品类型
    /// </summary>
    public List<Destiny2ManifestVendorAcceptedItemDefinitionDto> AcceptedItems { get; set; } = [];
    
    /// <summary>
    /// 是否随商人请求返回，控制API请求行为
    /// </summary>
    public bool ReturnWithVendorRequest { get; set; }
    
    /// <summary>
    /// 位置列表，商人可能出现的游戏位置
    /// </summary>
    public List<Destiny2ManifestVendorLocationDefinitionDto> Locations { get; set; } = [];
    
    /// <summary>
    /// 商人组引用列表，商人所属的分组
    /// </summary>
    public List<Destiny2ManifestVendorGroupReferenceDto> Groups { get; set; } = [];
    
    /// <summary>
    /// 忽略销售物品哈希列表，不在界面上显示的物品
    /// </summary>
    public List<uint> IgnoreSaleItemHashes { get; set; } = [];
}

/// <summary>
/// 命运2商人动作定义，描述商人可执行的操作
/// </summary>
public class Destiny2ManifestVendorActionDefinitionDto
{
    /// <summary>
    /// 动作描述文本
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// 执行时间（秒），完成动作所需的时间
    /// </summary>
    public int ExecuteSeconds { get; set; }
    
    /// <summary>
    /// 动作图标路径
    /// </summary>
    public string Icon { get; set; } = string.Empty;
    
    /// <summary>
    /// 动作名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 动作动词，显示在按钮上的文本
    /// </summary>
    public string Verb { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否为积极动作，影响UI显示样式
    /// </summary>
    public bool IsPositive { get; set; }
    
    /// <summary>
    /// 动作唯一标识符
    /// </summary>
    public string ActionId { get; set; } = string.Empty;
    
    /// <summary>
    /// 动作哈希值，用于在API中引用此动作
    /// </summary>
    public uint ActionHash { get; set; }
    
    /// <summary>
    /// 是否自动执行动作，无需玩家确认
    /// </summary>
    public bool AutoPerformAction { get; set; }
}

/// <summary>
/// 命运2商人分类条目定义，描述商人界面中的分类项
/// </summary>
public class Destiny2ManifestVendorCategoryEntryDefinitionDto
{
    /// <summary>
    /// 分类索引，在商人所有分类中的位置
    /// </summary>
    public int CategoryIndex { get; set; }
    
    /// <summary>
    /// 排序值，决定分类在界面中的显示顺序
    /// </summary>
    public int SortValue { get; set; }
    
    /// <summary>
    /// 分类哈希值，用于在API中引用此分类
    /// </summary>
    public uint CategoryHash { get; set; }
    
    /// <summary>
    /// 可用数量，此分类下可购买物品的数量
    /// </summary>
    public int QuantityAvailable { get; set; }
    
    /// <summary>
    /// 是否显示不可用物品
    /// </summary>
    public bool ShowUnavailableItems { get; set; }
    
    /// <summary>
    /// 如果没有相关货币是否隐藏此分类
    /// </summary>
    public bool HideIfNoCurrency { get; set; }
    
    /// <summary>
    /// 是否在常规购买界面隐藏此分类
    /// </summary>
    public bool HideFromRegularPurchase { get; set; }
    
    /// <summary>
    /// 购买按钮文本覆盖，替换默认的购买文本
    /// </summary>
    public string BuyStringOverride { get; set; } = string.Empty;
    
    /// <summary>
    /// 禁用状态下的描述文本
    /// </summary>
    public string DisabledDescription { get; set; } = string.Empty;
    
    /// <summary>
    /// 显示标题
    /// </summary>
    public string DisplayTitle { get; set; } = string.Empty;
    
    /// <summary>
    /// 分类覆盖信息，提供额外的UI元素
    /// </summary>
    public Destiny2ManifestVendorCategoryOverlayDefinitionDto Overlay { get; set; } = new();
    
    /// <summary>
    /// 商人物品索引，关联到商人物品列表的索引
    /// </summary>
    public int VendorItemIndexes { get; set; }
    
    /// <summary>
    /// 是否为预览分类
    /// </summary>
    public bool IsPreview { get; set; }
    
    /// <summary>
    /// 是否仅用于显示，不可交互
    /// </summary>
    public bool IsDisplayOnly { get; set; }
    
    /// <summary>
    /// 重置间隔（分钟）覆盖，覆盖商人默认的重置间隔
    /// </summary>
    public int ResetIntervalMinutesOverride { get; set; }
    
    /// <summary>
    /// 重置偏移（分钟）覆盖，覆盖商人默认的重置偏移
    /// </summary>
    public int ResetOffsetMinutesOverride { get; set; }
}

/// <summary>
/// 命运2商人分类覆盖定义，提供分类的额外UI元素和信息
/// </summary>
public class Destiny2ManifestVendorCategoryOverlayDefinitionDto
{
    /// <summary>
    /// 选择描述文本
    /// </summary>
    public string ChoiceDescription { get; set; } = string.Empty;
    
    /// <summary>
    /// 覆盖描述文本
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// 覆盖图标路径
    /// </summary>
    public string Icon { get; set; } = string.Empty;
    
    /// <summary>
    /// 覆盖标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 货币物品哈希值，用于显示购买所需的货币
    /// </summary>
    public uint CurrencyItemHash { get; set; }
}

/// <summary>
/// 命运2商人显示分类定义，控制商人界面中分类的显示方式
/// </summary>
public class Destiny2ManifestDisplayCategoryDefinitionDto
{
    /// <summary>
    /// 显示索引，决定分类在界面中的位置
    /// </summary>
    public int Index { get; set; }
    
    /// <summary>
    /// 标识符，分类的唯一标识字符串
    /// </summary>
    public string Identifier { get; set; } = string.Empty;
    
    /// <summary>
    /// 显示分类哈希值，用于在API中引用此分类
    /// </summary>
    public uint DisplayCategoryHash { get; set; }
    
    /// <summary>
    /// 显示属性，包含分类的名称、描述和图标等信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 是否显示横幅
    /// </summary>
    public bool DisplayBanner { get; set; }
    
    /// <summary>
    /// 进度哈希值，关联到此分类的进度系统
    /// </summary>
    public uint ProgressionHash { get; set; }
    
    /// <summary>
    /// 排序顺序，决定分类内物品的排列方式
    /// </summary>
    public Destiny2VendorDisplayCategorySortOrderEnum SortOrder { get; set; }
    
    /// <summary>
    /// 显示样式哈希值，关联到特定的UI样式
    /// </summary>
    public uint DisplayStyleHash { get; set; }
    
    /// <summary>
    /// 显示样式标识符
    /// </summary>
    public string DisplayStyleIdentifier { get; set; } = string.Empty;
}

/// <summary>
/// 命运2商人交互定义，描述与商人的对话和交互选项
/// </summary>
public class Destiny2ManifestVendorInteractionDefinitionDto
{
    /// <summary>
    /// 交互索引，在商人所有交互中的位置
    /// </summary>
    public int InteractionIndex { get; set; }
    
    /// <summary>
    /// 回复列表，玩家可选择的交互回复选项
    /// </summary>
    public List<Destiny2ManifestVendorInteractionReplyDefinitionDto> Replies { get; set; } = [];
    
    /// <summary>
    /// 商人分类索引，关联到特定的商品分类
    /// </summary>
    public int VendorCategoryIndex { get; set; }
    
    /// <summary>
    /// 任务线物品哈希值，关联到此交互的任务
    /// </summary>
    public uint QuestlineItemHash { get; set; }
    
    /// <summary>
    /// 奖励包交互列表
    /// </summary>
    public List<Destiny2ManifestVendorInteractionSackEntryDefinitionDto> SackInteractionList { get; set; } = [];
    
    /// <summary>
    /// UI交互类型，决定使用哪种界面模板
    /// </summary>
    public uint UiInteractionType { get; set; }
    
    /// <summary>
    /// 交互类型，定义交互的行为模式
    /// </summary>
    public int InteractionType { get; set; }
    
    /// <summary>
    /// 奖励区块标签文本
    /// </summary>
    public string RewardBlockLabel { get; set; } = string.Empty;
    
    /// <summary>
    /// 奖励商人分类索引，关联到奖励物品所在的分类
    /// </summary>
    public int RewardVendorCategoryIndex { get; set; }
    
    /// <summary>
    /// 风味文本一，用于对话或描述的第一行
    /// </summary>
    public string FlavorLineOne { get; set; } = string.Empty;
    
    /// <summary>
    /// 风味文本二，用于对话或描述的第二行
    /// </summary>
    public string FlavorLineTwo { get; set; } = string.Empty;
    
    /// <summary>
    /// 标题显示属性，包含交互标题的名称、描述和图标
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto HeaderDisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 交互指引文本，提示玩家如何与商人互动
    /// </summary>
    public string Instructions { get; set; } = string.Empty;
}

/// <summary>
/// 命运2商人交互回复定义，描述玩家对商人交互的回应选项
/// </summary>
public class Destiny2ManifestVendorInteractionReplyDefinitionDto
{
    /// <summary>
    /// 物品奖励选择索引，关联到可能获得的奖励
    /// </summary>
    public int ItemRewardsSelectionIndex { get; set; }
    
    /// <summary>
    /// 回复文本，玩家选择的回复内容
    /// </summary>
    public string Reply { get; set; } = string.Empty;
    
    /// <summary>
    /// 回复类型，定义回复的行为（接受、拒绝、完成）
    /// </summary>
    public Destiny2VendorReplyTypeEnum ReplyType { get; set; }
}

/// <summary>
/// 命运2商人交互奖励包条目定义，描述交互中可获得的奖励包
/// </summary>
public class Destiny2ManifestVendorInteractionSackEntryDefinitionDto
{
    /// <summary>
    /// 奖励包类型，定义奖励包的内容结构
    /// </summary>
    public uint SackType { get; set; }
}

/// <summary>
/// 命运2商人库存弹出窗口定义，描述商人界面中的弹出式库存界面
/// </summary>
public class Destiny2ManifestVendorInventoryFlyoutDefinitionDto
{
    /// <summary>
    /// 锁定状态下的描述文本，当弹出窗口不可用时显示
    /// </summary>
    public string LockedDescription { get; set; } = string.Empty;
    
    /// <summary>
    /// 显示属性，包含弹出窗口的名称、描述和图标等基本显示信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 物品分组列表，定义弹出窗口中物品的分类展示方式
    /// </summary>
    public List<Destiny2ManifestVendorInventoryFlyoutBucketDefinitionDto> Buckets { get; set; } = [];
    
    /// <summary>
    /// 弹出窗口ID，用于唯一标识此弹出窗口
    /// </summary>
    public uint FlyoutId { get; set; }
    
    /// <summary>
    /// 是否隐藏新物品标记，控制新获得物品的"新"标签显示
    /// </summary>
    public bool SuppressNewness { get; set; }
    
    /// <summary>
    /// 装备槽位哈希值，关联到此弹出窗口对应的装备槽位
    /// </summary>
    public uint EquipmentSlotHash { get; set; }
}

/// <summary>
/// 命运2商人库存弹出窗口分组定义，描述弹出窗口中的物品分组
/// </summary>
public class Destiny2ManifestVendorInventoryFlyoutBucketDefinitionDto
{
    /// <summary>
    /// 是否可折叠，控制此分组是否可以在UI中折叠显示
    /// </summary>
    public bool Collapsible { get; set; }
    
    /// <summary>
    /// 库存分组哈希值，关联到游戏中的库存分组定义
    /// </summary>
    public uint InventoryBucketHash { get; set; }
    
    /// <summary>
    /// 物品排序方式，定义分组中物品的排序规则
    /// </summary>
    public int SortItemsBy { get; set; }
}

/// <summary>
/// 命运2商人物品定义，描述商人出售的单个物品
/// </summary>
public class Destiny2ManifestVendorItemDefinitionDto
{
    /// <summary>
    /// 商人物品索引，在商人所有物品中的位置
    /// </summary>
    public int VendorItemIndex { get; set; }
    
    /// <summary>
    /// 物品哈希值，关联到此物品的详细定义
    /// </summary>
    public uint ItemHash { get; set; }
    
    /// <summary>
    /// 物品数量，购买时获得的数量
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// 失败索引列表，关联到阻止购买此物品的条件
    /// </summary>
    public List<int> FailureIndexes { get; set; } = [];
    
    /// <summary>
    /// 货币列表，购买此物品所需的货币和数量
    /// </summary>
    public List<Destiny2ManifestVendorItemQuantityDto> Currencies { get; set; } = [];
    
    /// <summary>
    /// 退款政策，定义物品的退款规则
    /// </summary>
    public int RefundPolicy { get; set; }
    
    /// <summary>
    /// 退款时限（秒），可退款的最长时间
    /// </summary>
    public int RefundTimeLimit { get; set; }
    
    /// <summary>
    /// 创建等级列表，物品可能的创建等级设置
    /// </summary>
    public List<Destiny2ManifestItemCreationEntryLevelDefinitionDto> CreationLevels { get; set; } = [];
    
    /// <summary>
    /// 显示分类索引，关联到物品在UI中显示的分类
    /// </summary>
    public int DisplayCategoryIndex { get; set; }
    
    /// <summary>
    /// 分类索引，关联到物品所属的内部分类
    /// </summary>
    public int CategoryIndex { get; set; }
    
    /// <summary>
    /// 原始分类索引，物品的默认分类设置
    /// </summary>
    public int OriginalCategoryIndex { get; set; }
    
    /// <summary>
    /// 最低等级要求，购买此物品所需的最低玩家等级
    /// </summary>
    public int MinimumLevel { get; set; }
    
    /// <summary>
    /// 最高等级限制，超过此等级无法购买此物品
    /// </summary>
    public int MaximumLevel { get; set; }

    /// <summary>
    /// 物品动作定义，描述购买物品时的动作行为
    /// </summary>
    public Destiny2ManifestVendorSaleItemActionBlockDefinitionDto Action { get; set; } = new();
    
    /// <summary>
    /// 显示分类名称，物品在UI中显示的分类名称
    /// </summary>
    public string DisplayCategory { get; set; } = string.Empty;
    
    /// <summary>
    /// 库存分组哈希值，关联到物品所属的库存分组
    /// </summary>
    public uint InventoryBucketHash { get; set; }
    
    /// <summary>
    /// 可见性范围，控制哪些玩家可以看到此物品
    /// </summary>
    public Destiny2GatingScopeEnum VisibilityScope { get; set; }
    
    /// <summary>
    /// 可购买范围，控制哪些玩家可以购买此物品
    /// </summary>
    public Destiny2GatingScopeEnum PurchasableScope { get; set; }
    
    /// <summary>
    /// 排他性，定义物品的独占或限制级别
    /// </summary>
    public int Exclusivity { get; set; }
    
    /// <summary>
    /// 是否为特殊优惠物品
    /// </summary>
    public bool IsOffer { get; set; }
    
    /// <summary>
    /// 是否为CRM（客户关系管理）物品，通常与真实货币购买相关
    /// </summary>
    public bool IsCrm { get; set; }
    
    /// <summary>
    /// 排序值，决定物品在商人界面中的显示顺序
    /// </summary>
    public int SortValue { get; set; }
    
    /// <summary>
    /// 过期提示文本，显示物品何时将不再可用
    /// </summary>
    public string ExpirationTooltip { get; set; } = string.Empty;
    
    /// <summary>
    /// 重定向销售索引列表，将购买行为重定向到其他物品
    /// </summary>
    public List<int> RedirectToSaleIndexes { get; set; } = [];
    
    /// <summary>
    /// 插槽覆盖列表，自定义物品的插槽选项
    /// </summary>
    public List<Destiny2ManifestVendorItemSocketOverrideDto> SocketOverrides { get; set; } = [];
    
    /// <summary>
    /// 是否不可购买，即使显示也无法购买
    /// </summary>
    public bool Unpurchasable { get; set; }
}

/// <summary>
/// 命运2商人物品数量，描述物品的数量和条件
/// </summary>
public class Destiny2ManifestVendorItemQuantityDto
{
    /// <summary>
    /// 物品哈希值，关联到物品的详细定义
    /// </summary>
    public uint ItemHash { get; set; }
    
    /// <summary>
    /// 物品实例ID，标识特定的物品实例
    /// </summary>
    public long ItemInstanceId { get; set; }
    
    /// <summary>
    /// 数量，物品的数量值
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// 是否有条件可见性，控制物品是否根据条件显示
    /// </summary>
    public bool HasConditionalVisibility { get; set; }
}

/// <summary>
/// 命运2物品创建等级条目定义，描述物品的创建等级
/// </summary>
public class Destiny2ManifestItemCreationEntryLevelDefinitionDto
{
    /// <summary>
    /// 等级值，物品创建时的等级设置
    /// </summary>
    public int Level { get; set; }
}

/// <summary>
/// 命运2商人销售物品动作块定义，描述物品购买动作的行为
/// </summary>
public class Destiny2ManifestVendorSaleItemActionBlockDefinitionDto
{
    /// <summary>
    /// 执行时间（秒），完成购买动作所需的时间
    /// </summary>
    public float ExecuteSeconds { get; set; }
    
    /// <summary>
    /// 是否为积极动作，影响UI显示样式
    /// </summary>
    public bool IsPositive { get; set; }
}

/// <summary>
/// 命运2商人物品插槽覆盖定义，自定义商人物品的插槽选项
/// </summary>
public class Destiny2ManifestVendorItemSocketOverrideDto
{
    /// <summary>
    /// 单一物品哈希值，指定插槽中的固定物品
    /// </summary>
    public uint SingleItemHash { get; set; }
    
    /// <summary>
    /// 随机选项数量，插槽可随机生成的选项数
    /// </summary>
    public int RandomizedOptionsCount { get; set; }
    
    /// <summary>
    /// 插槽类型哈希值，关联到插槽类型定义
    /// </summary>
    public uint SocketTypeHash { get; set; }
}

/// <summary>
/// 命运2商人服务定义，描述商人提供的特殊服务
/// </summary>
public class Destiny2ManifestVendorServiceDefinitionDto
{
    /// <summary>
    /// 服务名称，描述商人提供的服务类型
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// 命运2商人接受物品定义，描述商人可接收交换的物品类型
/// </summary>
public class Destiny2ManifestVendorAcceptedItemDefinitionDto
{
    /// <summary>
    /// 接受库存分组哈希值，商人可接收的物品所属分组
    /// </summary>
    public uint AcceptedInventoryBucketHash { get; set; }
    
    /// <summary>
    /// 目标库存分组哈希值，物品交换后放置的目标分组
    /// </summary>
    public uint DestinationInventoryBucketHash { get; set; }
}

/// <summary>
/// 命运2商人位置定义，描述商人在游戏世界中的位置
/// </summary>
public class Destiny2ManifestVendorLocationDefinitionDto
{
    /// <summary>
    /// 目的地哈希值，关联到商人所在的区域或星球
    /// </summary>
    public uint DestinationHash { get; set; }
    
    /// <summary>
    /// 背景图片路径，商人所在位置的背景图
    /// </summary>
    public string BackgroundImagePath { get; set; } = string.Empty;
}

/// <summary>
/// 命运2商人组引用，描述商人所属的分组关系
/// </summary>
public class Destiny2ManifestVendorGroupReferenceDto
{
    /// <summary>
    /// 商人组哈希值，关联到商人所属的分组定义
    /// </summary>
    public uint VendorGroupHash { get; set; }
}

/// <summary>
/// 命运2商人清单，包含所有商人定义的字典集合
/// </summary>
public class Destiny2ManifestVendorDto : Dictionary<string, Destiny2ManifestVendorDefinitionDto>
{
}