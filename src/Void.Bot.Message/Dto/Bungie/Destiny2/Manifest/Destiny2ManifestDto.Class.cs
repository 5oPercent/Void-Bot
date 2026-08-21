using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 职业类型定义
/// </summary>
public class Destiny2ManifestClassDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含职业名称、描述和图标
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 职业类型
    /// </summary>
    public Destiny2ClassEnum ClassType { get; set; }
    
    /// <summary>
    /// 性别职业名称
    /// </summary>
    public Dictionary<string, string> GenderedClassNames { get; set; } = [];

    /// <summary>
    /// 体型哈希职业名称
    /// </summary>
    public Dictionary<string, string> GenderedClassNamesByGenderHash { get; set; } = [];
}

/// <summary>
/// 职业类型清单
/// </summary>
public class Destiny2ManifestClassDto : Dictionary<string, Destiny2ManifestClassDefinitionDto>
{
}