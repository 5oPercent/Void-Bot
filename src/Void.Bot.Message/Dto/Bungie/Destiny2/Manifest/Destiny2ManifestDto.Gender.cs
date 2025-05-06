using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 体型定义
/// </summary>
public class Destiny2ManifestGenderDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 体型类型
    /// </summary>
    public Destiny2GenderEnum GenderType { get; set; }
    
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
}

/// <summary>
/// 体型类型清单
/// </summary>
public class Destiny2ManifestGenderDto : Dictionary<string, Destiny2ManifestGenderDefinitionDto>
{
}