namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 奖励映射定义
/// </summary>
public class Destiny2ManifestRewardMappingDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 映射哈希
    /// </summary>
    public uint MappingHash { get; set; }
    
    /// <summary>
    /// 映射索引
    /// </summary>
    public int MappingIndex { get; set; }

    /// <summary>
    /// 全局
    /// </summary>
    public bool IsGlobal { get; set; }
}

/// <summary>
/// 奖励映射清单
/// </summary>
public class Destiny2ManifestRewardMappingDto : Dictionary<string, Destiny2ManifestRewardMappingDefinitionDto>
{
}