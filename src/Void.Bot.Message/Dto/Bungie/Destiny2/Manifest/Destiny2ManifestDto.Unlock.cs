namespace Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;

/// <summary>
/// 解锁定义
/// </summary>
public class Destiny2ManifestUnlockDefinitionDto : Destiny2ManifestDefinitionDto
{
    /// <summary>
    /// 显示属性
    /// </summary>
    public Destiny2ManifestDisplayPropertiesDefinitionDto DisplayProperties { get; set; } = new();

    /// <summary>
    /// 解锁类型
    /// </summary>
    public int UnlockType { get; set; }
    
    /// <summary>
    /// 作用域
    /// </summary>
    public int Scope { get; set; }
}

/// <summary>
/// 解锁清单
/// </summary>
public class Destiny2ManifestUnlockDto : Dictionary<string, Destiny2ManifestGenderDefinitionDto>
{
}