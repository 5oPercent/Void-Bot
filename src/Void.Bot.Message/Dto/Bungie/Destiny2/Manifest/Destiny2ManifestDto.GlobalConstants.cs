namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 命运2全局常量定义定义
/// </summary>
public class Destiny2ManifestGlobalConstantsDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 寻路者常量，定义游戏中寻路系统的配置和拓扑结构
    /// </summary>
    public Destiny2ManifestPathfinderConstantsDefinitionDto PathfinderConstants { get; set; } = new();

    /// <summary>
    /// 收藏品根节点哈希，关联到收藏品系统的起始节点
    /// </summary>
    public uint CollectionsRootNodeHash { get; set; }

    /// <summary>
    /// 收藏品徽章根节点哈希，关联到徽章收藏系统的起始节点
    /// </summary>
    public uint CollectionBadgesRootNodeHash { get; set; }

    /// <summary>
    /// 活跃凯旋记录根节点哈希，关联到当前可完成的凯旋记录起始节点
    /// </summary>
    public uint ActiveTriumphsRootNodeHash { get; set; }

    /// <summary>
    /// 活跃印章根节点哈希，关联到当前可获取的印章起始节点
    /// </summary>
    public uint ActiveSealsRootNodeHash { get; set; }

    /// <summary>
    /// 遗留凯旋记录根节点哈希，关联到历史凯旋记录的起始节点
    /// </summary>
    public uint LegacyTriumphsRootNodeHash { get; set; }

    /// <summary>
    /// 遗留印章根节点哈希，关联到历史印章的起始节点
    /// </summary>
    public uint LegacySealsRootNodeHash { get; set; }

    /// <summary>
    /// 勋章根节点哈希，关联到勋章系统的起始节点
    /// </summary>
    public uint MedalsRootNodeHash { get; set; }

    /// <summary>
    /// 异域催化剂根节点哈希，关联到异域武器催化剂系统的起始节点
    /// </summary>
    public uint ExoticCatalystsRootNodeHash { get; set; }

    /// <summary>
    /// 传说根节点哈希，关联到游戏传说故事系统的起始节点
    /// </summary>
    public uint LoreRootNodeHash { get; set; }

    /// <summary>
    /// 指标根节点哈希，关联到玩家统计指标系统的起始节点
    /// </summary>
    public uint MetricsRootNodeHash { get; set; }

    /// <summary>
    /// 制作根节点哈希，关联到武器制作系统的起始节点
    /// </summary>
    public uint CraftingRootNodeHash { get; set; }

    /// <summary>
    /// 守护者等级根节点哈希，关联到守护者等级系统的起始节点
    /// </summary>
    public uint GuardianRanksRootNodeHash { get; set; }
}

/// <summary>
/// 命运2寻路者常量定义，描述游戏中寻路系统的配置和拓扑结构
/// </summary>
public class Destiny2ManifestPathfinderConstantsDefinitionDto
{
    /// <summary>
    /// 苍白之心寻路者根节点哈希，关联到苍白之心季节的寻路系统起始节点
    /// </summary>
    public uint ThePaleHeartPathfinderRootNodeHash { get; set; }

    /// <summary>
    /// 先锋队寻路者根节点哈希，关联到先锋队活动的寻路系统起始节点
    /// </summary>
    public uint VanguardPathfinderRootNodeHash { get; set; }

    /// <summary>
    /// 熔炉寻路者根节点哈希，关联到PVP熔炉活动的寻路系统起始节点
    /// </summary>
    public uint CruciblePathfinderRootNodeHash { get; set; }

    /// <summary>
    /// 智谋寻路者根节点哈希，关联到智谋活动的寻路系统起始节点
    /// </summary>
    public uint GambitPathfinderRootNodeHash { get; set; }

    /// <summary>
    /// 所有寻路者根节点哈希列表，包含所有寻路系统的根节点哈希值
    /// </summary>
    public List<uint> AllPathfinderRootNodeHashes { get; set; } = [];

    /// <summary>
    /// 寻路者树层级配置，键为节点数值，值为对应的层级
    /// </summary>
    public Dictionary<int, int> PathfinderTreeTiers { get; set; } = new();

    /// <summary>
    /// 寻路者拓扑结构，键为节点ID，值为与该节点相连的节点ID列表
    /// </summary>
    public Dictionary<string, List<int>> PathfinderTopology { get; set; } = new();
}

/// <summary>
/// 命运2全局常量清单清单
/// </summary>
public class Destiny2ManifestGlobalConstantsDto : Dictionary<string, Destiny2ManifestGlobalConstantsDefinitionDto>
{
}