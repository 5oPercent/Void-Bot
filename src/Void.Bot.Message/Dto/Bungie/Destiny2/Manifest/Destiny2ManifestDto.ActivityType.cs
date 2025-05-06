namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 活动类型定义
/// </summary>
public class Destiny2ManifestActivityTypeDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
}

/// <summary>
/// 活动类型清单
/// </summary>
public class Destiny2ManifestActivityTypeDto : Dictionary<string, Destiny2ManifestActivityTypeDefinitionDto>
{
}