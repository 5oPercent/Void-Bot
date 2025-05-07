namespace Void.Bot.Message.Constants.Bungie.Destiny2;

/// <summary>
/// 命运2清单组件常量，用于访问和引用各种游戏数据
/// </summary>
public partial class Destiny2ManifestConstants
{
    /// <summary>数据组件缓存主键前缀</summary>
    public const string ComponentPrefixCacheKey = $"{Destiny2Constants.Destiny2}:{Manifest}";
    
    /// <summary>游戏数据版本</summary>
    public const string Version = nameof(Version);
    /// <summary>游戏数据版本 - 缓存主键</summary>
    public const string VersionCacheKey = $"{ComponentPrefixCacheKey}:{Version}";
    
    /// <summary>艺术染色通道</summary>
    public const string ArtDyeChannel = nameof(ArtDyeChannel);
    /// <summary>艺术染色通道 - 缓存主键</summary>
    public const string ArtDyeChannelCacheKey = $"{ComponentPrefixCacheKey}:{ArtDyeChannel}";

    /// <summary>艺术染色引用</summary>
    public const string ArtDyeReference = nameof(ArtDyeReference);
    /// <summary>艺术染色引用 - 缓存主键</summary>
    public const string ArtDyeReferenceCacheKey = $"{ComponentPrefixCacheKey}:{ArtDyeReference}";

    /// <summary>地点</summary>
    public const string Place = nameof(Place);
    /// <summary>地点 - 缓存主键</summary>
    public const string PlaceCacheKey = $"{ComponentPrefixCacheKey}:{Place}";

    /// <summary>活动</summary>
    public const string Activity = nameof(Activity);
    /// <summary>活动 - 缓存主键</summary>
    public const string ActivityCacheKey = $"{ComponentPrefixCacheKey}:{Activity}";

    /// <summary>活动类型</summary>
    public const string ActivityType = nameof(ActivityType);
    /// <summary>活动类型 - 缓存主键</summary>
    public const string ActivityTypeCacheKey = $"{ComponentPrefixCacheKey}:{ActivityType}";

    /// <summary>职业</summary>
    public const string Class = nameof(Class);
    /// <summary>职业 - 缓存主键</summary>
    public const string ClassCacheKey = $"{ComponentPrefixCacheKey}:{Class}";

    /// <summary>性别</summary>
    public const string Gender = nameof(Gender);
    /// <summary>性别 - 缓存主键</summary>
    public const string GenderCacheKey = $"{ComponentPrefixCacheKey}:{Gender}";

    /// <summary>物品栏位</summary>
    public const string InventoryBucket = nameof(InventoryBucket);
    /// <summary>物品栏位 - 缓存主键</summary>
    public const string InventoryBucketCacheKey = $"{ComponentPrefixCacheKey}:{InventoryBucket}";

    /// <summary>种族</summary>
    public const string Race = nameof(Race);
    /// <summary>种族 - 缓存主键</summary>
    public const string RaceCacheKey = $"{ComponentPrefixCacheKey}:{Race}";

    /// <summary>解锁</summary>
    public const string Unlock = nameof(Unlock);
    /// <summary>解锁 - 缓存主键</summary>
    public const string UnlockCacheKey = $"{ComponentPrefixCacheKey}:{Unlock}";

    /// <summary>属性组</summary>
    public const string StatGroup = nameof(StatGroup);
    /// <summary>属性组 - 缓存主键</summary>
    public const string StatGroupCacheKey = $"{ComponentPrefixCacheKey}:{StatGroup}";

    /// <summary>进度映射</summary>
    public const string ProgressionMapping = nameof(ProgressionMapping);
    /// <summary>进度映射 - 缓存主键</summary>
    public const string ProgressionMappingCacheKey = $"{ComponentPrefixCacheKey}:{ProgressionMapping}";

    /// <summary>阵营</summary>
    public const string Faction = nameof(Faction);
    /// <summary>阵营 - 缓存主键</summary>
    public const string FactionCacheKey = $"{ComponentPrefixCacheKey}:{Faction}";

    /// <summary>商人组</summary>
    public const string VendorGroup = nameof(VendorGroup);
    /// <summary>商人组 - 缓存主键</summary>
    public const string VendorGroupCacheKey = $"{ComponentPrefixCacheKey}:{VendorGroup}";

    /// <summary>奖励来源</summary>
    public const string RewardSource = nameof(RewardSource);
    /// <summary>奖励来源 - 缓存主键</summary>
    public const string RewardSourceCacheKey = $"{ComponentPrefixCacheKey}:{RewardSource}";

    /// <summary>解锁值</summary>
    public const string UnlockValue = nameof(UnlockValue);
    /// <summary>解锁值 - 缓存主键</summary>
    public const string UnlockValueCacheKey = $"{ComponentPrefixCacheKey}:{UnlockValue}";

    /// <summary>奖励映射</summary>
    public const string RewardMapping = nameof(RewardMapping);
    /// <summary>奖励映射 - 缓存主键</summary>
    public const string RewardMappingCacheKey = $"{ComponentPrefixCacheKey}:{RewardMapping}";

    /// <summary>奖励表单</summary>
    public const string RewardSheet = nameof(RewardSheet);
    /// <summary>奖励表单 - 缓存主键</summary>
    public const string RewardSheetCacheKey = $"{ComponentPrefixCacheKey}:{RewardSheet}";

    /// <summary>物品类别</summary>
    public const string ItemCategory = nameof(ItemCategory);
    /// <summary>物品类别 - 缓存主键</summary>
    public const string ItemCategoryCacheKey = $"{ComponentPrefixCacheKey}:{ItemCategory}";

    /// <summary>伤害类型</summary>
    public const string DamageType = nameof(DamageType);
    /// <summary>伤害类型 - 缓存主键</summary>
    public const string DamageTypeCacheKey = $"{ComponentPrefixCacheKey}:{DamageType}";

    /// <summary>活动模式</summary>
    public const string ActivityMode = nameof(ActivityMode);
    /// <summary>活动模式 - 缓存主键</summary>
    public const string ActivityModeCacheKey = $"{ComponentPrefixCacheKey}:{ActivityMode}";

    /// <summary>奖章等级</summary>
    public const string MedalTier = nameof(MedalTier);
    /// <summary>奖章等级 - 缓存主键</summary>
    public const string MedalTierCacheKey = $"{ComponentPrefixCacheKey}:{MedalTier}";

    /// <summary>成就</summary>
    public const string Achievement = nameof(Achievement);
    /// <summary>成就 - 缓存主键</summary>
    public const string AchievementCacheKey = $"{ComponentPrefixCacheKey}:{Achievement}";
    
    /// <summary>活动图谱</summary>
    public const string ActivityGraph = nameof(ActivityGraph);
    /// <summary>活动图谱 - 缓存主键</summary>
    public const string ActivityGraphCacheKey = $"{ComponentPrefixCacheKey}:{ActivityGraph}";

    /// <summary>活动交互对象</summary>
    public const string ActivityInteractable = nameof(ActivityInteractable);
    /// <summary>活动交互对象 - 缓存主键</summary>
    public const string ActivityInteractableCacheKey = $"{ComponentPrefixCacheKey}:{ActivityInteractable}";

    /// <summary>羁绊</summary>
    public const string Bond = nameof(Bond);
    /// <summary>羁绊 - 缓存主键</summary>
    public const string BondCacheKey = $"{ComponentPrefixCacheKey}:{Bond}";

    /// <summary>角色自定义类别</summary>
    public const string CharacterCustomizationCategory = nameof(CharacterCustomizationCategory);
    /// <summary>角色自定义类别 - 缓存主键</summary>
    public const string CharacterCustomizationCategoryCacheKey = $"{ComponentPrefixCacheKey}:{CharacterCustomizationCategory}";

    /// <summary>角色自定义选项</summary>
    public const string CharacterCustomizationOption = nameof(CharacterCustomizationOption);
    /// <summary>角色自定义选项 - 缓存主键</summary>
    public const string CharacterCustomizationOptionCacheKey = $"{ComponentPrefixCacheKey}:{CharacterCustomizationOption}";

    /// <summary>收藏品</summary>
    public const string Collectible = nameof(Collectible);
    /// <summary>收藏品 - 缓存主键</summary>
    public const string CollectibleCacheKey = $"{ComponentPrefixCacheKey}:{Collectible}";

    /// <summary>目的地</summary>
    public const string Destination = nameof(Destination);
    /// <summary>目的地 - 缓存主键</summary>
    public const string DestinationCacheKey = $"{ComponentPrefixCacheKey}:{Destination}";

    /// <summary>授权优惠</summary>
    public const string EntitlementOffer = nameof(EntitlementOffer);
    /// <summary>授权优惠 - 缓存主键</summary>
    public const string EntitlementOfferCacheKey = $"{ComponentPrefixCacheKey}:{EntitlementOffer}";

    /// <summary>装备槽位</summary>
    public const string EquipmentSlot = nameof(EquipmentSlot);
    /// <summary>装备槽位 - 缓存主键</summary>
    public const string EquipmentSlotCacheKey = $"{ComponentPrefixCacheKey}:{EquipmentSlot}";

    /// <summary>事件卡</summary>
    public const string EventCard = nameof(EventCard);
    /// <summary>事件卡 - 缓存主键</summary>
    public const string EventCardCacheKey = $"{ComponentPrefixCacheKey}:{EventCard}";

    /// <summary>火力战队查找活动图谱</summary>
    public const string FireteamFinderActivityGraph = nameof(FireteamFinderActivityGraph);
    /// <summary>火力战队查找活动图谱 - 缓存主键</summary>
    public const string FireteamFinderActivityGraphCacheKey = $"{ComponentPrefixCacheKey}:{FireteamFinderActivityGraph}";

    /// <summary>火力战队查找活动集合</summary>
    public const string FireteamFinderActivitySet = nameof(FireteamFinderActivitySet);
    /// <summary>火力战队查找活动集合 - 缓存主键</summary>
    public const string FireteamFinderActivitySetCacheKey = $"{ComponentPrefixCacheKey}:{FireteamFinderActivitySet}";

    /// <summary>火力战队查找标签</summary>
    public const string FireteamFinderLabel = nameof(FireteamFinderLabel);
    /// <summary>火力战队查找标签 - 缓存主键</summary>
    public const string FireteamFinderLabelCacheKey = $"{ComponentPrefixCacheKey}:{FireteamFinderLabel}";

    /// <summary>火力战队查找标签组</summary>
    public const string FireteamFinderLabelGroup = nameof(FireteamFinderLabelGroup);
    /// <summary>火力战队查找标签组 - 缓存主键</summary>
    public const string FireteamFinderLabelGroupCacheKey = $"{ComponentPrefixCacheKey}:{FireteamFinderLabelGroup}";

    /// <summary>火力战队查找选项</summary>
    public const string FireteamFinderOption = nameof(FireteamFinderOption);
    /// <summary>火力战队查找选项 - 缓存主键</summary>
    public const string FireteamFinderOptionCacheKey = $"{ComponentPrefixCacheKey}:{FireteamFinderOption}";

    /// <summary>火力战队查找选项组</summary>
    public const string FireteamFinderOptionGroup = nameof(FireteamFinderOptionGroup);
    /// <summary>火力战队查找选项组 - 缓存主键</summary>
    public const string FireteamFinderOptionGroupCacheKey = $"{ComponentPrefixCacheKey}:{FireteamFinderOptionGroup}";

    /// <summary>属性统计</summary>
    public const string Stat = nameof(Stat);
    /// <summary>属性统计 - 缓存主键</summary>
    public const string StatCacheKey = $"{ComponentPrefixCacheKey}:{Stat}";

    /// <summary>物品清单</summary>
    public const string InventoryItem = nameof(InventoryItem);
    /// <summary>物品清单 - 缓存主键</summary>
    public const string InventoryItemCacheKey = $"{ComponentPrefixCacheKey}:{InventoryItem}";

    /// <summary>物品清单简化</summary>
    public const string InventoryItemLite = nameof(InventoryItemLite);
    /// <summary>物品清单简化 - 缓存主键</summary>
    public const string InventoryItemLiteCacheKey = $"{ComponentPrefixCacheKey}:{InventoryItemLite}";

    /// <summary>物品等级类型</summary>
    public const string ItemTierType = nameof(ItemTierType);
    /// <summary>物品等级类型 - 缓存主键</summary>
    public const string ItemTierTypeCacheKey = $"{ComponentPrefixCacheKey}:{ItemTierType}";

    /// <summary>装备配置颜色</summary>
    public const string LoadoutColor = nameof(LoadoutColor);
    /// <summary>装备配置颜色 - 缓存主键</summary>
    public const string LoadoutColorCacheKey = $"{ComponentPrefixCacheKey}:{LoadoutColor}";

    /// <summary>装备配置图标</summary>
    public const string LoadoutIcon = nameof(LoadoutIcon);
    /// <summary>装备配置图标 - 缓存主键</summary>
    public const string LoadoutIconCacheKey = $"{ComponentPrefixCacheKey}:{LoadoutIcon}";

    /// <summary>装备配置名称</summary>
    public const string LoadoutName = nameof(LoadoutName);
    /// <summary>装备配置名称 - 缓存主键</summary>
    public const string LoadoutNameCacheKey = $"{ComponentPrefixCacheKey}:{LoadoutName}";

    /// <summary>位置</summary>
    public const string Location = nameof(Location);
    /// <summary>位置 - 缓存主键</summary>
    public const string LocationCacheKey = $"{ComponentPrefixCacheKey}:{Location}";

    /// <summary>传说故事</summary>
    public const string Lore = nameof(Lore);
    /// <summary>传说故事 - 缓存主键</summary>
    public const string LoreCacheKey = $"{ComponentPrefixCacheKey}:{Lore}";

    /// <summary>材料需求集合</summary>
    public const string MaterialRequirementSet = nameof(MaterialRequirementSet);
    /// <summary>材料需求集合 - 缓存主键</summary>
    public const string MaterialRequirementSetCacheKey = $"{ComponentPrefixCacheKey}:{MaterialRequirementSet}";

    /// <summary>指标</summary>
    public const string Metric = nameof(Metric);
    /// <summary>指标 - 缓存主键</summary>
    public const string MetricCacheKey = $"{ComponentPrefixCacheKey}:{Metric}";

    /// <summary>目标</summary>
    public const string Objective = nameof(Objective);
    /// <summary>目标 - 缓存主键</summary>
    public const string ObjectiveCacheKey = $"{ComponentPrefixCacheKey}:{Objective}";

    /// <summary>沙盒特性</summary>
    public const string SandboxPerk = nameof(SandboxPerk);
    /// <summary>沙盒特性 - 缓存主键</summary>
    public const string SandboxPerkCacheKey = $"{ComponentPrefixCacheKey}:{SandboxPerk}";

    /// <summary>平台栏位映射</summary>
    public const string PlatformBucketMapping = nameof(PlatformBucketMapping);
    /// <summary>平台栏位映射 - 缓存主键</summary>
    public const string PlatformBucketMappingCacheKey = $"{ComponentPrefixCacheKey}:{PlatformBucketMapping}";

    /// <summary>插槽集合</summary>
    public const string PlugSet = nameof(PlugSet);
    /// <summary>��槽集合 - 缓存主键</summary>
    public const string PlugSetCacheKey = $"{ComponentPrefixCacheKey}:{PlugSet}";

    /// <summary>能量上限</summary>
    public const string PowerCap = nameof(PowerCap);
    /// <summary>能量上限 - 缓存主键</summary>
    public const string PowerCapCacheKey = $"{ComponentPrefixCacheKey}:{PowerCap}";

    /// <summary>展示节点</summary>
    public const string PresentationNode = nameof(PresentationNode);
    /// <summary>展示节点 - 缓存主键</summary>
    public const string PresentationNodeCacheKey = $"{ComponentPrefixCacheKey}:{PresentationNode}";

    /// <summary>进度</summary>
    public const string Progression = nameof(Progression);
    /// <summary>进度 - 缓存主键</summary>
    public const string ProgressionCacheKey = $"{ComponentPrefixCacheKey}:{Progression}";

    /// <summary>进度等级需求</summary>
    public const string ProgressionLevelRequirement = nameof(ProgressionLevelRequirement);
    /// <summary>进度等级需求 - 缓存主键</summary>
    public const string ProgressionLevelRequirementCacheKey = $"{ComponentPrefixCacheKey}:{ProgressionLevelRequirement}";

    /// <summary>记录</summary>
    public const string Record = nameof(Record);
    /// <summary>记录 - 缓存主键</summary>
    public const string RecordCacheKey = $"{ComponentPrefixCacheKey}:{Record}";

    /// <summary>奖励调整指针</summary>
    public const string RewardAdjusterPointer = nameof(RewardAdjusterPointer);
    /// <summary>奖励调整指针 - 缓存主键</summary>
    public const string RewardAdjusterPointerCacheKey = $"{ComponentPrefixCacheKey}:{RewardAdjusterPointer}";

    /// <summary>奖励调整进度映射</summary>
    public const string RewardAdjusterProgressionMap = nameof(RewardAdjusterProgressionMap);
    /// <summary>奖励调整进度映射 - 缓存主键</summary>
    public const string RewardAdjusterProgressionMapCacheKey = $"{ComponentPrefixCacheKey}:{RewardAdjusterProgressionMap}";

    /// <summary>奖励物品列表</summary>
    public const string RewardItemList = nameof(RewardItemList);
    /// <summary>奖励物品列表 - 缓存主键</summary>
    public const string RewardItemListCacheKey = $"{ComponentPrefixCacheKey}:{RewardItemList}";

    /// <summary>袋装奖励物品列表</summary>
    public const string SackRewardItemList = nameof(SackRewardItemList);
    /// <summary>袋装奖励物品列表 - 缓存主键</summary>
    public const string SackRewardItemListCacheKey = $"{ComponentPrefixCacheKey}:{SackRewardItemList}";

    /// <summary>沙盒模式</summary>
    public const string SandboxPattern = nameof(SandboxPattern);
    /// <summary>沙盒模式 - 缓存主键</summary>
    public const string SandboxPatternCacheKey = $"{ComponentPrefixCacheKey}:{SandboxPattern}";

    /// <summary>赛季</summary>
    public const string Season = nameof(Season);
    /// <summary>赛季 - 缓存主键</summary>
    public const string SeasonCacheKey = $"{ComponentPrefixCacheKey}:{Season}";

    /// <summary>赛季通行证</summary>
    public const string SeasonPass = nameof(SeasonPass);
    /// <summary>赛季通行证 - 缓存主键</summary>
    public const string SeasonPassCacheKey = $"{ComponentPrefixCacheKey}:{SeasonPass}";

    /// <summary>社交表彰</summary>
    public const string SocialCommendation = nameof(SocialCommendation);
    /// <summary>社交表彰 - 缓存主键</summary>
    public const string SocialCommendationCacheKey = $"{ComponentPrefixCacheKey}:{SocialCommendation}";

    /// <summary>插槽类别</summary>
    public const string SocketCategory = nameof(SocketCategory);
    /// <summary>插槽类别 - 缓存主键</summary>
    public const string SocketCategoryCacheKey = $"{ComponentPrefixCacheKey}:{SocketCategory}";

    /// <summary>插槽类型</summary>
    public const string SocketType = nameof(SocketType);
    /// <summary>插槽类型 - 缓存主键</summary>
    public const string SocketTypeCacheKey = $"{ComponentPrefixCacheKey}:{SocketType}";

    /// <summary>特性</summary>
    public const string Trait = nameof(Trait);
    /// <summary>特性 - 缓存主键</summary>
    public const string TraitCacheKey = $"{ComponentPrefixCacheKey}:{Trait}";

    /// <summary>解锁计数映射</summary>
    public const string UnlockCountMapping = nameof(UnlockCountMapping);
    /// <summary>解锁计数映射 - 缓存主键</summary>
    public const string UnlockCountMappingCacheKey = $"{ComponentPrefixCacheKey}:{UnlockCountMapping}";

    /// <summary>解锁事件</summary>
    public const string UnlockEvent = nameof(UnlockEvent);
    /// <summary>解锁事件 - 缓存主键</summary>
    public const string UnlockEventCacheKey = $"{ComponentPrefixCacheKey}:{UnlockEvent}";

    /// <summary>解锁表达式映射</summary>
    public const string UnlockExpressionMapping = nameof(UnlockExpressionMapping);
    /// <summary>解锁表达式映射 - 缓存主键</summary>
    public const string UnlockExpressionMappingCacheKey = $"{ComponentPrefixCacheKey}:{UnlockExpressionMapping}";

    /// <summary>商人</summary>
    public const string Vendor = nameof(Vendor);
    /// <summary>商人 - 缓存主键</summary>
    public const string VendorCacheKey = $"{ComponentPrefixCacheKey}:{Vendor}";

    /// <summary>里程碑</summary>
    public const string Milestone = nameof(Milestone);
    /// <summary>里程碑 - 缓存主键</summary>
    public const string MilestoneCacheKey = $"{ComponentPrefixCacheKey}:{Milestone}";

    /// <summary>活动调整器</summary>
    public const string ActivityModifier = nameof(ActivityModifier);
    /// <summary>活动调整器 - 缓存主键</summary>
    public const string ActivityModifierCacheKey = $"{ComponentPrefixCacheKey}:{ActivityModifier}";

    /// <summary>报告原因类别</summary>
    public const string ReportReasonCategory = nameof(ReportReasonCategory);
    /// <summary>报告原因类别 - 缓存主键</summary>
    public const string ReportReasonCategoryCacheKey = $"{ComponentPrefixCacheKey}:{ReportReasonCategory}";

    /// <summary>神器</summary>
    public const string Artifact = nameof(Artifact);
    /// <summary>神器 - 缓存主键</summary>
    public const string ArtifactCacheKey = $"{ComponentPrefixCacheKey}:{Artifact}";

    /// <summary>破坏者类型</summary>
    public const string BreakerType = nameof(BreakerType);
    /// <summary>破坏者类型 - ��存主键</summary>
    public const string BreakerTypeCacheKey = $"{ComponentPrefixCacheKey}:{BreakerType}";

    /// <summary>检查列表</summary>
    public const string Checklist = nameof(Checklist);
    /// <summary>检查列表 - 缓存主键</summary>
    public const string ChecklistCacheKey = $"{ComponentPrefixCacheKey}:{Checklist}";

    /// <summary>能量类型</summary>
    public const string EnergyType = nameof(EnergyType);
    /// <summary>能量类型 - 缓存主键</summary>
    public const string EnergyTypeCacheKey = $"{ComponentPrefixCacheKey}:{EnergyType}";

    /// <summary>社交表彰节点</summary>
    public const string SocialCommendationNode = nameof(SocialCommendationNode);
    /// <summary>社交表彰节点 - 缓存主键</summary>
    public const string SocialCommendationNodeCacheKey = $"{ComponentPrefixCacheKey}:{SocialCommendationNode}";

    /// <summary>守护者等级</summary>
    public const string GuardianRank = nameof(GuardianRank);
    /// <summary>守护者等级 - 缓存主键</summary>
    public const string GuardianRankCacheKey = $"{ComponentPrefixCacheKey}:{GuardianRank}";

    /// <summary>守护者等级常量</summary>
    public const string GuardianRankConstants = nameof(GuardianRankConstants);
    /// <summary>守护者等级常量 - 缓存主键</summary>
    public const string GuardianRankConstantsCacheKey = $"{ComponentPrefixCacheKey}:{GuardianRankConstants}";
    
    /// <summary>装备配置常量</summary>
    public const string LoadoutConstants = nameof(LoadoutConstants);
    /// <summary>装备配置常量 - 缓存主键</summary>
    public const string LoadoutConstantsCacheKey = $"{ComponentPrefixCacheKey}:{LoadoutConstants}";

    /// <summary>火力战队查找常量</summary>
    public const string FireteamFinderConstants = nameof(FireteamFinderConstants);
    /// <summary>火力战队查找常量 - 缓存主键</summary>
    public const string FireteamFinderConstantsCacheKey = $"{ComponentPrefixCacheKey}:{FireteamFinderConstants}";

    /// <summary>全局常量</summary>
    public const string GlobalConstants = nameof(GlobalConstants);
    /// <summary>全局常量 - 缓存主键</summary>
    public const string GlobalConstantsCacheKey = $"{ComponentPrefixCacheKey}:{GlobalConstants}";
}