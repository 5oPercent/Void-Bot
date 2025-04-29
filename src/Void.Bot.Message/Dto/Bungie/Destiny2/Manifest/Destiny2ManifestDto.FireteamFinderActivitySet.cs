namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 组队查找器活动集定义
/// </summary>
public class Destiny2ManifestFireteamFinderActivitySetDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 组队查找器活动集定义
    /// </summary>
    public int MaximumPartySize { get; set; }

    /// <summary>
    /// 选项哈希列表
    /// </summary>
    public List<uint> OptionHashes { get; set; } = [];
    
    /// <summary>
    /// 标签哈希列表
    /// </summary>
    public List<uint> LabelHashes { get; set; } = [];
    
    /// <summary>
    /// 活动图哈希列表
    /// </summary>
    public List<uint> ActivityGraphHashes { get; set; } = [];
    
    /// <summary>
    /// 活动哈希列表
    /// </summary>
    public List<uint> ActivityHashes { get; set; } = [];
}

/// <summary>
/// 组队查找器活动集清单，包含所有可用的活动集定义
/// </summary>
public class Destiny2ManifestFireteamFinderActivitySetDto : Dictionary<string, Destiny2ManifestFireteamFinderActivitySetDefinitionDto>
{
}