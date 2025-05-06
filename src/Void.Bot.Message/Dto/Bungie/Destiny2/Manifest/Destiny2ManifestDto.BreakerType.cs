using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 破盾类型定义
/// </summary>
public class Destiny2ManifestBreakerTypeDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 破盾类型枚举值，对应到破盾类型系统中的具体类型
    /// </summary>
    public Destiny2BreakerTypeEnum EnumValue { get; set; }
}

/// <summary>
/// 破盾类型清单
/// </summary>
public class Destiny2ManifestBreakerTypeDto : Dictionary<string, Destiny2ManifestBreakerTypeDefinitionDto>
{
}