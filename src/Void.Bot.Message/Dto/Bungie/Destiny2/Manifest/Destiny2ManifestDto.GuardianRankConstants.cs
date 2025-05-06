namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 命运2守护者等级常量定义，描述游戏中守护者等级系统的全局配置
/// </summary>
public class Destiny2ManifestGuardianRankConstantsDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性，包含守护者等级系统的名称、描述和图标等基本显示信息
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 等级总数，定义守护者等级系统中的等级数量
    /// </summary>
    public int RankCount { get; set; }

    /// <summary>
    /// 守护者等级哈希列表，包含所有可用守护者等级的哈希值
    /// </summary>
    public List<uint> GuardianRankHashes { get; set; } = [];

    /// <summary>
    /// 根节点哈希，关联到守护者等级系统的起始节点
    /// </summary>
    public uint RootNodeHash { get; set; }

    /// <summary>
    /// 图标背景，包含守护者等级图标使用的各种背景图像资源
    /// </summary>
    public Destiny2ManifestGuardianRankIconBackgroundsDefinitionDto IconBackgrounds { get; set; } = new();
}

/// <summary>
/// 命运2守护者等级图标背景定义，包含守护者等级界面使用的各种背景图像路径
/// </summary>
public class Destiny2ManifestGuardianRankIconBackgroundsDefinitionDto
{
    /// <summary>
    /// 空白边框背景图像路径
    /// </summary>
    public string BackgroundEmptyBorderedImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 空白蓝色渐变边框背景图像路径
    /// </summary>
    public string BackgroundEmptyBlueGradientBorderedImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 填充蓝色边框背景图像路径
    /// </summary>
    public string BackgroundFilledBlueBorderedImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 填充蓝色渐变边框背景图像路径
    /// </summary>
    public string BackgroundFilledBlueGradientBorderedImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 填充低透明度蓝色背景图像路径
    /// </summary>
    public string BackgroundFilledBlueLowAlphaImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 填充中等透明度蓝色背景图像路径
    /// </summary>
    public string BackgroundFilledBlueMediumAlphaImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 填充中等透明度灰色边框背景图像路径
    /// </summary>
    public string BackgroundFilledGrayMediumAlphaBorderedImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 填充高透明度灰色边框背景图像路径
    /// </summary>
    public string BackgroundFilledGrayHeavyAlphaBorderedImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 填充中等透明度白色背景图像路径
    /// </summary>
    public string BackgroundFilledWhiteMediumAlphaImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 填充白色背景图像路径
    /// </summary>
    public string BackgroundFilledWhiteImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 白色底板背景图像路径
    /// </summary>
    public string BackgroundPlateWhiteImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 黑色底板背景图像路径
    /// </summary>
    public string BackgroundPlateBlackImagePath { get; set; } = string.Empty;

    /// <summary>
    /// 透明黑色底板背景图像路径
    /// </summary>
    public string BackgroundPlateBlackAlphaImagePath { get; set; } = string.Empty;
}

/// <summary>
/// 命运2守护者等级常量清单，包含守护者等级系统全局配置的定义集合
/// </summary>
public class Destiny2ManifestGuardianRankConstantsDto : Dictionary<string, Destiny2ManifestGuardianRankConstantsDefinitionDto>
{
}