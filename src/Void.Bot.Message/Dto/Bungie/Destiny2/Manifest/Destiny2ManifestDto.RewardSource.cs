namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 奖励来源定义
/// </summary>
public class Destiny2ManifestRewardSourceDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 分类
    /// </summary>
    public int Category { get; set; }
}

/// <summary>
/// 奖励来源清单
/// </summary>
public class Destiny2ManifestRewardSourceDto : Dictionary<string, Destiny2ManifestRewardSourceDefinitionDto>
{
}