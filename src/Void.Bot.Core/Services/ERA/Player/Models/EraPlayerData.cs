using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA.Player.Models;

/// <summary>
/// ERA 玩家预加载结果，只在当前请求作用域内保存，不属于全局玩家缓存。
/// </summary>
public class EraPlayerData
{
    /// <summary>
    /// 已验证的当前账号玩家；公共配置和初始化请求可以为空。
    /// </summary>
    public EraPlayer? CurrentPlayer
    {
        get;
        internal set;
    }

    /// <summary>
    /// 当前请求已加载的玩家，包含管理员指定的目标玩家。
    /// </summary>
    internal Dictionary<Guid, EraPlayer> Players
    {
        get;
    } = new Dictionary<Guid, EraPlayer>();

    /// <summary>
    /// 已在当前命令事务中取得行锁的玩家 ID。
    /// </summary>
    internal HashSet<Guid> LockedPlayers
    {
        get;
    } = new HashSet<Guid>();
}
