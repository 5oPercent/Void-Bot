using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Configuration;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public class GetEraResonanceConfigurationRequestHandler : IRequestHandler<GetEraResonanceConfigurationRequest, GetEraResonanceConfigurationResponse>
{
    private readonly IEraAbilityCatalogQueryService _service;

    public GetEraResonanceConfigurationRequestHandler(IEraAbilityCatalogQueryService service)
    {
        _service = service;
    }

    public async Task<GetEraResonanceConfigurationResponse> Handle(
        IReceiveContext<GetEraResonanceConfigurationRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .GetConfigurationAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
