using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public class GetEraResonanceProfileRequestHandler : IRequestHandler<GetEraResonanceProfileRequest, GetEraResonanceProfileResponse>
{
    private readonly IEraResonanceService _service;

    public GetEraResonanceProfileRequestHandler(IEraResonanceService service)
    {
        _service = service;
    }

    public async Task<GetEraResonanceProfileResponse> Handle(
        IReceiveContext<GetEraResonanceProfileRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .GetProfileAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
