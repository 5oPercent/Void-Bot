namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

/// <summary>
/// 武器外观资源。
/// </summary>
public class Destiny2WeaponAppearanceDto
{
    /// <summary>
    /// 武器展示大图路径。
    /// </summary>
    public string Screenshot { get; set; } = string.Empty;

    /// <summary>
    /// 当前版本图标水印路径。
    /// </summary>
    public string IconWatermark { get; set; } = string.Empty;

    /// <summary>
    /// 退役版本图标水印路径。
    /// </summary>
    public string IconWatermarkShelved { get; set; } = string.Empty;

    /// <summary>
    /// 辅助图标路径。
    /// </summary>
    public string SecondaryIcon { get; set; } = string.Empty;

    /// <summary>
    /// 辅助覆盖图路径。
    /// </summary>
    public string SecondaryOverlay { get; set; } = string.Empty;

    /// <summary>
    /// 辅助特殊背景图路径。
    /// </summary>
    public string SecondarySpecial { get; set; } = string.Empty;

    /// <summary>
    /// 武器背景颜色。
    /// </summary>
    public Destiny2WeaponColorDto BackgroundColor { get; set; } = new();
}

/// <summary>
/// 武器背景颜色的 RGBA 分量。
/// </summary>
public class Destiny2WeaponColorDto
{
    /// <summary>
    /// 红色分量。
    /// </summary>
    public int Red { get; set; }

    /// <summary>
    /// 绿色分量。
    /// </summary>
    public int Green { get; set; }

    /// <summary>
    /// 蓝色分量。
    /// </summary>
    public int Blue { get; set; }

    /// <summary>
    /// 透明度分量。
    /// </summary>
    public int Alpha { get; set; }
}
