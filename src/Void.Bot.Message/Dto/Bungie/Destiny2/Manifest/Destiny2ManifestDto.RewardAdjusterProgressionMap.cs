namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 奖励调节进度映射定义，描述游戏中进度与奖励调整的关联关系
/// </summary>
public class Destiny2ManifestRewardAdjusterProgressionMapDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 是否叠加，标识调整效果是否与其他调整器叠加
    /// </summary>
    public bool IsAdditive { get; set; }
}

/// <summary>
/// 奖励调节进度映射清单，存储所有奖励调节进度映射定义的字典集合
/// </summary>
public class Destiny2ManifestRewardAdjusterProgressionMapDto : Dictionary<string, Destiny2ManifestRewardAdjusterProgressionMapDefinitionDto>
{
}