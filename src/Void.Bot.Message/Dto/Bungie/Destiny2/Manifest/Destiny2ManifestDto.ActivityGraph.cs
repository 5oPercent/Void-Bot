using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 活动视图定义
/// </summary>
public class Destiny2ManifestActivityGraphDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 代表地图视图中的视觉“节点”。这些是您可以在地图上点击的活动。
    /// </summary>
    public List<Destiny2ManifestActivityGraphNodeDto> Nodes { get; set; } = [];

    /// <summary>
    /// 表示地图上出现的单次/特殊 UI 元素
    /// </summary>
    public List<Destiny2ManifestActivityGraphNodeArtElementDto> ArtElements { get; set; } = [];
    
    /// <summary>
    /// 图节点之间的连接
    /// </summary>
    public List<Destiny2ManifestActivityGraphConnectionDto> Connections { get; set; } = [];
    
    /// <summary>
    /// 目标
    /// </summary>
    public List<Destiny2ManifestActivityGraphDisplayObjectiveDto> DisplayObjectives { get; set; } = [];

    /// <summary>
    /// 进度
    /// </summary>
    public List<Destiny2ManifestActivityGraphDisplayProgressionDto> DisplayProgressions { get; set; } = [];

    /// <summary>
    /// 与其它活动图关联信息
    /// </summary>
    public List<Destiny2ManifestLinkedGraphDefinitionDto> LinkedGraphs { get; set; } = [];
    
    public int UiScreen { get; set; }
    
    /// <summary>
    /// 忽略里程碑
    /// </summary>
    public bool IgnoreForMilestones { get; set; }
}

#region Node

/// <summary>
/// 命运2清单活动节点
/// </summary>
public class Destiny2ManifestActivityGraphNodeDto
{
    public uint NodeId { get; set; }

    public Destiny2ManifestDisplayPropertiesDefinitionDto OverrideDisplay { get; set; } = new();
    
    public Destiny2ManifestPositionDefinitionDto Position { get; set; } = new();

    public List<Destiny2ManifestActivityGraphNodeFeaturingStateDto> FeaturingStates { get; set; } = [];

    public List<Destiny2ManifestActivityGraphNodeActivityDto> Activities { get; set; } = [];
}

/// <summary>
/// 命运2清单活动节点视觉状态
/// </summary>
public class Destiny2ManifestActivityGraphNodeFeaturingStateDto
{
    /// <summary>
    /// 高亮类型
    /// </summary>
    public Destiny2ActivityGraphNodeHighlightTypeEnum HighlightType { get; set; }
}

/// <summary>
/// 命运2清单活动节点
/// </summary>
public class Destiny2ManifestActivityGraphNodeActivityDto
{
    /// <summary>
    /// 活动节点ID
    /// </summary>
    public uint ActivityId { get; set; }

    /// <summary>
    /// 活动哈希
    /// </summary>
    public uint ActivityHash { get; set; }
}

/// <summary>
/// 命运2清单活动节点状态
/// </summary>
public class Destiny2ManifestActivityGraphNodeStateDto
{
    public Destiny2GraphNodeStateEnum State { get; set; }
}

#endregion

/// <summary>
/// 艺术元素定义
/// </summary>
public class Destiny2ManifestActivityGraphNodeArtElementDto
{
    public Destiny2ManifestPositionDefinitionDto Position { get; set; } = new();
}

/// <summary>
/// 图节点之间的连接
/// </summary>
public class Destiny2ManifestActivityGraphConnectionDto
{
    public uint SourceNodeHash { get; set; }

    public uint DestNodeHash { get; set; }
}

/// <summary>
/// 地图上的活动目标
/// </summary>
public class Destiny2ManifestActivityGraphDisplayObjectiveDto
{
    public uint Id { get; set; }
    
    public uint ObjectiveHash { get; set; }
}

/// <summary>
/// 地图上的活动进度
/// </summary>
public class Destiny2ManifestActivityGraphDisplayProgressionDto
{
    public uint Id { get; set; }

    public uint ProgressionHash { get; set; }
}

/// <summary>
/// 命运2清单活动图列表条目定义，用于引用活动图
/// </summary>
public class Destiny2ManifestActivityGraphListEntryDto
{    /// <summary>
    /// 活动图哈希值，用于标识和检索特定的活动图
    /// </summary>
    public uint ActivityGraphHash { get; set; }
}

/// <summary>
/// 活动视图清单
/// </summary>
public class Destiny2ManifestActivityGraphDto : Dictionary<string, Destiny2ManifestActivityGraphDefinitionDto>
{
}