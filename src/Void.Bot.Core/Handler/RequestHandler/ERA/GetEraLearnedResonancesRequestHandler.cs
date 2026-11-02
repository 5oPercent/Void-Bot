using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public class GetEraLearnedResonancesRequestHandler : IRequestHandler<GetEraLearnedResonancesRequest, GetEraLearnedResonancesResponse>
{
    private readonly IEraResonanceService _service;

    public GetEraLearnedResonancesRequestHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<GetEraLearnedResonancesResponse> Handle(
        IReceiveContext<GetEraLearnedResonancesRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .GetLearnedResonancesAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
