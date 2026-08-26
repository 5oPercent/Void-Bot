using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Serilog;
using Void.Bot.Core.Data;
using Void.Bot.Core.Middlewares.UnitofWork;
using Void.Bot.Core.Services.EventAction;
using Void.Bot.Message.Events.EventAction;

namespace Void.Bot.Core.Handler.EventHandler.EventAction;

public sealed class AccountPasswordForceResetEventHandler(
    IBuiltInEventDispatcher dispatcher,
    IUnitOfWorkExecutionContext executionContext,
    VoidBotDbContext dbContext,
    ILogger logger)
    : IEventHandler<AccountPasswordForceResetEvent>
{
    public Task Handle(
        IReceiveContext<AccountPasswordForceResetEvent> context,
        CancellationToken cancellationToken)
    {
        var builtInEvent = context.Message;
        executionContext.EnqueueAfterCommit(async afterCommitCancellationToken =>
        {
            try
            {
                await dispatcher.DispatchAsync(
                    builtInEvent, afterCommitCancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                dbContext.ChangeTracker.Clear();
                logger.Error(
                    exception,
                    "Failed to execute configured actions for event {EventName}",
                    builtInEvent.Name);
            }
        });

        return Task.CompletedTask;
    }
}
