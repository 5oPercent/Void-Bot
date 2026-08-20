namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

/// <summary>
/// 武器制造商或铸造厂信息。
/// </summary>
public class Destiny2WeaponFoundryDto
{
    /// <summary>
    /// 制造商特质哈希。
    /// </summary>
    public uint TraitHash { get; set; }

    /// <summary>
    /// 制造商特质标识。
    /// </summary>
    public string TraitId { get; set; } = string.Empty;

    /// <summary>
    /// 本地化制造商名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 本地化制造商描述。
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 制造商图标路径。
    /// </summary>
    public string Icon { get; set; } = string.Empty;
}
