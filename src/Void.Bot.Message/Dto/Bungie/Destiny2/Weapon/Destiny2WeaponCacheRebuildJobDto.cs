namespace Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

/// <summary>
/// 武器派生缓存后台重建任务信息。
/// </summary>
public class Destiny2WeaponCacheRebuildJobDto
{
    /// <summary>
    /// 后台任务标识。
    /// </summary>
    public string JobId { get; set; } = string.Empty;

    /// <summary>
    /// 任务需要重建的清单语言。
    /// </summary>
    public List<string> Languages { get; set; } = [];
}
