using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 进度定义，描述游戏中的各种进度系统（如声望、等级、季票等）
/// </summary>
public class Destiny2ManifestProgressionDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含进度系统的名称、描述和图标
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 作用域，定义进度系统的应用范围（账号级或角色级）
    /// </summary>
    public Destiny2ScopeEnum Scope { get; set; }

    /// <summary>
    /// 是否重复最后一步，达到最高级后是否循环最后一级的经验值
    /// </summary>
    public bool RepeatLastStep { get; set; }

    /// <summary>
    /// 来源，获取该进度的途径描述
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// 进度步骤，定义每个等级的经验需求和奖励
    /// </summary>
    public List<Destiny2ManifestProgressionStepDefinitionDto> Steps { get; set; } = [];

    /// <summary>
    /// 是否可见，决定进度是否显示给玩家
    /// </summary>
    public bool Visible { get; set; }

    /// <summary>
    /// 阵营哈希，关联到此进度系统相关的阵营定义
    /// </summary>
    public uint FactionHash { get; set; }

    /// <summary>
    /// 颜色，进度系统的主题颜色定义
    /// </summary>
    public Destiny2ManifestColorDefinitionDto Color {get; set; } = new();

    /// <summary>
    /// 等级图标，显示当前等级的图标资源路径
    /// </summary>
    public string RankIcon { get; set; } = string.Empty;

    /// <summary>
    /// 奖励物品，在达到特定进度级别时获得的物品列表
    /// </summary>
    public List<Destiny2ManifestProgressionRewardItemQuantityDto> RewardItems { get; set; } = [];
}

/// <summary>
/// 进度步骤定义，描述进度系统中的单个等级或阶段
/// </summary>
public class Destiny2ManifestProgressionStepDefinitionDto
{
    /// <summary>
    /// 步骤名称，此进度级别的显示名称
    /// </summary>
    public string StepName { get; set; } = string.Empty;

    /// <summary>
    /// 显示效果类型，达到此级别时触发的视觉效果
    /// </summary>
    public Destiny2ProgressionStepDisplayEffectEnum DisplayEffectType { get; set; }

    /// <summary>
    /// 进度总量，完成此步骤所需的经验值总数
    /// </summary>
    public int ProgressTotal { get; set; }

    /// <summary>
    /// 奖励物品，达到此级别时获得的物品列表
    /// </summary>
    public List<Destiny2ManifestItemQuantityDto> RewardItems { get; set; } = [];

    /// <summary>
    /// 图标，此进度级别的专属图标资源路径
    /// </summary>
    public string Icon { get; set; } = string.Empty;
}

/// <summary>
/// 进度奖励物品数量，描述进度系统奖励的物品及其获取方式
/// </summary>
public class Destiny2ManifestProgressionRewardItemQuantityDto
{
    /// <summary>
    /// 奖励物品索引，在奖励列表中的位置
    /// </summary>
    public int RewardItemIndex { get; set; }

    /// <summary>
    /// 奖励获得的进度级别，达到此级别时解锁奖励
    /// </summary>
    public int RewardedAtProgressionLevel { get; set; }

    /// <summary>
    /// 获取行为，决定奖励物品是自动获得还是需要玩家手动领取
    /// </summary>
    public Destiny2ProgressionRewardItemAcquisitionBehaviorEnum AcquisitionBehavior { get; set; }

    /// <summary>
    /// UI显示风格，奖励在界面中的显示方式
    /// </summary>
    public string UiDisplayStyle { get; set; } = string.Empty;

    /// <summary>
    /// 领取解锁显示文本列表，解锁奖励时显示的提示文本
    /// </summary>
    public List<string> ClaimUnlockDisplayStrings { get; set; } = [];

    /// <summary>
    /// 插槽覆盖列表，定义奖励对物品插槽的特殊修改
    /// </summary>
    public List<Destiny2ManifestProgressionSocketPlugOverrideDto> SocketOverrides { get; set; } = [];

    /// <summary>
    /// 物品哈希，关联到奖励物品的定义
    /// </summary>
    public uint ItemHash { get; set; }

    /// <summary>
    /// 物品实例ID，特定物品实例的唯一标识
    /// </summary>
    public long ItemInstanceId { get; set; }

    /// <summary>
    /// 数量，奖励物品的获取数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 是否有条件可见性，决定奖励是否基于某些条件才显示
    /// </summary>
    public bool HasConditionalVisibility { get; set; }
}

/// <summary>
/// 进度插槽插件覆盖，定义奖励对物品插槽的特殊修改
/// </summary>
public class Destiny2ManifestProgressionSocketPlugOverrideDto
{
    /// <summary>
    /// 插槽类型哈希，关联到要覆盖的插槽类型定义
    /// </summary>
    public uint SocketTypeHash { get; set; }
    
    /// <summary>
    /// 覆盖单物品哈希，用于替换插槽中默认插件的新插件哈希
    /// </summary>
    public uint OverrideSingleItemHash { get; set; }
}

/// <summary>
/// 进度系统清单，存储所有进度系统定义的字典集合
/// </summary>
public class Destiny2ManifestProgressionDto : Dictionary<string, Destiny2ManifestProgressionDefinitionDto>
{
}