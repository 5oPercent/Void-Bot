using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraWeaponTemplateRequestHandler : IRequestHandler<GetEraWeaponTemplateRequest, GetEraWeaponTemplateResponse>
{
    private readonly IEraWeaponService _service;

    public GetEraWeaponTemplateRequestHandler(IEraWeaponService service)
    {
        _service = service;
    }

    public async Task<GetEraWeaponTemplateResponse> Handle(IReceiveContext<GetEraWeaponTemplateRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetTemplateAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
