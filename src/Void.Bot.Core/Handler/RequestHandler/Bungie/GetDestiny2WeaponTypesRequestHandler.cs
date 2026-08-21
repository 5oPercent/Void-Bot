using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Bungie.Destiny2.Weapon;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Handler.RequestHandler.Bungie;

public class GetDestiny2WeaponTypesRequestHandler
    : IRequestHandler<GetDestiny2WeaponTypesRequest, GetDestiny2WeaponTypesResponse>
{
    private readonly IDestiny2WeaponService _service;

    public GetDestiny2WeaponTypesRequestHandler(IDestiny2WeaponService service)
    {
        _service = service;
    }

    public Task<GetDestiny2WeaponTypesResponse> Handle(
        IReceiveContext<GetDestiny2WeaponTypesRequest> context,
        CancellationToken cancellationToken)
    {
        return _service.GetWeaponTypesAsync(context.Message, cancellationToken);
    }
}
