namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 角色自定义类别定义
/// </summary>
public class Destiny2ManifestCharacterCustomizationCategoryDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
}

/// <summary>
/// 角色自定义类别清单
/// </summary>
public class Destiny2ManifestCharacterCustomizationCategoryDto : Dictionary<string, Destiny2ManifestCharacterCustomizationCategoryDefinitionDto>
{
}