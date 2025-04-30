namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 赛季定义，描述游戏中的赛季内容和时间范围
/// </summary>
public class Destiny2ManifestSeasonDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含赛季名称、描述和图标
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 背景图片路径，赛季界面使用的背景图片
    /// </summary>
    public string BackgroundImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 赛季编号，游戏中显示的赛季序号
    /// </summary>
    public int SeasonNumber { get; set; }

    /// <summary>
    /// 开始日期，赛季正式开始的时间
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// 结束日期，赛季正式结束的时间
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// 赛季通行证哈希，关联到赛季通行证定义
    /// </summary>
    public uint SeasonPassHash { get; set; }

    /// <summary>
    /// 赛季通行证进度哈希，关联到赛季等级进度系统
    /// </summary>
    public uint SeasonPassProgressionHash { get; set; }

    /// <summary>
    /// 神器物品哈希，关联到赛季神器物品
    /// </summary>
    public uint ArtifactItemHash { get; set; }

    /// <summary>
    /// 印章展示节点哈希，关联到赛季印章成就
    /// </summary>
    public uint SealPresentationNodeHash { get; set; }

    /// <summary>
    /// 赛季活动列表，包含赛季中的各个阶段活动
    /// </summary>
    public List<Destiny2ManifestSeasonActDefinitionDto> Acts { get; set; } = [];

    /// <summary>
    /// 赛季挑战展示节点哈希，关联到赛季挑战任务系统
    /// </summary>
    public uint SeasonalChallengesPresentationNodeHash { get; set; }

    /// <summary>
    /// 赛季预览信息，包含赛季的宣传展示内容
    /// </summary>
    public Destiny2ManifestSeasonPreviewDefinitionDto Preview { get; set; } = new();
}

/// <summary>
/// 赛季活动定义，描述赛季中的特定活动阶段
/// </summary>
public class Destiny2ManifestSeasonActDefinitionDto
{
    /// <summary>
    /// 显示名称，活动阶段的名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 开始时间，活动阶段的开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 等级数量，此活动阶段包含的等级数
    /// </summary>
    public int RankCount { get; set; }
}

/// <summary>
/// 赛季预览定义，描述赛季的宣传和预览信息
/// </summary>
public class Destiny2ManifestSeasonPreviewDefinitionDto
{
    /// <summary>
    /// 显示名称，预览内容的标题
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 开始时间，预览内容的发布时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 等级数量，预览中展示的等级数量
    /// </summary>
    public int RankCount { get; set; }
}

/// <summary>
/// 赛季清单，存储所有赛季定义的字典集合
/// </summary>
public class Destiny2ManifestSeasonDto : Dictionary<string, Destiny2ManifestSeasonDefinitionDto>
{
}