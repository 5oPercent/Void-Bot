using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Bungie.Destiny2.Weapon;
using Void.Bot.Message.Commands.Bungie.Destiny2;

namespace Void.Bot.Core.Handler.CommandHandler.Bungie;

/// <summary>
/// 处理 Destiny 2 武器派生缓存重建命令。
/// </summary>
public class RebuildDestiny2WeaponCacheCommandHandler
    : ICommandHandler<RebuildDestiny2WeaponCacheCommand, RebuildDestiny2WeaponCacheResponse>
{
    private readonly IDestiny2WeaponService _service;

    public RebuildDestiny2WeaponCacheCommandHandler(IDestiny2WeaponService service)
    {
        _service = service;
    }

    public async Task<RebuildDestiny2WeaponCacheResponse> Handle(
        IReceiveContext<RebuildDestiny2WeaponCacheCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service.RebuildCacheAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
