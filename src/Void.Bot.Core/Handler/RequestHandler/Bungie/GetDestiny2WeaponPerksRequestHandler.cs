using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Bungie.Destiny2.Weapon;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Handler.RequestHandler.Bungie;

public class GetDestiny2WeaponPerksRequestHandler
    : IRequestHandler<GetDestiny2WeaponPerksRequest, GetDestiny2WeaponPerksResponse>
{
    private readonly IDestiny2WeaponService _service;

    public GetDestiny2WeaponPerksRequestHandler(IDestiny2WeaponService service)
    {
        _service = service;
    }

    public Task<GetDestiny2WeaponPerksResponse> Handle(
        IReceiveContext<GetDestiny2WeaponPerksRequest> context,
        CancellationToken cancellationToken)
    {
        return _service.GetPerksAsync(context.Message, cancellationToken);
    }
}
