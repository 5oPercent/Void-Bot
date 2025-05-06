namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 装备配置图标定义，描述游戏中装备配置使用的图标
/// </summary>
public class Destiny2ManifestLoadoutIconDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 图标图像路径，指向代表此装备配置图标的图像资源
    /// </summary>
    public string IconImagePath { get; set; } = string.Empty;
}

/// <summary>
/// 装备配置图标清单，存储所有装备配置图标定义的字典集合
/// </summary>
public class Destiny2ManifestLoadoutIconDto : Dictionary<string, Destiny2ManifestLoadoutIconDefinitionDto>
{
}