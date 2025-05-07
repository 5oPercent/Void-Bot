namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 命运2解锁计数映射定义
/// </summary>
public class Destiny2ManifestUnlockCountMappingDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 解锁值哈希，关联到追踪解锁进度的统计值或计数器
    /// </summary>
    public uint UnlockValueHash { get; set; }
}

/// <summary>
/// 命运2解锁计数映射清单，存储所有解锁计数映射定义的字典集合
/// </summary>
public class Destiny2ManifestUnlockCountMappingDto : Dictionary<string, Destiny2ManifestUnlockCountMappingDefinitionDto>
{
}