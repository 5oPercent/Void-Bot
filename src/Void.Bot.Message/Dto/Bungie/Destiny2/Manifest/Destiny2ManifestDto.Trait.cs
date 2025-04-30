namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 命运2特质定义，描述装备和物品的特性、属性和类别
/// </summary>
public class Destiny2ManifestTraitDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 显示提示，提供关于特质如何在界面中显示的额外信息
    /// </summary>
    public string DisplayHint { get; set; } = string.Empty;
}

/// <summary>
/// 命运2特质清单，存储所有特质定义的字典集合
/// </summary>
public class Destiny2ManifestTraitDto : Dictionary<string, Destiny2ManifestTraitDefinitionDto>
{
}