using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Handler.RequestHandler.Bungie;

public class GetDestiny2SeasonRequestHandler : IRequestHandler<GetDestiny2SeasonRequest, GetDestiny2SeasonResponse>
{
    private readonly IBungieService _service;

    public GetDestiny2SeasonRequestHandler(IBungieService service)
    {
        _service = service;
    }

    public async Task<GetDestiny2SeasonResponse> Handle(IReceiveContext<GetDestiny2SeasonRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetDestiny2SeasonAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}