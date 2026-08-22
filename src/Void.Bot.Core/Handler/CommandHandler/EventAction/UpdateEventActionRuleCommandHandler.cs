using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.EventAction;
using Void.Bot.Message.Commands.EventAction;

namespace Void.Bot.Core.Handler.CommandHandler.EventAction;

public class UpdateEventActionRuleCommandHandler(IEventActionRuleService service)
    : ICommandHandler<UpdateEventActionRuleCommand, UpdateEventActionRuleResponse>
{
    public Task<UpdateEventActionRuleResponse> Handle(
        IReceiveContext<UpdateEventActionRuleCommand> context,
        CancellationToken cancellationToken)
    {
        return service.UpdateRuleAsync(context.Message, cancellationToken);
    }
}
