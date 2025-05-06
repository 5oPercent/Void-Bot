namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 装备配置名称定义，描述游戏中装备配置的预设名称
/// </summary>
public class Destiny2ManifestLoadoutNameDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 装备配置名称，表示预设的装备配置名称文本
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// 装备配置名称清单，存储所有装备配置预设名称定义的字典集合
/// </summary>
public class Destiny2ManifestLoadoutNameDto : Dictionary<string, Destiny2ManifestLoadoutNameDefinitionDto>
{
}