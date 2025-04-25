namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 地点定义
/// </summary>
public class Destiny2ManifestPlaceDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
}

/// <summary>
/// 地点清单
/// </summary>
public class Destiny2ManifestPlaceDto : Dictionary<string, Destiny2ManifestPlaceDefinitionDto>
{
}