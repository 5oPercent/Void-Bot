using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.AbilityProfile;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public class GetEraResonanceProfileRequestHandler : IRequestHandler<GetEraResonanceProfileRequest, GetEraResonanceProfileResponse>
{
    private readonly IEraAbilityProfileService _service;

    public GetEraResonanceProfileRequestHandler(IEraAbilityProfileService service)
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
