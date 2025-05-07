using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 记录定义，描述游戏中的成就、挑战和追踪记录
/// </summary>
public class Destiny2ManifestRecordDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含记录的名称、描述和图标
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 作用域，定义记录的应用范围（账号级或角色级）
    /// </summary>
    public Destiny2ScopeEnum Scope { get; set; }

    /// <summary>
    /// 传说故事哈希，关联到此记录相关的传说故事定义
    /// </summary>
    public uint LoreHash { get; set; }

    /// <summary>
    /// 目标哈希列表，关联到实现此记录需要的目标任务
    /// </summary>
    public List<uint> ObjectiveHashes { get; set; } = [];

    /// <summary>
    /// 记录值样式，定义记录进度的显示格式
    /// </summary>
    public Destiny2RecordValueStyleEnum RecordValueStyle { get; set; }

    /// <summary>
    /// 是否用于称号镀金，标识此记录是否参与称号镀金进程
    /// </summary>
    public bool ForTitleGilding { get; set; }

    /// <summary>
    /// 是否显示大图标，决定记录在界面中的显示尺寸
    /// </summary>
    public bool ShouldShowLargeIcons { get; set; }

    /// <summary>
    /// 称号信息，包含记录关联的称号详情
    /// </summary>
    public Destiny2ManifestRecordTitleBlockDto TitleInfo { get; set; } = new();

    /// <summary>
    /// 完成信息，定义记录完成时的行为和奖励
    /// </summary>
    public Destiny2ManifestRecordCompletionBlockDto CompletionInfo { get; set; } = new();

    /// <summary>
    /// 状态信息，包含记录的显示状态和优先级
    /// </summary>
    public Destiny2ManifestSchemaRecordStateBlockDto StateInfo { get; set; } = new();

    /// <summary>
    /// 需求信息，定义解锁此记录所需的前置条件
    /// </summary>
    public Destiny2ManifestPresentationNodeRequirementsBlockDto Requirements { get; set; } = new();

    /// <summary>
    /// 过期信息，定义记录的有效期限和过期行为
    /// </summary>
    public Destiny2ManifestRecordExpirationBlockDto ExpirationInfo { get; set; } = new();

    /// <summary>
    /// 间隔信息，定义记录的阶段性目标和奖励
    /// </summary>
    public Destiny2ManifestRecordIntervalBlockDto IntervalInfo { get; set; } = new();

    /// <summary>
    /// 奖励物品，完成记录后获得的物品列表
    /// </summary>
    public List<Destiny2ManifestItemQuantityDto> RewardItems { get; set; } = [];

    /// <summary>
    /// 记录类型名称，标识记录的分类名称
    /// </summary>
    public string RecordTypeName { get; set; } = string.Empty;

    /// <summary>
    /// 展示节点类型，定义记录在展示树中的节点类型
    /// </summary>
    public Destiny2PresentationNodeTypeEnum PresentationNodeType { get; set; }

    /// <summary>
    /// 特性ID列表，记录关联的特性标识符
    /// </summary>
    public List<string> TraitIds { get; set; } = [];

    /// <summary>
    /// 特性哈希列表，关联到记录特性的定义
    /// </summary>
    public List<uint> TraitHashes { get; set; } = [];

    /// <summary>
    /// 父节点哈希列表，关联到此记录所属的展示节点
    /// </summary>
    public List<uint> ParentNodeHashes { get; set; } = [];
}

/// <summary>
/// 记录称号块，定义记录解锁的玩家称号
/// </summary>
public class Destiny2ManifestRecordTitleBlockDto
{
    /// <summary>
    /// 是否有称号，标识此记录是否解锁玩家称号
    /// </summary>
    public bool HasTitle { get; set; }

    /// <summary>
    /// 按性别划分的称号，根据角色性别显示不同称号文本
    /// </summary>
    public Dictionary<int, string> TitlesByGender { get; set; } = new();

    /// <summary>
    /// 按性别哈希划分的称号，使用性别定义哈希作为键
    /// </summary>
    public Dictionary<uint, string> TitlesByGenderHash { get; set; } = new();

    /// <summary>
    /// 镀金追踪记录哈希，关联到称号镀金进度的追踪记录
    /// </summary>
    public uint GildingTrackingRecordHash { get; set; }
}

/// <summary>
/// 记录完成块，定义记录完成时的行为和奖励
/// </summary>
public class Destiny2ManifestRecordCompletionBlockDto
{
    /// <summary>
    /// 部分实现目标计数阈值，达到此数量目标视为部分完成
    /// </summary>
    public int PartialCompletionObjectiveCountThreshold { get; set; }

    /// <summary>
    /// 分数值，完成记录获得的成就分数
    /// </summary>
    public int ScoreValue { get; set; }

    /// <summary>
    /// 是否触发提示通知，完成时是否显示屏幕通知
    /// </summary>
    public bool ShouldFireToast { get; set; }

    /// <summary>
    /// 提示样式，定义完成通知的显示样式
    /// </summary>
    public Destiny2RecordToastStyleEnum ToastStyle { get; set; }
}

/// <summary>
/// 记录状态块，定义记录在界面中的显示状态
/// </summary>
public class Destiny2ManifestSchemaRecordStateBlockDto
{
    /// <summary>
    /// 特色优先级，决定在特色列表中的排序位置
    /// </summary>
    public int FeaturedPriority { get; set; }

    /// <summary>
    /// 模糊化名称，在记录未解锁时显示的替代名称
    /// </summary>
    public string ObscuredName { get; set; } = string.Empty;

    /// <summary>
    /// 模糊化描述，在记录未解锁时显示的替代描述
    /// </summary>
    public string ObscuredDescription { get; set; } = string.Empty;
}

/// <summary>
/// 记录过期块，定义记录的有效期限和过期行为
/// </summary>
public class Destiny2ManifestRecordExpirationBlockDto
{
    /// <summary>
    /// 是否有过期时间，标识记录是否会过期
    /// </summary>
    public bool HasExpiration { get; set; }

    /// <summary>
    /// 描述，关于记录过期的说明文本
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 图标，显示记录过期状态的图标
    /// </summary>
    public string Icon { get; set; } = string.Empty;
}

/// <summary>
/// 记录间隔块，定义记录的阶段性目标和奖励
/// </summary>
public class Destiny2ManifestRecordIntervalBlockDto
{
    /// <summary>
    /// 间隔目标列表，记录中的阶段性目标定义
    /// </summary>
    public List<Destiny2ManifestRecordIntervalObjectiveDto> IntervalObjectives { get; set; } = [];

    /// <summary>
    /// 间隔奖励列表，实现阶段性目标获得的奖励
    /// </summary>
    public List<Destiny2ManifestRecordIntervalRewardsDto> IntervalRewards { get; set; } = [];

    /// <summary>
    /// 原始目标数组插入索引，在目标数组中的插入位置
    /// </summary>
    public int OriginalObjectiveArrayInsertionIndex { get; set; }
}

/// <summary>
/// 记录间隔目标，定义记录中的单个阶段性目标
/// </summary>
public class Destiny2ManifestRecordIntervalObjectiveDto
{
    /// <summary>
    /// 间隔目标哈希，关联到此阶段目标的定义
    /// </summary>
    public uint IntervalObjectiveHash { get; set; }

    /// <summary>
    /// 间隔分数值，实现此阶段目标获得的分数
    /// </summary>
    public int IntervalScoreValue { get; set; }
}

/// <summary>
/// 记录间隔奖励，定义实现阶段性目标获得的奖励
/// </summary>
public class Destiny2ManifestRecordIntervalRewardsDto
{
    /// <summary>
    /// 间隔奖励物品，实现阶段目标获得的物品列表
    /// </summary>
    public List<Destiny2ManifestItemQuantityDto> IntervalRewardItems { get; set; } = [];
}

/// <summary>
/// 记录清单，存储所有记录定义的字典集合
/// </summary>
public class Destiny2ManifestRecordDto : Dictionary<string, Destiny2ManifestRecordDefinitionDto>
{
}