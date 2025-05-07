namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 传说故事定义，描述游戏中的传说、背景故事和相关叙事内容
/// </summary>
public class Destiny2ManifestLoreDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();
    
    /// <summary>
    /// 副标题，传说故事的补充标题或章节标识
    /// </summary>
    public string SubTitle { get; set; } = string.Empty;
}

/// <summary>
/// 传说故事清单，存储所有传说故事定义的字典集合
/// </summary>
public class Destiny2ManifestLoreDto : Dictionary<string, Destiny2ManifestLoreDefinitionDto>
{
}