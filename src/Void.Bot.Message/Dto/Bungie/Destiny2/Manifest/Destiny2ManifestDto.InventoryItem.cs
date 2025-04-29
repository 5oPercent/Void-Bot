using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 物品清单定义，包含物品的详细信息和属性
/// </summary>
public class Destiny2ManifestInventoryItemDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，如名称、描述和图标
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 物品提示通知列表
    /// </summary>
    public List<Destiny2ManifestDisplayStringDto> TooltipNotifications { get; set; } = [];

    /// <summary>
    /// 收藏品哈希，关联到收藏品系统
    /// </summary>
    public uint CollectibleHash { get; set; }

    /// <summary>
    /// 图标水印，用于在图标上添加额外的视觉效果
    /// </summary>
    public string IconWatermark { get; set; } = string.Empty;
    
    /// <summary>
    /// 被搁置时的图标水印
    /// </summary>
    public string IconWatermarkShelved { get; set; } = string.Empty;
    
    /// <summary>
    /// 次要图标路径
    /// </summary>
    public string SecondaryIcon { get; set; } = string.Empty;
    
    /// <summary>
    /// 次要覆盖图路径
    /// </summary>
    public string SecondaryOverlay { get; set; } = string.Empty;
    
    /// <summary>
    /// 次要特殊图标路径
    /// </summary>
    public string SecondarySpecial { get; set; } = string.Empty;

    /// <summary>
    /// 物品背景颜色定义
    /// </summary>
    public Destiny2ManifestColorDefinitionDto BackgroundColor { get; set; } = new();
    
    /// <summary>
    /// 截图路径
    /// </summary>
    public string Screenshot { get; set; } = string.Empty;
    
    /// <summary>
    /// 物品类型显示名称
    /// </summary>
    public string ItemTypeDisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// 物品描述文本
    /// </summary>
    public string FlavorText { get; set; } = string.Empty;
    
    /// <summary>
    /// UI物品显示样式
    /// </summary>
    public string UiItemDisplayStyle { get; set; } = string.Empty;
    
    /// <summary>
    /// 物品类型和等级显示名称
    /// </summary>
    public string ItemTypeAndTierDisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// 显示来源信息
    /// </summary>
    public string DisplaySource { get; set; } = string.Empty;
    
    /// <summary>
    /// 提示样式
    /// </summary>
    public string TooltipStyle { get; set; } = string.Empty;

    /// <summary>
    /// 物品行动块，定义物品可执行的操作
    /// </summary>
    public Destiny2ManifestItemActionBlockDto Action { get; set; } = new();
    
    /// <summary>
    /// 物品制作块，定义物品的制作相关属性
    /// </summary>
    public Destiny2ManifestDisplayStringDto Crafting { get; set; } = new();
    
    /// <summary>
    /// 物品库存块，定义物品在库存中的属性
    /// </summary>
    public Destiny2ManifestItemInventoryBlockDto Inventory { get; set; } = new();
    
    /// <summary>
    /// 物品集合数据，定义物品所属的集合
    /// </summary>
    public Destiny2ManifestItemSetBlockDto SetData { get; set; } = new();
    
    /// <summary>
    /// 物品属性块，定义物品的统计属性
    /// </summary>
    public Destiny2ManifestItemStatBlockDto Stats { get; set; } = new();

    /// <summary>
    /// 徽章目标哈希
    /// </summary>
    public uint EmblemObjectiveHash { get; set; }
    
    /// <summary>
    /// 装备块定义列表
    /// </summary>
    public List<Destiny2ManifestEquippingBlockDefinition> EquippingBlock { get; set; } = [];

    /// <summary>
    /// 物品翻译块定义列表
    /// </summary>
    public List<Destiny2ManifestItemTranslationBlockDefinitionDto> TranslationBlock { get; set; } = [];

    /// <summary>
    /// 物品预览块定义列表
    /// </summary>
    public List<Destiny2ManifestItemPreviewBlockDefinitionDto> Preview { get; set; } = [];
    
    /// <summary>
    /// 物品质量块定义列表
    /// </summary>
    public List<Destiny2ManifestItemQualityBlockDefinitionDto> Quality { get; set; } = [];
    
    /// <summary>
    /// 物品价值块定义列表
    /// </summary>
    public List<Destiny2ManifestItemValueBlockDefinitionDto> Value { get; set; } = [];
    
    /// <summary>
    /// 物品来源块定义列表
    /// </summary>
    public List<Destiny2ManifestItemSourceBlockDefinitionDto> SourceData { get; set; } = [];
    
    /// <summary>
    /// 物品目标块定义列表
    /// </summary>
    public List<Destiny2ManifestItemObjectiveBlockDefinitionDto> Objectives { get; set; } = [];
    
    /// <summary>
    /// 物品指标块定义列表
    /// </summary>
    public List<Destiny2ManifestItemMetricBlockDefinitionDto> Metrics { get; set; } = [];
    
    /// <summary>
    /// 物品插件定义
    /// </summary>
    public Destiny2ManifestItemPlugDefinitionDto Plug { get; set; } = new();
    
    /// <summary>
    /// 物品装备组定义
    /// </summary>
    public Destiny2ManifestItemGearsetBlockDefinitionDto Gearset { get; set; } = new();
    
    /// <summary>
    /// 物品包定义
    /// </summary>
    public Destiny2ManifestItemSackBlockDefinitionDto Sack { get; set; } = new();
    
    /// <summary>
    /// 物品插槽块定义
    /// </summary>
    public Destiny2ManifestItemSocketBlockDefinitionDto Sockets { get; set; } = new();
    
    /// <summary>
    /// 物品摘要块定义
    /// </summary>
    public Destiny2ManifestItemSummaryBlockDefinitionDto Summary { get; set; } = new();
    
    /// <summary>
    /// 物品天赋网格块定义
    /// </summary>
    public Destiny2ManifestItemTalentGridBlockDefinitionDto TalentGrid { get; set; } = new();
    
    /// <summary>
    /// 物品投资属性定义
    /// </summary>
    public Destiny2ManifestItemInvestmentStatDefinitionDto InvestmentStats { get; set; } = new();
    
    /// <summary>
    /// 物品特性条目定义
    /// </summary>
    public Destiny2ManifestItemPerkEntryDefinitionDto Perks { get; set; } = new();
    
    /// <summary>
    /// 物品传说哈希，关联到物品的传说/故事
    /// </summary>
    public uint LoreHash { get; set; }
    
    /// <summary>
    /// 摘要物品哈希
    /// </summary>
    public uint SummaryItemHash { get; set; }
    
    /// <summary>
    /// 动画引用列表
    /// </summary>
    public List<Destiny2ManifestAnimationReferenceDto> Animations { get; set; } = [];
    
    /// <summary>
    /// 是否允许执行操作
    /// </summary>
    public bool AllowActions { get; set; }
    
    /// <summary>
    /// 超链接引用列表
    /// </summary>
    public List<Destiny2ManifestHyperlinkReferenceDto> Links { get; set; } = [];
    
    /// <summary>
    /// 从邮件管理员获取物品时是否有副作用
    /// </summary>
    public bool DoesPostmasterPullHaveSideEffects { get; set; }
    
    /// <summary>
    /// 物品是否不可转移
    /// </summary>
    public bool NonTransferrable { get; set; }
    
    /// <summary>
    /// 物品类别哈希列表
    /// </summary>
    public List<uint> ItemCategoryHashes { get; set; } = [];
    
    /// <summary>
    /// 特殊物品类型
    /// </summary>
    public Destiny2SpecialItemTypeEnum SpecialItemType { get; set; }
    
    /// <summary>
    /// 物品类型
    /// </summary>
    public Destiny2ItemTypeEnum ItemType { get; set; }
    
    /// <summary>
    /// 物品子类型
    /// </summary>
    public Destiny2ItemSubTypeEnum ItemSubType { get; set; }
    
    /// <summary>
    /// 职业类型
    /// </summary>
    public Destiny2ClassEnum Class { get; set; }
    
    /// <summary>
    /// 破坏者类型
    /// </summary>
    public Destiny2BreakerTypeEnum BucketType { get; set; }
    
    /// <summary>
    /// 破坏者类型哈希
    /// </summary>
    public uint BreakerTypeHash { get; set; }
    
    /// <summary>
    /// 是否可装备
    /// </summary>
    public bool Equippable { get; set; }

    /// <summary>
    /// 伤害类型哈希列表
    /// </summary>
    public List<uint> DamageTypeHashes { get; set; } = [];

    /// <summary>
    /// 伤害类型列表
    /// </summary>
    public List<Destiny2DamageTypeEnum> DamageTypes { get; set; } = [];
    
    /// <summary>
    /// 默认伤害类型
    /// </summary>
    public Destiny2DamageTypeEnum DefaultDamageType { get; set; }
    
    /// <summary>
    /// 默认伤害类型哈希
    /// </summary>
    public uint DefaultDamageTypeHash { get; set; }
    
    /// <summary>
    /// 赛季哈希
    /// </summary>
    public uint SeasonHash { get; set; }
    
    /// <summary>
    /// 是否为包装器
    /// </summary>
    public bool IsWrapper { get; set; }
    
    /// <summary>
    /// 特性ID列表
    /// </summary>
    public List<string> TraitIds { get; set; } = [];
    
    /// <summary>
    /// 特性哈希列表
    /// </summary>
    public List<uint> TraitHashes { get; set; } = [];
}

/// <summary>
/// 物品行动块，定义物品可执行的操作相关信息
/// </summary>
public class Destiny2ManifestItemActionBlockDto
{
    /// <summary>
    /// 动作动词名称
    /// </summary>
    public string VerbName { get; set; } = string.Empty;

    /// <summary>
    /// 动作动词描述
    /// </summary>
    public string VerbDescription { get; set; } = string.Empty;

    /// <summary>
    /// 是否为积极的动作
    /// </summary>
    public bool IsPositive { get; set; }

    /// <summary>
    /// 覆盖屏幕名称
    /// </summary>
    public string OverlayScreenName { get; set; } = string.Empty;

    /// <summary>
    /// 覆盖图标路径
    /// </summary>
    public string OverlayIcon { get; set; } = string.Empty;

    /// <summary>
    /// 所需冷却时间（秒）
    /// </summary>
    public int RequiredCooldownSeconds { get; set; }

    /// <summary>
    /// 执行动作所需物品列表
    /// </summary>
    public List<Destiny2ManifestItemActionRequiredItemDto> RequiredItems { get; set; } = [];

    /// <summary>
    /// 进度奖励定义列表
    /// </summary>
    public List<Destiny2ManifestProgressionRewardDefinitionDto> ProgressionRewards { get; set; } = [];

    /// <summary>
    /// 动作类型标签
    /// </summary>
    public string ActionTypeLabel { get; set; } = string.Empty;

    /// <summary>
    /// 所需位置
    /// </summary>
    public string RequiredLocation { get; set; } = string.Empty;

    /// <summary>
    /// 所需冷却哈希
    /// </summary>
    public uint RequiredCooldownHash { get; set; }

    /// <summary>
    /// 动作后是否删除物品
    /// </summary>
    public bool DeleteOnAction { get; set; }

    /// <summary>
    /// 是否消耗整个堆栈
    /// </summary>
    public bool ConsumeEntireStack { get; set; }

    /// <summary>
    /// 获取时是否立即使用
    /// </summary>
    public bool UseOnAcquire { get; set; }
}

/// <summary>
/// 物品动作所需的物品定义
/// </summary>
public class Destiny2ManifestItemActionRequiredItemDto
{
    /// <summary>
    /// 所需数量
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 所需物品哈希
    /// </summary>
    public uint ItemHash { get; set; }

    /// <summary>
    /// 动作后是否删除
    /// </summary>
    public bool DeleteOnAction { get; set; }
}

/// <summary>
/// 物品库存块定义，包含物品在库存中的属性和行为
/// </summary>
public class Destiny2ManifestItemInventoryBlockDto
{
    /// <summary>
    /// 堆叠唯一标签
    /// </summary>
    public string StackUniqueLabel { get; set; } = string.Empty;

    /// <summary>
    /// 最大堆叠数量
    /// </summary>
    public int MaxStackSize { get; set; }

    /// <summary>
    /// 存储分类哈希
    /// </summary>
    public uint BucketTypeHash { get; set; }

    /// <summary>
    /// 恢复存储分类哈希
    /// </summary>
    public uint RecoveryBucketTypeHash { get; set; }

    /// <summary>
    /// 等级类型哈希
    /// </summary>
    public uint TierTypeHash { get; set; }

    /// <summary>
    /// 是否为实例物品
    /// </summary>
    public bool IsInstanceItem { get; set; }

    /// <summary>
    /// 等级类型名称
    /// </summary>
    public string TierTypeName { get; set; } = string.Empty;

    /// <summary>
    /// 等级类型
    /// </summary>
    public Destiny2TierTypeEnum TierType { get; set; }

    /// <summary>
    /// 过期提示文本
    /// </summary>
    public string ExpirationTooltip { get; set; } = string.Empty;

    /// <summary>
    /// 在活动中过期的消息
    /// </summary>
    public string ExpiredInActivityMessage { get; set; } = string.Empty;

    /// <summary>
    /// 在轨道中过期的消息
    /// </summary>
    public string ExpiredInOrbitMessage { get; set; } = string.Empty;

    /// <summary>
    /// 完成目标时是否抑制过期
    /// </summary>
    public bool SuppressExpirationWhenObjectivesComplete { get; set; }

    /// <summary>
    /// 配方物品哈希
    /// </summary>
    public uint RecipeItemHash { get; set; }
}

/// <summary>
/// 物品集合块定义，用于定义物品所属的集合及其特性
/// </summary>
public class Destiny2ManifestItemSetBlockDto
{
    /// <summary>
    /// 集合中的物品列表
    /// </summary>
    public List<Destiny2ManifestItemSetBlockEntryDto> ItemList { get; set; } = [];

    /// <summary>
    /// 是否需要按顺序添加集合物品
    /// </summary>
    public bool RequireOrderedSetItemAdd { get; set; }

    /// <summary>
    /// 集合是否为特色
    /// </summary>
    public bool SetIsFeatured { get; set; }

    /// <summary>
    /// 集合类型
    /// </summary>
    public string SetType { get; set; } = string.Empty;

    /// <summary>
    /// 任务线名称
    /// </summary>
    public string QuestLineName { get; set; } = string.Empty;

    /// <summary>
    /// 任务线描述
    /// </summary>
    public string QuestLineDescription { get; set; } = string.Empty;

    /// <summary>
    /// 任务步骤摘要
    /// </summary>
    public string QuestStepSummary { get; set; } = string.Empty;
}

/// <summary>
/// 物品集合块条目定义，代表集合中的单个物品
/// </summary>
public class Destiny2ManifestItemSetBlockEntryDto
{
    /// <summary>
    /// 跟踪值
    /// </summary>
    public int TrackingValue { get; set; }

    /// <summary>
    /// 物品哈希
    /// </summary>
    public uint ItemHash { get; set; }
}

/// <summary>
/// 物品统计块定义，包含物品的统计属性信息
/// </summary>
public class Destiny2ManifestItemStatBlockDto
{
    /// <summary>
    /// 是否禁用主要统计显示
    /// </summary>
    public bool DisablePrimaryStatDisplay { get; set; }

    /// <summary>
    /// 统计组哈希
    /// </summary>
    public uint StatGroupHash { get; set; }

    /// <summary>
    /// 统计属性字典，键为统计哈希，值为统计定义
    /// </summary>
    public Dictionary<uint, Destiny2ManifestInventoryItemStatDefinitionDto> Stats { get; set; } = new();

    /// <summary>
    /// 是否有可显示的统计
    /// </summary>
    public bool HasDisplayableStats { get; set; }

    /// <summary>
    /// 主要基础统计哈希
    /// </summary>
    public uint PrimaryBaseStatHash { get; set; }
}

/// <summary>
/// 物品统计属性定义，描述单个统计属性的详细信息
/// </summary>
public class Destiny2ManifestInventoryItemStatDefinitionDto
{
    /// <summary>
    /// 统计属性哈希
    /// </summary>
    public uint StatHash { get; set; }

    /// <summary>
    /// 统计属性值
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// 最小值
    /// </summary>
    public int Minimum { get; set; }

    /// <summary>
    /// 最大值
    /// </summary>
    public int Maximum { get; set; }

    /// <summary>
    /// 显示的最大值
    /// </summary>
    public int DisplayMaximum { get; set; }
}

/// <summary>
/// 物品翻译块定义，用于处理物品的视觉表现和翻译
/// </summary>
public class Destiny2ManifestItemTranslationBlockDefinitionDto
{
    /// <summary>
    /// 武器模式唯一标识符
    /// </summary>
    public string UniqweaponPatternIdentifierueLabel { get; set; } = string.Empty;

    /// <summary>
    /// 武器模式哈希
    /// </summary>
    public uint WeaponPatternHash { get; set; }

    /// <summary>
    /// 默认染料列表
    /// </summary>
    public List<Destiny2ManifestDyeReferenceDto> DefaultDyes { get; set; } = [];

    /// <summary>
    /// 锁定染料列表
    /// </summary>
    public List<Destiny2ManifestDyeReferenceDto> LockedDyes { get; set; } = [];

    /// <summary>
    /// 自定义染料列表
    /// </summary>
    public List<Destiny2ManifestDyeReferenceDto> CustomDyes { get; set; } = [];

    /// <summary>
    /// 装备艺术排列引用列表
    /// </summary>
    public List<Destiny2ManifestGearArtArrangementReferenceDto> Arrangements { get; set; } = [];

    /// <summary>
    /// 是否有几何体数据
    /// </summary>
    public bool HasGeometry { get; set; }
}

/// <summary>
/// 物品预览块定义，控制物品在预览时的显示方式和内容
/// </summary>
public class Destiny2ManifestItemPreviewBlockDefinitionDto
{
    /// <summary>
    /// 屏幕样式
    /// </summary>
    public string ScreenStyle { get; set; } = string.Empty;

    /// <summary>
    /// 预览商人哈希
    /// </summary>
    public uint PreviewVendorHash { get; set; }

    /// <summary>
    /// 神器哈希
    /// </summary>
    public uint ArtifactHash { get; set; }

    /// <summary>
    /// 预览动作字符串
    /// </summary>
    public string PreviewActionString { get; set; } = string.Empty;

    /// <summary>
    /// 派生物品分类列表
    /// </summary>
    public List<Destiny2ManifestDerivedItemCategoryDefinitionDto> DerivedItemCategories { get; set; } = [];
}

/// <summary>
/// 物品质量块定义，描述物品的品质等级和提升系统
/// </summary>
public class Destiny2ManifestItemQualityBlockDefinitionDto
{
    /// <summary>
    /// 物品等级列表
    /// </summary>
    public List<int> ItemLevels { get; set; } = [];

    /// <summary>
    /// 质量等级
    /// </summary>
    public int QualityLevel { get; set; }

    /// <summary>
    /// 注入类别名称
    /// </summary>
    public string InfusionCategoryName { get; set; } = string.Empty;

    /// <summary>
    /// 注入类别哈希
    /// </summary>
    public uint InfusionCategoryHash { get; set; }

    /// <summary>
    /// 注入类别哈希列表
    /// </summary>
    public List<uint> InfusionCategoryHashes { get; set; } = [];

    /// <summary>
    /// 进度等级需求哈希列表
    /// </summary>
    public List<uint> ProgressionLevelRequirementHash { get; set; } = [];

    /// <summary>
    /// 当前版本
    /// </summary>
    public uint CurrentVersion { get; set; }

    /// <summary>
    /// 版本列表
    /// </summary>
    public List<Destiny2ManifestItemVersionDefinitionDto> Versions { get; set; } = [];

    /// <summary>
    /// 显示版本水印图标列表
    /// </summary>
    public List<string> DisplayVersionWatermarkIcons { get; set; } = [];
}

/// <summary>
/// 物品版本定义，描述物品的不同版本信息
/// </summary>
public class Destiny2ManifestItemVersionDefinitionDto
{
    /// <summary>
    /// 能力上限哈希
    /// </summary>
    public uint PowerCapHash { get; set; }
}

/// <summary>
/// 物品价值块定义，描述物品的价值和兑换信息
/// </summary>
public class Destiny2ManifestItemValueBlockDefinitionDto
{
    /// <summary>
    /// 物品价值列表
    /// </summary>
    public List<Destiny2ManifestItemQuantityDto> ItemValue { get; set; } = [];

    /// <summary>
    /// 价值描述
    /// </summary>
    public string ValueDescription { get; set; } = string.Empty;
}

/// <summary>
/// 物品数量定义，描述物品的数量信息
/// </summary>
public class Destiny2ManifestItemQuantityDto
{
    /// <summary>
    /// 物品哈希
    /// </summary>
    public uint ItemHash { get; set; }

    /// <summary>
    /// 物品实例ID
    /// </summary>
    public long ItemInstanceId { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 是否有条件可见性
    /// </summary>
    public bool HasConditionalVisibility { get; set; }
}
/// <summary>
/// 物品来源块定义，描述物品的获取途径和来源信息
/// </summary>
public class Destiny2ManifestItemSourceBlockDefinitionDto
{
    /// <summary>
    /// 来源哈希列表，标识物品的来源
    /// </summary>
    public List<uint> SourceHashes { get; set; } = [];
    
    /// <summary>
    /// 物品来源定义列表，详细描述物品的获取来源
    /// </summary>
    public List<Destiny2ManifestItemSourceDefinitionDto> Sources { get; set; } = [];
    
    /// <summary>
    /// 排他性标识，指示物品来源的排他性级别
    /// </summary>
    public int Exclusive { get; set; }
    
    /// <summary>
    /// 商人来源引用列表，描述可以从哪些商人处获取该物品
    /// </summary>
    public List<Destiny2ManifestItemVendorSourceReferenceDto> VendorSources { get; set; } = [];
}

/// <summary>
/// 物品来源定义，详细描述物品的特定来源信息和获取条件
/// </summary>
public class Destiny2ManifestItemSourceDefinitionDto
{
    /// <summary>
    /// 来源等级
    /// </summary>
    public int Level { get; set; }
    
    /// <summary>
    /// 最小获取数量
    /// </summary>
    public int MinQuantity { get; set; }
    
    /// <summary>
    /// 最大获取数量
    /// </summary>
    public int MaxQuantity { get; set; }

    /// <summary>
    /// 获取所需的最小等级
    /// </summary>
    public int MinLevelRequired { get; set; }

    /// <summary>
    /// 获取所需的最大等级
    /// </summary>
    public int MaxLevelRequired { get; set; }
    
    /// <summary>
    /// 计算的统计属性字典，键为统计哈希，值为统计定义
    /// </summary>
    public Dictionary<uint, Destiny2ManifestInventoryItemStatDefinitionDto> ComputedStats { get; set; } = new();
    
    /// <summary>
    /// 奖励来源哈希字典，键为来源哈希，值为奖励来源定义
    /// </summary>
    public Dictionary<uint, Destiny2ManifestRewardSourceDefinitionDto> SourceHashes { get; set; } = new();
}

/// <summary>
/// 物品商人来源引用，描述特定商人处可获取的物品信息
/// </summary>
public class Destiny2ManifestItemVendorSourceReferenceDto
{
    /// <summary>
    /// 商人哈希，用于标识特定的商人
    /// </summary>
    public uint VendorHash { get; set; }

    /// <summary>
    /// 商人物品索引列表，指示物品在商人库存中的位置
    /// </summary>
    public List<int> VendorItemIndexes { get; set; } = [];
}

/// <summary>
/// 物品目标块定义，描述物品相关的任务和目标信息
/// </summary>
public class Destiny2ManifestItemObjectiveBlockDefinitionDto
{
    /// <summary>
    /// 目标哈希列表，关联到具体的目标定义
    /// </summary>
    public List<uint> ObjectiveHashes { get; set; } = [];
    
    /// <summary>
    /// 显示活动哈希列表，指示目标相关的活动
    /// </summary>
    public List<uint> DisplayActivityHashes { get; set; } = [];

    /// <summary>
    /// 是否需要完成所有目标才能视为完成
    /// </summary>
    public bool RequireFullObjectiveCompletion { get; set; }

    /// <summary>
    /// 任务线物品哈希，关联到此目标所属的任务线
    /// </summary>
    public int QuestlineItemHash { get; set; }
    
    /// <summary>
    /// 叙事文本，描述目标的背景故事
    /// </summary>
    public string Narrative { get; set; } = string.Empty;
    
    /// <summary>
    /// 任务类型标识符
    /// </summary>
    public string QuestTypeIdentifier { get; set; } = string.Empty;
    
    /// <summary>
    /// 任务类型哈希
    /// </summary>
    public uint QuestTypeHash { get; set; }

    /// <summary>
    /// 每个目标的显示属性列表
    /// </summary>
    public List<Destiny2ManifestObjectiveDisplayPropertiesDto> PerObjectiveDisplayProperties { get; set; } = [];
    
    /// <summary>
    /// 是否显示为统计跟踪器
    /// </summary>
    public bool DisplayAsStatTracker { get; set; }
}

/// <summary>
/// 物品指标块定义，描述与物品相关的游戏内指标系统
/// </summary>
public class Destiny2ManifestItemMetricBlockDefinitionDto
{
    /// <summary>
    /// 可用指标类别节点哈希列表，关联到指标系统中的类别节点
    /// </summary>
    public List<uint> AvailableMetricCategoryNodeHashes { get; set; } = [];
}

/// <summary>
/// 命运2物品插件定义，描述物品可插入的插件和属性
/// </summary>
public class Destiny2ManifestItemPlugDefinitionDto
{
    /// <summary>
    /// 插入规则列表，定义何时可以插入插件
    /// </summary>
    public List<Destiny2ManifestPlugRuleDefinitionDto> InsertionRules { get; set; } = [];
    
    /// <summary>
    /// 插件类别标识符
    /// </summary>
    public string PlugCategoryIdentifier { get; set; } = string.Empty;
    
    /// <summary>
    /// 插件类别哈希
    /// </summary>
    public uint PlugCategoryHash { get; set; }
    
    /// <summary>
    /// 执行操作时是否重新创建自身
    /// </summary>
    public bool OnActionRecreateSelf { get; set; }
    
    /// <summary>
    /// 插入所需材料需求哈希
    /// </summary>
    public uint InsertionMaterialRequirementHash { get; set; }
    
    /// <summary>
    /// 预览物品覆盖哈希
    /// </summary>
    public uint PreviewItemOverrideHash { get; set; }
    
    /// <summary>
    /// 启用所需材料需求哈希
    /// </summary>
    public uint EnabledMaterialRequirementHash { get; set; }
    
    /// <summary>
    /// 启用规则列表，定义何时插件可被启用
    /// </summary>
    public List<Destiny2ManifestPlugRuleDefinitionDto> EnabledRules { get; set; } = [];
    
    /// <summary>
    /// UI插件标签
    /// </summary>
    public string UiPlugLabel { get; set; } = string.Empty;
    
    /// <summary>
    /// 插件样式
    /// </summary>
    public int PlugStyle { get; set; }
    
    /// <summary>
    /// 插件可用性
    /// </summary>
    public int PlugAvailability { get; set; }
    
    /// <summary>
    /// 备用UI插件标签
    /// </summary>
    public string AlternateUiPlugLabel { get; set; } = string.Empty;
    
    /// <summary>
    /// 备用插件样式
    /// </summary>
    public int AlternatePlugStyle { get; set; }
    
    /// <summary>
    /// 是否为虚拟插件
    /// </summary>
    public bool IsDummyPlug { get; set; }
    
    /// <summary>
    /// 父物品覆盖设置
    /// </summary>
    public Destiny2ManifestParentItemOverrideDto ParentItemOverride { get; set; } = new();
    
    /// <summary>
    /// 能量容量条目
    /// </summary>
    public Destiny2ManifestEnergyCapacityEntryDto EnergyCapacity { get; set; } = new();
    
    /// <summary>
    /// 能量消耗条目
    /// </summary>
    public Destiny2ManifestEnergyCostEntryDto EnergyCost { get; set; } = new();
}

/// <summary>
/// 命运2插件规则定义，描述插件使用的规则和限制
/// </summary>
public class Destiny2ManifestPlugRuleDefinitionDto
{
    /// <summary>
    /// 失败消息，当规则验证失败时显示
    /// </summary>
    public string FailureMessage { get; set; } = string.Empty;
}

/// <summary>
/// 命运2父物品覆盖定义，描述插件如何覆盖父物品的某些属性
/// </summary>
public class Destiny2ManifestParentItemOverrideDto
{
    /// <summary>
    /// 额外装备需求显示字符串列表
    /// </summary>
    public List<string> AdditionalEquipRequirementsDisplayStrings { get; set; } = [];
    
    /// <summary>
    /// 标记图标路径
    /// </summary>
    public string PipIcon { get; set; } = string.Empty;
}

/// <summary>
/// 命运2能量容量条目定义，描述物品的能量容量
/// </summary>
public class Destiny2ManifestEnergyCapacityEntryDto
{
    /// <summary>
    /// 容量值
    /// </summary>
    public int CapacityValue { get; set; }
    
    /// <summary>
    /// 能量类型哈希
    /// </summary>
    public uint EnergyTypeHash { get; set; }
    
    /// <summary>
    /// 能量类型枚举
    /// </summary>
    public Destiny2EnergyTypeEnum EnergyType { get; set; }
}

/// <summary>
/// 命运2能量消耗条目定义，描述物品的能量消耗
/// </summary>
public class Destiny2ManifestEnergyCostEntryDto
{
    /// <summary>
    /// 能量消耗值
    /// </summary>
    public int EnergyCost { get; set; }
    
    /// <summary>
    /// 能量类型哈希
    /// </summary>
    public uint EnergyTypeHash { get; set; }
    
    /// <summary>
    /// 能量类型枚举
    /// </summary>
    public Destiny2EnergyTypeEnum EnergyType { get; set; }
}

/// <summary>
/// 命运2物品装备组块定义，描述一组相关联的装备
/// </summary>
public class Destiny2ManifestItemGearsetBlockDefinitionDto
{
    /// <summary>
    /// 跟踪值最大数量
    /// </summary>
    public int TrackingValueMax { get; set; }

    /// <summary>
    /// 物品列表，包含装备组中的所有物品哈希
    /// </summary>
    public List<uint> ItemList { get; set; } = [];
}

/// <summary>
/// 命运2物品包块定义，描述可开启的物品包和其内容
/// </summary>
public class Destiny2ManifestItemSackBlockDefinitionDto
{
    /// <summary>
    /// 详情动作
    /// </summary>
    public string DetailAction { get; set; } = string.Empty;
    
    /// <summary>
    /// 打开动作
    /// </summary>
    public string OpenAction { get; set; } = string.Empty;
    
    /// <summary>
    /// 可选择的物品数量
    /// </summary>
    public int SelectItemCount { get; set; }
    
    /// <summary>
    /// 商人包类型
    /// </summary>
    public string VendorSackType { get; set; } = string.Empty;
    
    /// <summary>
    /// 获取时是否自动打开
    /// </summary>
    public string OpenOnAcquire { get; set; } = string.Empty;
}

/// <summary>
/// 命运2物品插槽块定义，描述物品上的插槽系统及其配置
/// </summary>
public class Destiny2ManifestItemSocketBlockDefinitionDto
{
    /// <summary>
    /// 详细说明
    /// </summary>
    public string Detail { get; set; } = string.Empty;

    /// <summary>
    /// 插槽条目列表，定义物品上的所有可用插槽
    /// </summary>
    public List<Destiny2ManifestItemSocketEntryDefinitionDto> SocketEntries { get; set; } = [];
    
    /// <summary>
    /// 固有插槽条目列表，描述物品自带的固有插槽
    /// </summary>
    public List<Destiny2ManifestItemIntrinsicSocketEntryDefinitionDto> IntrinsicSockets { get; set; } = [];
    
    /// <summary>
    /// 插槽类别列表，用于对插槽进行分组
    /// </summary>
    public List<Destiny2ManifestItemSocketCategoryDefinitionDto> SocketCategories { get; set; } = [];
}

/// <summary>
/// 命运2物品插槽条目定义，描述单个插槽的属性和可用选项
/// </summary>
public class Destiny2ManifestItemSocketEntryDefinitionDto
{
    /// <summary>
    /// 插槽类型哈希，定义插槽的基本类型
    /// </summary>
    public uint SocketTypeHash { get; set; }
    
    /// <summary>
    /// 初始插入物品哈希，表示插槽初始状态下插入的物品
    /// </summary>
    public uint SingleInitialItemHash { get; set; }
    
    /// <summary>
    /// 可重用插件物品列表，可以被插入此插槽的插件
    /// </summary>
    public List<Destiny2ManifestItemSocketEntryPlugItemDefinitionDto> ReusablePlugItems { get; set; } = [];
    
    /// <summary>
    /// 是否阻止在商人购买时初始化插槽
    /// </summary>
    public bool PreventInitializationOnVendorPurchase { get; set; }
    
    /// <summary>
    /// 是否在物品提示中隐藏特性
    /// </summary>
    public bool HidePerksInItemTooltip { get; set; }

    /// <summary>
    /// 插件来源标识
    /// </summary>
    public int PlugSources { get; set; }
    
    /// <summary>
    /// 可重用插件集哈希，关联到可用于此插槽的插件集合
    /// </summary>
    public uint ReusablePlugSetHash { get; set; }
    
    /// <summary>
    /// 随机化插件集哈希，关联到可随机生成的插件集合
    /// </summary>
    public uint RandomizedPlugSetHash { get; set; }
    
    /// <summary>
    /// 默认是否可见
    /// </summary>
    public bool DefaultVisible { get; set; }
}

/// <summary>
/// 命运2物品插槽条目插件物品定义，描述可插入插槽的单个插件
/// </summary>
public class Destiny2ManifestItemSocketEntryPlugItemDefinitionDto
{
   /// <summary>
   /// 插件物品哈希，关联到具体的插件物品
   /// </summary>
   public uint PlugItemHash { get; set; }
}

/// <summary>
/// 命运2物品固有插槽条目定义，描述物品自带的不可更改的插槽
/// </summary>
public class Destiny2ManifestItemIntrinsicSocketEntryDefinitionDto
{
    /// <summary>
    /// 插件物品哈希，关联到默认插入的固有插件
    /// </summary>
    public uint PlugItemHash { get; set; }
    
    /// <summary>
    /// 插槽类型哈希，定义固有插槽的类型
    /// </summary>
    public uint SocketTypeHash { get; set; }
    
    /// <summary>
    /// 默认是否可见
    /// </summary>
    public bool DefaultVisible { get; set; }
}

/// <summary>
/// 命运2物品插槽类别定义，用于对插槽进行分组和分类
/// </summary>
public class Destiny2ManifestItemSocketCategoryDefinitionDto
{
    /// <summary>
    /// 插槽类别哈希，关联到插槽类别定义
    /// </summary>
    public uint SocketCategoryHash { get; set; }
    
    /// <summary>
    /// 插槽索引，指示此类别包含的插槽索引
    /// </summary>
    public int SocketIndexes { get; set; }
}

/// <summary>
/// 命运2物品摘要块定义，包含物品在摘要视图中的显示信息
/// </summary>
public class Destiny2ManifestItemSummaryBlockDefinitionDto
{
    /// <summary>
    /// 排序优先级，决定在摘要中的显示顺序
    /// </summary>
    public int SortPriority { get; set; }
}

/// <summary>
/// 命运2物品天赋网格块定义，描述物品的天赋树结构
/// </summary>
public class Destiny2ManifestItemTalentGridBlockDefinitionDto
{
    /// <summary>
    /// 天赋网格哈希，关联到天赋网格定义
    /// </summary>
    public uint TalentGridHash { get; set; }
    
    /// <summary>
    /// 物品详细说明字符串
    /// </summary>
    public string ItemDetailString { get; set; } = string.Empty;
    
    /// <summary>
    /// 构建名称
    /// </summary>
    public string BuildName { get; set; } = string.Empty;
    
    /// <summary>
    /// HUD显示的伤害类型
    /// </summary>
    public Destiny2DamageTypeEnum HudDamageType { get; set; }
    
    /// <summary>
    /// HUD图标路径
    /// </summary>
    public string HudIcon { get; set; } = string.Empty;
}

/// <summary>
/// 命运2物品投资统计定义，描述物品的基础统计值投资
/// </summary>
public class Destiny2ManifestItemInvestmentStatDefinitionDto
{
    /// <summary>
    /// 统计类型哈希，关联到特定的统计属性类型
    /// </summary>
    public uint StatTypeHash { get; set; }
    
    /// <summary>
    /// 统计值
    /// </summary>
    public int Value { get; set; }
    
    /// <summary>
    /// 是否有条件激活
    /// </summary>
    public bool IsConditionallyActive { get; set; }
}

/// <summary>
/// 命运2物品特性条目定义，描述物品的特性和能力
/// </summary>
public class Destiny2ManifestItemPerkEntryDefinitionDto
{
    /// <summary>
    /// 需求显示字符串，描述激活特性的需求
    /// </summary>
    public string RequirementDisplayString { get; set; } = string.Empty;
    
    /// <summary>
    /// 特性哈希，关联到特性定义
    /// </summary>
    public uint PerkHash { get; set; }
    
    /// <summary>
    /// 特性可见性级别
    /// </summary>
    public int PerkVisibility { get; set; }
}

/// <summary>
/// 物品清单
/// </summary>
public class Destiny2ManifestInventoryItemDto : Dictionary<string, Destiny2ManifestInventoryItemDefinitionDto>
{
}