using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

namespace Void.Bot.Message.Commands.Bungie.Destiny2;

/// <summary>
/// 使用已缓存的清单组件重建 Destiny 2 武器派生缓存。
/// </summary>
public class RebuildDestiny2WeaponCacheCommand : ICommand
{
    /// <summary>
    /// 需要重建缓存的清单语言；未传或为空时使用服务端同步语言配置。
    /// </summary>
    public List<string>? Languages { get; set; }
}

/// <summary>
/// Destiny 2 武器派生缓存重建响应。
/// </summary>
public class RebuildDestiny2WeaponCacheResponse : VoidBotResponse<Destiny2WeaponCacheRebuildJobDto>
{
}
