using System.Reflection;
using System.Runtime.ExceptionServices;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Newtonsoft.Json;
using Void.Bot.Core.Services.MessageLogging;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Commands.MessageLogging;

namespace Void.Bot.Core.Middlewares.MessageLogger;

public class MessageLoggingSpecification<TContext>(IMessageLoggingService messageLoggingService) : IPipeSpecification<TContext>
    where TContext : IContext<IMessage>
{
    public bool ShouldExecute(TContext context, CancellationToken cancellationToken)
    {
        if (context.Result == null) return false;
        
        return context.Message.GetType().GetCustomAttribute<MessageLoggingAttribute>() != null;
    }

    public Task BeforeExecute(TContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task Execute(TContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task AfterExecute(TContext context, CancellationToken cancellationToken)
    {
        if (!ShouldExecute(context, cancellationToken)) return;
        
        await messageLoggingService.AddLogsAsync(new List<AddMessageLoggingCommand>
        {
            new()
            {
                MessageType = context.Message.GetType().ToString(),
                ResultType = context.Result.GetType().ToString(),
                MessageJson = JsonConvert.SerializeObject(context.Message),
                ResultJson = JsonConvert.SerializeObject(context.Result)
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    public Task OnException(Exception ex, TContext context)
    {
        ExceptionDispatchInfo.Capture(ex).Throw();
        throw ex;
    }
}