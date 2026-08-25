using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 活动定义
/// </summary>
public class Destiny2ManifestActivityDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 原始显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto OriginalDisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 发布图标
    /// </summary>
    public string ReleaseIcon { get; set; } = string.Empty;
    
    /// <summary>
    /// 发布时间
    /// </summary>
    public long ReleaseTime { get; set; }
    
    /// <summary>
    /// 完成解锁哈希
    /// </summary>
    public uint CompletionUnlockHash { get; set; }

    /// <summary>
    /// 活动光等推荐
    /// </summary>
    public int ActivityLightLevel { get; set; }
    
    /// <summary>
    /// 目的地哈希
    /// </summary>
    public uint DestinationHash { get; set; }
    
    /// <summary>
    /// 地点哈希
    /// </summary>
    public uint PlaceHash { get; set; }
    
    /// <summary>
    /// 活动类型哈希
    /// </summary>
    public uint ActivityTypeHash { get; set; }
    
    /// <summary>
    /// 活动难度等级
    /// </summary>
    public int Tier { get; set; }
    
    /// <summary>
    /// 活动报告(Post-Game Carnage Report)图像路径
    /// </summary>
    public string PgcrImage { get; set; } = string.Empty;
    
    /// <summary>
    /// 奖励清单
    /// </summary>
    public List<Destiny2ManifestActivityRewardsDto>? Rewards { get; set; } = [];
    
    /// <summary>
    /// 修改器清单
    /// </summary>
    public List<Destiny2ManifestModifyDefinitionDto> Modifiers { get; set; } = [];
    
    /// <summary>
    /// 是否为可游玩列表活动
    /// </summary>
    public bool IsPlayList { get; set; }
    
    /// <summary>
    /// 挑战清单
    /// </summary>
    public List<Destiny2ManifestActivityChallengeDto> Challenges { get; set; } = [];
    
    /// <summary>
    /// 可选解锁字符串列表
    /// </summary>
    public List<Destiny2ManifestDisplayStringDefinitionDto> OptionalUnlockStrings { get; set; } = [];
    
    /// <summary>
    /// 是否继承自自由漫游
    /// </summary>
    public bool InheritFromFreeRoam { get; set; }
    
    /// <summary>
    /// 是否抑制其他奖励
    /// </summary>
    public bool SuppressOtherRewards { get; set; }

    /// <summary>
    /// 活动需求
    /// </summary>
    public Destiny2ManifestActivityRequirementsDto Requirements { get; set; } = new();
    
    /// <summary>
    /// 游玩列表项目
    /// </summary>
    public List<Destiny2ManifestActivityPlayItemDto> PlaylistItems { get; set; } = [];
    
    /// <summary>
    /// 活动匹配条件
    /// </summary>
    public Destiny2ManifestActivityMatchmakingBlockDto Matchmaking { get; set; } = new();

    /// <summary>
    /// 指引模式
    /// </summary>
    public Destiny2ManifestActivityGuidedBlockDto GuidedGame { get; set; } = new();
    
    /// <summary>
    /// 主要活动模式哈希
    /// </summary>
    public uint DirectActivityModeHash { get; set; }

    /// <summary>
    /// 主要活动模式类型
    /// </summary>
    public Destiny2ActivityModeTypeEnum DirectActivityModeType { get; set; }

    /// <summary>
    /// 活动模式哈希列表
    /// </summary>
    public List<uint> ActivityModeHashes { get; set; } = [];

    /// <summary>
    /// 活动模式类型列表
    /// </summary>
    public List<Destiny2ActivityModeTypeEnum> ActivityModeTypes { get; set; } = [];

    /// <summary>
    /// 是否为PVP活动
    /// </summary>
    public bool IsPvP { get; set; }

    /// <summary>
    /// 活动插入点列表
    /// </summary>
    public List<Destiny2ManifestActivityPointDto> InsertionPoints { get; set; } = [];
}

/// <summary>
/// 命运2清单组件奖励
/// </summary>
public class Destiny2ManifestActivityRewardsDto
{
    public string RewardText { get; set; } = string.Empty;

    public List<Destiny2ManifestRewardDefinitionDto> RewardItems { get; set; } = [];
}

/// <summary>
/// 命运2清单组件挑战
/// </summary>
public class Destiny2ManifestActivityChallengeDto
{
    /// <summary>
    /// 奖励物品哈希
    /// </summary>
    public uint RewardItemHash { get; set; }
    
    /// <summary>
    /// 禁止奖励解锁哈希
    /// </summary>
    public uint InhibitRewardsUnlockHash { get; set; }
    
    /// <summary>
    /// 目标哈希
    /// </summary>
    public uint ObjectiveHash { get; set; }
    
    /// <summary>
    /// 虚拟奖励列表
    /// </summary>
    public List<Destiny2ManifestRewardDefinitionDto> DummyRewards { get; set; } = [];
}

/// <summary>
/// 命运2清单组件活动需求
/// </summary>
public class Destiny2ManifestActivityRequirementsDto
{
    /// <summary>
    /// 队长需求标签列表
    /// </summary>
    public List<Destiny2ManifestDisplayStringDefinitionDto> LeaderRequirementLabels { get; set; } = [];
    
    /// <summary>
    /// 火力战队需求标签列表
    /// </summary>
    public List<Destiny2ManifestDisplayStringDefinitionDto> FireteamRequirementLabels { get; set; } = [];
}

/// <summary>
/// 命运2清单组件游玩信息
/// </summary>
public class Destiny2ManifestActivityPlayItemDto
{
    /// <summary>
    /// 活动哈希
    /// </summary>
    public uint ActivityHash { get; set; }

    /// <summary>
    /// 直接活动模式哈希
    /// </summary>
    public uint DirectActivityModeHash { get; set; }

    /// <summary>
    /// 直接活动模式类型
    /// </summary>
    public Destiny2ActivityModeTypeEnum DirectActivityModeType { get; set; }

    /// <summary>
    /// 活动模式哈希列表
    /// </summary>
    public List<uint> ActivityModeHashes { get; set; } = [];
        
    /// <summary>
    /// 活动模式类型列表
    /// </summary>
    public List<Destiny2ActivityModeTypeEnum> ActivityModeTypes { get; set; } = [];
        
    /// <summary>
    /// 权重
    /// </summary>
    public int Weight { get; set; }
}

/// <summary>
/// 命运2清单活动匹配设置
/// </summary>
public class Destiny2ManifestActivityMatchmakingBlockDto
{
    /// <summary>
    /// 是否启用匹配
    /// </summary>
    public bool IsMatchmade { get; set; }
    
    /// <summary>
    /// 最小队伍人数
    /// </summary>
    public int MinParty { get; set; }
    
    /// <summary>
    /// 最大队伍人数
    /// </summary>
    public int MaxParty { get; set; }
    
    /// <summary>
    /// 最大玩家数
    /// </summary>
    public int MaxPlayers { get; set; }

    /// <summary>
    /// 是否需要守护者誓言
    /// </summary>
    public bool RequiresGuardianOath { get; set; }
}

/// <summary>
/// 命运2清单活动指引模式信息
/// </summary>
public class Destiny2ManifestActivityGuidedBlockDto
{
    /// <summary>
    /// 等待大厅最多人数
    /// </summary>
    public int GuidedMaxLobbySize { get; set; }

    /// <summary>
    /// 等待大厅最小人数
    /// </summary>
    public int GuidedMinLobbySize { get; set; }

    /// <summary>
    /// 解散队伍所需票数, 值 -1 时代表无法解散
    /// </summary>
    public int GuidedDisbandCount { get; set; }
}

/// <summary>
/// 命运2清单活动插入点
/// </summary>
public class Destiny2ManifestActivityPointDto
{
    /// <summary>
    /// 阶段哈希
    /// </summary>
    public uint PhaseHash { get; set; }
    
    /// <summary>
    /// 解锁哈希
    /// </summary>
    public uint UnlockHash { get; set; }
}

/// <summary>
/// 命运2清单活动位置映射
/// </summary>
public class Destiny2ManifestActivityLocationMappingDto
{
    /// <summary>
    /// 位置哈希
    /// </summary>
    public uint LocationHash { get; set; }

    /// <summary>
    /// 激活源
    /// </summary>
    public string ActivationSource { get; set; } = string.Empty;

    /// <summary>
    /// 活动哈希
    /// </summary>
    public uint ActivityHash { get; set; }
}

/// <summary>
/// 活动清单
/// </summary>
public class Destiny2ManifestActivityDto : Dictionary<string, Destiny2ManifestActivityDefinitionDto>
{
}
