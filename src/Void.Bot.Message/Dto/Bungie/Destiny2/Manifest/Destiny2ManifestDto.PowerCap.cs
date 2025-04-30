namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 能量上限定义，描述物品或装备的最大能量等级限制
/// </summary>
public class Destiny2ManifestPowerCapDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 能量上限，定义物品可以达到的最大能量值
    /// </summary>
    public int PowerCap { get; set; }
}

/// <summary>
/// 能量上限清单，存储所有能量上限定义的字典集合
/// </summary>
public class Destiny2ManifestPowerCapDto : Dictionary<string, Destiny2ManifestPowerCapDefinitionDto>
{
}