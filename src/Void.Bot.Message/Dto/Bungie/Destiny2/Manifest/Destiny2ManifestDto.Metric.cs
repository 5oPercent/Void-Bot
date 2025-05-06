namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 指标定义，描述游戏中可追踪的统计数据和成就指标
/// </summary>
public class Destiny2ManifestMetricDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含名称、描述和图标等信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 追踪目标哈希，关联到此指标追踪的具体目标
    /// </summary>
    public uint TrackingObjectiveHash { get; set; }

    /// <summary>
    /// 是否较低的值更好，用于确定指标的排序和评估方式
    /// </summary>
    public bool LowerValueIsBetter { get; set; }

    /// <summary>
    /// 展示节点类型，定义指标在UI中的展示方式
    /// </summary>
    public uint PresentationNodeType { get; set; }

    /// <summary>
    /// 特性ID列表，描述此指标的特性标签
    /// </summary>
    public List<string> TraitIds { get; set; } = [];

    /// <summary>
    /// 特性哈希列表，关联到特性定义
    /// </summary>
    public List<uint> TraitHashes { get; set; } = [];

    /// <summary>
    /// 父节点哈希列表，指标在UI层级结构中的父节点
    /// </summary>
    public List<uint> ParentNodeHashes { get; set; } = [];
}

/// <summary>
/// 指标清单，存储所有指标定义的字典集合
/// </summary>
public class Destiny2ManifestMetricDto : Dictionary<string, Destiny2ManifestMetricDefinitionDto>
{
}