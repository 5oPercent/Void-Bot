namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 成就定义
/// </summary>
public class Destiny2ManifestAchievementDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 成就累计阈值，达到该阈值时解锁成就
    /// </summary>
    public int AcccumulatorThreshold { get; set; }

    /// <summary>
    /// 平台索引，指示该成就所属的游戏平台
    /// </summary>
    public int PlatformIndex { get; set; }
}

/// <summary>
/// 成就清单
/// </summary>
public class Destiny2ManifestAchievementDto : Dictionary<string, Destiny2ManifestAchievementDefinitionDto>
{
}