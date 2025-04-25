namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 伤害类型定义
/// </summary>
public class Destiny2ManifestDamageTypeDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 透明图标路径
    /// </summary>
    public string TransparentIconPath { get; set; } = string.Empty;

    /// <summary>
    /// 显示图标
    /// </summary>
    public bool ShowIcon { get; set; }

    /// <summary>
    /// 当前伤害类型枚举值
    /// </summary>
    public int EnumValue { get; set; }

    /// <summary>
    /// 伤害类型关联的颜色
    /// </summary>
    public Destiny2ManifestColorDefinitionDto Color { get; set; } = new();
}

/// <summary>
/// 伤害类型清单
/// </summary>
public class Destiny2ManifestDamageTypeDto : Dictionary<string, Destiny2ManifestDamageTypeDefinitionDto>
{
}