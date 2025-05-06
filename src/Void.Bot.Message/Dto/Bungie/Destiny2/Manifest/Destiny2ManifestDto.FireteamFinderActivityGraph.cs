namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 活动图定义
/// </summary>
public class Destiny2ManifestFireteamFinderActivityGraphDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性。
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 子节点哈希值列表。
    /// </summary>
    public List<uint> Children { get; set; } = [];
    
    /// <summary>
    /// 当前节点及其所有后代的哈希值列表。
    /// </summary>
    public List<uint> SelfAndAllDescendantHashes { get; set; } = [];

    /// <summary>
    /// 相关活动集合哈希值列表。
    /// </summary>
    public List<uint> RelatedActivitySetHashes { get; set; } = [];

    /// <summary>
    /// 相关导演节点哈希值列表。
    /// </summary>
    public List<uint> RelatedDirectorNodes { get; set; } = [];

    /// <summary>
    /// 相关可交互活动列表。
    /// </summary>
    public List<Destiny2ManifestFireteamFinderRelatedInteractableActivityEntryDto> RelatedInteractableActivities { get; set; } = [];

    /// <summary>
    /// 相关位置哈希值列表。
    /// </summary>
    public List<uint> RelatedLocationHashes { get; set; } = [];

    /// <summary>
    /// 是否是玩家选择的难度节点。
    /// </summary>
    public bool IsPlayerElectedDifficultyNode { get; set; }

    /// <summary>
    /// 父节点哈希值。
    /// </summary>
    public uint ParentHash { get; set; }
}

/// <summary>
/// 可交互活动
/// </summary>
public class Destiny2ManifestFireteamFinderRelatedInteractableActivityEntryDto
{
    /// <summary>
    /// 可交互活动哈希值。
    /// </summary>
    public uint ActivityInteractableHash { get; set; }
    
    /// <summary>
    /// 可交互活动元素索引
    /// </summary>
    public int ActivityInteractableElementIndex { get; set; }
}

/// <summary>
/// 活动图定义集合。
/// </summary>
public class Destiny2ManifestFireteamFinderActivityGraphListDto : Dictionary<string, Destiny2ManifestFireteamFinderActivityGraphDefinitionDto>
{
}