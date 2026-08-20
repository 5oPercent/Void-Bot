namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

/// <summary>
/// 武器版本与注入信息。
/// </summary>
public class Destiny2WeaponVersionDto
{
    /// <summary>
    /// 当前版本索引。
    /// </summary>
    public uint CurrentVersion { get; set; }

    /// <summary>
    /// 各版本对应的力量上限哈希。
    /// </summary>
    public List<uint> PowerCapHashes { get; set; } = [];

    /// <summary>
    /// 各版本对应的水印图标路径。
    /// </summary>
    public List<string> DisplayWatermarkIcons { get; set; } = [];

    /// <summary>
    /// 注入类别哈希。
    /// </summary>
    public uint InfusionCategoryHash { get; set; }

    /// <summary>
    /// 本地化注入类别名称。
    /// </summary>
    public string InfusionCategoryName { get; set; } = string.Empty;
}
