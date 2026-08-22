using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.EventAction;
using Void.Bot.Message.Request.EventAction;

namespace Void.Bot.Core.Handler.RequestHandler.EventAction;

public class GetEventActionDefinitionRequestHandler(IEventActionRuleService service)
    : IRequestHandler<GetEventActionDefinitionRequest, GetEventActionDefinitionResponse>
{
    public Task<GetEventActionDefinitionResponse> Handle(
        IReceiveContext<GetEventActionDefinitionRequest> context,
        CancellationToken cancellationToken)
    {
        return service.GetDefinitionsAsync(cancellationToken);
    }
}
