using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 命运2插槽类别定义，描述游戏中装备插槽的分类和样式
/// </summary>
public class Destiny2ManifestSocketCategoryDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含插槽类别的名称、描述和图标
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// UI类别样式哈希，关联到界面上插槽类别的视觉样式定义
    /// </summary>
    public uint UiCategoryStyle { get; set; }

    /// <summary>
    /// 类别样式，定义插槽的显示方式和功能特性
    /// </summary>
    public Destiny2SocketCategoryStyleEnum CategoryStyle { get; set; }
}

/// <summary>
/// 命运2插槽类别清单，存储所有插槽类别定义的字典集合
/// </summary>
public class Destiny2ManifestSocketCategoryDto : Dictionary<string, Destiny2ManifestSocketCategoryDefinitionDto>
{
}