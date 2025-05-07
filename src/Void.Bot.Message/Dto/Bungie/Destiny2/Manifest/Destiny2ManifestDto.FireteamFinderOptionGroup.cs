namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 组队查找器选项组定义
/// </summary>
public class Destiny2ManifestFireteamFinderOptionGroupDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 降序排序优先级
    /// </summary>
    public int DescendingSortPriority { get; set; }
}

/// <summary>
/// 组队查找器选项组清单
/// </summary>
public class Destiny2ManifestFireteamFinderOptionGroupDto : Dictionary<string, Destiny2ManifestFireteamFinderOptionGroupDefinitionDto>
{
}