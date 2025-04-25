namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 解锁值定义
/// </summary>
public class Destiny2ManifestUnlockValueDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 聚合类型
    /// </summary>
    public int AggregationType { get; set; }

    /// <summary>
    /// 范围
    /// </summary>
    public int Scope { get; set; }

    /// <summary>
    /// 映射索引
    /// </summary>
    public int MappingIndex { get; set; }
}

/// <summary>
/// 解锁值清单
/// </summary>
public class Destiny2ManifestUnlockValueDto : Dictionary<string, Destiny2ManifestUnlockValueDefinitionDto>
{
}