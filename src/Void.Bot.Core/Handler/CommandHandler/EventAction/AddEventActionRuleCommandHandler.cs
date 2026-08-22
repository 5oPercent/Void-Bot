using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.EventAction;
using Void.Bot.Message.Commands.EventAction;

namespace Void.Bot.Core.Handler.CommandHandler.EventAction;

public class AddEventActionRuleCommandHandler(IEventActionRuleService service)
    : ICommandHandler<AddEventActionRuleCommand, AddEventActionRuleResponse>
{
    public Task<AddEventActionRuleResponse> Handle(
        IReceiveContext<AddEventActionRuleCommand> context,
        CancellationToken cancellationToken)
    {
        return service.AddRuleAsync(context.Message, cancellationToken);
    }
}
