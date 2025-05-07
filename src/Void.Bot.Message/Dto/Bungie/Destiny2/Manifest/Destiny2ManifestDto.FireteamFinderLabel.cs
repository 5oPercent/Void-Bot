namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 组队查找器标签定义
/// </summary>
public class Destiny2ManifestFireteamFinderLabelDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 降序排序优先级
    /// </summary>
    public int DescendingSortPriority { get; set; }

    /// <summary>
    /// 组哈希
    /// </summary>
    public uint GroupHash { get; set; }
    
    /// <summary>
    /// 允许在字段中的标志
    /// </summary>
    public int AllowInFields { get; set; }
}

/// <summary>
/// 组队查找器标签清单，包含所有可用的标签定义
/// </summary>
public class Destiny2ManifestFireteamFinderLabelDto : Dictionary<string, Destiny2ManifestFireteamFinderLabelDefinitionDto>
{
}