namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 赛季通行证定义，描述游戏中赛季通行证的结构和进度系统
/// </summary>
public class Destiny2ManifestSeasonPassDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含赛季通行证的名称、描述和图标
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 奖励进度哈希，关联到赛季通行证的免费奖励进度系统
    /// </summary>
    public uint RewardProgressionHash { get; set; }

    /// <summary>
    /// 高级进度哈希，关联到赛季通行证的付费(高级)奖励进度系统
    /// </summary>
    public uint PrestigeProgressionHash { get; set; }
}

/// <summary>
/// 赛季通行证清单，存储所有赛季通行证定义的字典集合
/// </summary>
public class Destiny2ManifestSeasonPassDto : Dictionary<string, Destiny2ManifestSeasonPassDefinitionDto>
{
}