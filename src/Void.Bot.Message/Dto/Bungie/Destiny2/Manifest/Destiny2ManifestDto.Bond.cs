namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;


/// <summary>
/// 联动关系定义
/// 可能存在于角色、物品或活动之间，提供特定奖励或解锁特定内容的机制
/// </summary>
public class Destiny2ManifestBondDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 关联到此Bond解锁的内容或功能的唯一标识符
    /// </summary>
    public uint ProvidedUnlockHash { get; set; }

    /// <summary>
    /// 定义解锁后获得的具体值或状态的唯一标识符
    /// </summary>
    public uint ProvidedUnlockValueHash { get; set; }
}

/// <summary>
/// 联动关系清单
/// </summary>
public class Destiny2ManifestBondDto : Dictionary<string, Destiny2ManifestBondDefinitionDto>
{
}