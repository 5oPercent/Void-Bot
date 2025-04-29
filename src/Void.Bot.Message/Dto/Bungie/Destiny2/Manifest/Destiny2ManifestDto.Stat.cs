using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 统计定义
/// </summary>
public class Destiny2ManifestStatDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 统计值聚合类型
    /// </summary>
    public Destiny2StatAggregationTypeEnum AggregationType { get; set; }

    /// <summary>
    /// 统计类别
    /// </summary>
    public Destiny2StatCategoryEnum StatCategory { get; set; }
    
    /// <summary>
    /// 是否有计算
    /// </summary>
    public bool HasComputedBlock { get; set; }

    /// <summary>
    /// 是否插值
    /// </summary>
    public bool Interpolate { get; set; }
}

/// <summary>
/// 统计清单
/// </summary>
public class Destiny2ManifestStatDto : Dictionary<string, Destiny2ManifestStatDefinitionDto>
{
}