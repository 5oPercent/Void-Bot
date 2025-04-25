namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 奖励等级定义
/// </summary>
public class Destiny2ManifestMedalTierDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 级别名称
    /// </summary>
    public string TierName { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Order { get; set; }
}

/// <summary>
/// 奖励等级清单
/// </summary>
public class Destiny2ManifestMedalTierDto : Dictionary<string, Destiny2ManifestMedalTierDefinitionDto>
{
}