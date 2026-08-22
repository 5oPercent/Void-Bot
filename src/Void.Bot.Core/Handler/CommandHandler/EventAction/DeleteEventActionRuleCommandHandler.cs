using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.EventAction;
using Void.Bot.Message.Commands.EventAction;

namespace Void.Bot.Core.Handler.CommandHandler.EventAction;

public class DeleteEventActionRuleCommandHandler(IEventActionRuleService service)
    : ICommandHandler<DeleteEventActionRuleCommand, DeleteEventActionRuleResponse>
{
    public Task<DeleteEventActionRuleResponse> Handle(
        IReceiveContext<DeleteEventActionRuleCommand> context,
        CancellationToken cancellationToken)
    {
        return service.DeleteRulesAsync(context.Message, cancellationToken);
    }
}
