namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 装备配置颜色定义，描述游戏中装备配置使用的颜色方案
/// </summary>
public class Destiny2ManifestLoadoutColorDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 颜色图像路径，指向代表此颜色的图像资源
    /// </summary>
    public string ColorImagePath { get; set; } = string.Empty;
}

/// <summary>
/// 装备配置颜色清单，存储所有装备配置颜色定义的字典集合
/// </summary>
public class Destiny2ManifestLoadoutColorDto : Dictionary<string, Destiny2ManifestLoadoutColorDefinitionDto>
{
}