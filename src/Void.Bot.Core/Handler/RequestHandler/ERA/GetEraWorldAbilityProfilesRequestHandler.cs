using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.AbilityProfile;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public sealed class GetEraWorldAbilityProfilesRequestHandler : IRequestHandler<GetEraWorldAbilityProfilesRequest, GetEraWorldAbilityProfilesResponse>
{
    private readonly IEraWorldAbilityService _service;

    public GetEraWorldAbilityProfilesRequestHandler(IEraWorldAbilityService service)
    {
        _service = service;
    }

    public async Task<GetEraWorldAbilityProfilesResponse> Handle(IReceiveContext<GetEraWorldAbilityProfilesRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
