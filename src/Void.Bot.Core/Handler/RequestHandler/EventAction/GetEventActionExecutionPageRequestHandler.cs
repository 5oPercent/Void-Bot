using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.EventAction;
using Void.Bot.Message.Request.EventAction;

namespace Void.Bot.Core.Handler.RequestHandler.EventAction;

public class GetEventActionExecutionPageRequestHandler(IEventActionExecutionService service)
    : IRequestHandler<GetEventActionExecutionPageRequest, GetEventActionExecutionPageResponse>
{
    public Task<GetEventActionExecutionPageResponse> Handle(
        IReceiveContext<GetEventActionExecutionPageRequest> context,
        CancellationToken cancellationToken)
    {
        return service.GetExecutionPageAsync(context.Message, cancellationToken);
    }
}
