using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 地点定义，描述游戏中的位置信息和相关属性
/// </summary>
public class Destiny2ManifestLocationDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 地点哈希值，用于唯一标识该地点
    /// </summary>
    public uint LocationHash { get; set; }

    /// <summary>
    /// 地点发布列表，包含此地点的不同版本或变种
    /// </summary>
    public List<Destiny2ManifestLocationReleaseDefinitionDto> LocationReleases { get; set; } = [];
}

/// <summary>
/// 地点发布定义，描述特定地点发布版本的详细信息
/// </summary>
public class Destiny2ManifestLocationReleaseDefinitionDto
{
    /// <summary>
    /// 显示属性，包含名称、描述和图标等信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 小型透明图标路径，用于界面显示
    /// </summary>
    public string SmallTransparentIcon { get; set; } = string.Empty;

    /// <summary>
    /// 地图图标路径，用于在世界地图上显示
    /// </summary>
    public string MapIcon { get; set; } = string.Empty;

    /// <summary>
    /// 大型透明图标路径，用于界面显示
    /// </summary>
    public string LargeTransparentIcon { get; set; } = string.Empty;

    /// <summary>
    /// 出生点位置标识符
    /// </summary>
    public uint SpawnPoint { get; set; }

    /// <summary>
    /// 目的地哈希值，关联此地点所在的目的地
    /// </summary>
    public uint DestinationHash { get; set; }

    /// <summary>
    /// 活动哈希值，关联此地点可进行的活动
    /// </summary>
    public uint ActivityHash { get; set; }

    /// <summary>
    /// 活动图哈希值，关联活动的图表数据
    /// </summary>
    public uint ActivityGraphHash { get; set; }

    /// <summary>
    /// 活动图节点哈希值，在活动图中的具体节点位置
    /// </summary>
    public uint ActivityGraphNodeHash { get; set; }

    /// <summary>
    /// 活动区域名称标识符
    /// </summary>
    public uint ActivityBubbleName { get; set; }
    
    /// <summary>
    /// 活动路径包标识符，用于导航系统
    /// </summary>
    public uint ActivityPathBundle { get; set; }

    /// <summary>
    /// 活动路径目的地标识符，路径的终点
    /// </summary>
    public uint ActivityPathDestination { get; set; }

    /// <summary>
    /// 导航点类型，指定此地点在导航系统中的类型
    /// </summary>
    public Destiny2ActivityNavPointTypeEnum NavPointType { get; set; }

    /// <summary>
    /// 世界坐标位置，地点在3D世界中的坐标值
    /// </summary>
    public List<int> WorldPosition { get; set; } = [];
}

/// <summary>
/// 地点清单，存储所有地点定义的字典集合
/// </summary>
public class Destiny2ManifestLocationDto : Dictionary<string, Destiny2ManifestLocationDefinitionDto>
{
}