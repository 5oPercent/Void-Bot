using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.EventAction;
using Void.Bot.Message.Request.EventAction;

namespace Void.Bot.Core.Handler.RequestHandler.EventAction;

public class GetEventActionRuleListRequestHandler(IEventActionRuleService service)
    : IRequestHandler<GetEventActionRuleListRequest, GetEventActionRuleListResponse>
{
    public Task<GetEventActionRuleListResponse> Handle(
        IReceiveContext<GetEventActionRuleListRequest> context,
        CancellationToken cancellationToken)
    {
        return service.GetRuleListAsync(context.Message, cancellationToken);
    }
}
