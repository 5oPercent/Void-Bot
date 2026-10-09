using System.Runtime.ExceptionServices;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using System.Reflection;
using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Middlewares.UnitofWork;

public class UnitOfWorkSpecification<TContext>(
    IUnitOfWork unitOfWork,
    DbContext dbContext,
    IUnitOfWorkExecutionContext executionContext) : IPipeSpecification<TContext>
    where TContext : IContext<IMessage>
{
    public bool ShouldExecute(TContext context, CancellationToken cancellationToken)
    {
        return context.Message is ICommand or IEvent;
    }

    public Task BeforeExecute(TContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task Execute(TContext context, CancellationToken cancellationToken)
    {
        if (ShouldExecute(context, cancellationToken))
        {
            var frame = executionContext.Enter(
                context, dbContext.Database.CurrentTransaction != null);
            if (frame.OwnsTransaction)
            {
                try
                {
                    var transactionIsolation = context.Message.GetType()
                        .GetCustomAttribute<TransactionIsolationAttribute>();
                    if (transactionIsolation != null)
                    {
                        await dbContext.Database.BeginTransactionAsync(transactionIsolation.IsolationLevel,
                            cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
                catch
                {
                    executionContext.TryExit(context, out _);
                    throw;
                }
            }
        }
    }

    public async Task AfterExecute(TContext context, CancellationToken cancellationToken)
    {
        if (ShouldExecute(context, cancellationToken))
        {
            if (!executionContext.TryExit(context, out var frame) || !frame.IsOutermost)
            {
                return;
            }

            try
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);

                if (frame.OwnsTransaction && dbContext.Database.CurrentTransaction != null)
                {
                    await dbContext.Database.CommitTransactionAsync(cancellationToken);
                }

                foreach (var callback in executionContext.TakeAfterCommitCallbacks())
                {
                    await callback(cancellationToken);
                }
            }
            catch
            {
                executionContext.ClearAfterCommitCallbacks();
                if (frame.OwnsTransaction && dbContext.Database.CurrentTransaction != null)
                {
                    await dbContext.Database.RollbackTransactionAsync(cancellationToken);
                }

                throw;
            }
        }
    }

    public async Task OnException(Exception ex, TContext context)
    {
        var isOutermost = executionContext.TryExit(context, out var frame) && frame.IsOutermost;
        if (isOutermost)
        {
            executionContext.ClearAfterCommitCallbacks();
            if (frame.OwnsTransaction && dbContext.Database.CurrentTransaction != null)
            {
                await dbContext.Database.RollbackTransactionAsync();
            }
        }

        ExceptionDispatchInfo.Capture(ex).Throw();
    }
}
