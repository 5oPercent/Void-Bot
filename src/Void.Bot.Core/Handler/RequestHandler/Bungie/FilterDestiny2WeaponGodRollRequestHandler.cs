using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Bungie.Destiny2.Weapon;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Handler.RequestHandler.Bungie;

public class FilterDestiny2WeaponGodRollRequestHandler
    : IRequestHandler<FilterDestiny2WeaponGodRollRequest, FilterDestiny2WeaponGodRollResponse>
{
    private readonly IDestiny2WeaponService _service;

    public FilterDestiny2WeaponGodRollRequestHandler(IDestiny2WeaponService service)
    {
        _service = service;
    }

    public Task<FilterDestiny2WeaponGodRollResponse> Handle(
        IReceiveContext<FilterDestiny2WeaponGodRollRequest> context,
        CancellationToken cancellationToken)
    {
        return _service.FilterGodRollAsync(context.Message, cancellationToken);
    }
}
