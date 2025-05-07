namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 命运2守护者等级定义，描述游戏中玩家的守护者等级系统
/// </summary>
public class Destiny2ManifestGuardianRankDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含守护者等级的名称、描述和图标等基本显示信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 等级数值，表示守护者等级的具体数字
    /// </summary>
    public int RankNumber { get; set; }

    /// <summary>
    /// 展示节点哈希，关联到包含此等级详细信息的展示节点
    /// </summary>
    public uint PresentationNodeHash { get; set; }
    
    /// <summary>
    /// 前景图像路径，用于显示等级的主要图标
    /// </summary>
    public string ForegroundImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 覆盖图像路径，用于显示等级的额外视觉效果层
    /// </summary>
    public string OverlayImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 覆盖蒙版图像路径，用于控制覆盖图像的显示区域
    /// </summary>
    public string OverlayMaskImagePath { get; set; } = string.Empty;
}

/// <summary>
/// 命运2守护者等级清单，包含所有守护者等级的定义集合
/// </summary>
public class Destiny2ManifestGuardianRankDto : Dictionary<string, Destiny2ManifestGuardianRankDefinitionDto>
{
}