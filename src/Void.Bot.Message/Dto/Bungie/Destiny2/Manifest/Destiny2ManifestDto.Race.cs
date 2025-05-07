using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 种族类型定义
/// </summary>
public class Destiny2ManifestRaceDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 种族类型
    /// </summary>
    public Destiny2RaceEnum RaceType { get; set; }
    
    /// <summary>
    /// 性别种族名称
    /// </summary>
    public Dictionary<string, string> GenderedRaceNames { get; set; } = [];

    /// <summary>
    /// 体型哈希种族名称
    /// </summary>
    public Dictionary<string, string> GenderedRaceNamesByGenderHash { get; set; } = [];
}

/// <summary>
/// 种族类型清单
/// </summary>
public class Destiny2ManifestRaceDto : Dictionary<string, Destiny2ManifestRaceDefinitionDto>
{
}