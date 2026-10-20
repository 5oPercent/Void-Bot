using System.Runtime.ExceptionServices;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Middlewares.ERA;

public class EraPlayerSpecification<TContext> : IPipeSpecification<TContext>
    where TContext : IContext<IMessage>
{
    private readonly IEraPlayerContext _playerContext;

    public EraPlayerSpecification(IEraPlayerContext playerContext)
    {
        _playerContext = playerContext;
    }

    public bool ShouldExecute(TContext context, CancellationToken cancellationToken)
    {
        return context.Message is IRequireEraPlayer or IRequireEraTargetPlayer;
    }

    public Task BeforeExecute(TContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task Execute(TContext context, CancellationToken cancellationToken)
    {
        if (!ShouldExecute(context, cancellationToken))
        {
            return;
        }

        var forUpdate = context.Message is ICommand;
        if (context.Message is IRequireEraPlayer)
        {
            await _playerContext.RequireCurrentAsync(forUpdate, cancellationToken).ConfigureAwait(false);
        }

        if (context.Message is IRequireEraTargetPlayer target)
        {
            await _playerContext.RequireTargetAsync(target.PlayerId, forUpdate, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public Task AfterExecute(TContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task OnException(Exception exception, TContext context)
    {
        ExceptionDispatchInfo.Capture(exception).Throw();
        return Task.CompletedTask;
    }
}
