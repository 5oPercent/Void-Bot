namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 活动修改器定义定义
/// </summary>
public class Destiny2ManifestActivityModifierDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 是否在导航模式中显示，控制此活动修改器是否在导航界面中可见
    /// </summary>
    public bool DisplayNavMode { get; set; }

    /// <summary>
    /// 是否在活动选择界面显示，控制此活动修改器是否在活动选择界面中可见
    /// </summary>
    public bool DisplayInActivitySelection { get; set; }
}

/// <summary>
/// 活动修改器定义清单
/// </summary>
public class Destiny2ManifestActivityModifierDto : Dictionary<string, Destiny2ManifestActivityModifierDefinitionDto>
{
}