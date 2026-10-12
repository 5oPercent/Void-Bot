using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public class GetEraSkillConfigurationRequestHandler : IRequestHandler<GetEraSkillConfigurationRequest, GetEraSkillConfigurationResponse>
{
    private readonly IEraSkillService _service;

    public GetEraSkillConfigurationRequestHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<GetEraSkillConfigurationResponse> Handle(
        IReceiveContext<GetEraSkillConfigurationRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .GetConfigurationAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
