using System.Runtime.ExceptionServices;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Serilog;
using Void.Bot.Core.Extension;

namespace Void.Bot.Core.Middlewares.Logger;

public class LoggerSpecification<TContext>(ILogger logger) : IPipeSpecification<TContext>
    where TContext : IContext<IMessage>
{
    public bool ShouldExecute(TContext context, CancellationToken cancellationToken)
    {
        return true;
    }

    public Task BeforeExecute(TContext context, CancellationToken cancellationToken)
    {
        logger.Information(
            "----- Handling message {MessageName}: {@Message}",
            context.Message.GetGenericTypeName(),
            LogRedactor.Redact(context.Message));
        
        return Task.CompletedTask;
    }

    public Task Execute(TContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task AfterExecute(TContext context, CancellationToken cancellationToken)
    {
        logger.Information(
            "----- Message {MessageName} handled response: {@Response}",
            context.Message.GetGenericTypeName(),
            LogRedactor.Redact(context.Result));
        
        return Task.CompletedTask;
    }

    public async Task OnException(Exception ex, TContext context)
    {
        if (LogRedactor.ContainsSensitiveValues(context.Message))
        {
            logger.Error(
                "----- Error handling OAuth message {MessageName}: exception {ExceptionType}",
                context.Message.GetGenericTypeName(),
                ex.GetType().Name);
        }
        else
        {
            logger.Error(
                ex,
                "----- Error handling message {MessageName}",
                context.Message.GetGenericTypeName());
        }
        
        ExceptionDispatchInfo.Capture(ex).Throw();
    }
}
