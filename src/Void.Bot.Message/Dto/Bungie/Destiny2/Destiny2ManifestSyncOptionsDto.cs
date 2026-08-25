namespace Void.Bot.Message.Dto.Bungie.Destiny2;

/// <summary>
/// Destiny 2 Manifest 同步可选项。
/// </summary>
public class Destiny2ManifestSyncOptionsDto
{
    /// <summary>
    /// 可同步的语言代码。
    /// </summary>
    public List<string> Languages { get; set; } = [];

    /// <summary>
    /// 可同步的清单组件标识。
    /// </summary>
    public List<string> Components { get; set; } = [];
}
