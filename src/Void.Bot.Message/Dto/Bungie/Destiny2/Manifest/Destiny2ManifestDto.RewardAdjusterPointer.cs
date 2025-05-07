namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 奖励调节指针定义，用于定向引用游戏内奖励修改系统
/// </summary>
public class Destiny2ManifestRewardAdjusterPointerDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 调节器类型，定义应用于奖励的调整方式类型
    /// </summary>
    public int AdjusterType { get; set; } = 0;
}

/// <summary>
/// 奖励调节指针清单，存储所有奖励调节指针定义的字典集合
/// </summary>
public class Destiny2ManifestRewardAdjusterPointerDto : Dictionary<string, Destiny2ManifestRewardAdjusterPointerDefinitionDto>
{
}