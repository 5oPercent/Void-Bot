namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 进度映射定义
/// </summary>
public class Destiny2ManifestProgressionMappingDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 显示单位
    /// </summary>
    public string DisplayUnits { get; set; }
}

/// <summary>
/// 进度映射清单
/// </summary>
public class Destiny2ManifestProgressionMappingDto : Dictionary<string, Destiny2ManifestProgressionMappingDefinitionDto>
{
}