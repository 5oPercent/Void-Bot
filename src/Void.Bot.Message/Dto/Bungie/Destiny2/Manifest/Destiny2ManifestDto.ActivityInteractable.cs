namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 活动交互定义
/// </summary>
public class Destiny2ManifestActivityInteractableDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 交互列表
    /// </summary>
    public List<Destiny2ManifestActivityInteractableEntryDto> Entries { get; set; } = [];
}

/// <summary>
/// 可交互活动
/// </summary>
public class Destiny2ManifestActivityInteractableEntryDto
{
    /// <summary>
    /// 活动哈希
    /// </summary>
    public uint ActivityHash { get; set; }
}

/// <summary>
/// 活动互动清单
/// </summary>
public class Destiny2ManifestActivityInteractableDto : Dictionary<string, Destiny2ManifestActivityInteractableDefinitionDto>
{
}