using System.Runtime.ExceptionServices;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;

namespace Void.Bot.Core.Middlewares.UnitofWork;

public class UnitOfWorkSpecification<TContext>(IUnitOfWork unitOfWork, DbContext dbContext) : IPipeSpecification<TContext>
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
            if (dbContext.Database.CurrentTransaction == null)
            {
                await dbContext.Database.BeginTransactionAsync(cancellationToken);
            }
        }
    }

    public async Task AfterExecute(TContext context, CancellationToken cancellationToken)
    {
        if (ShouldExecute(context, cancellationToken))
        {
            try
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);

                if (dbContext.Database.CurrentTransaction != null)
                {
                    await dbContext.Database.CommitTransactionAsync(cancellationToken);
                }
            }
            catch
            {
                if (dbContext.Database.CurrentTransaction != null)
                {
                    await dbContext.Database.RollbackTransactionAsync(cancellationToken);
                }

                throw;
            }
        }
    }

    public async Task OnException(Exception ex, TContext context)
    {
        if (dbContext.Database.CurrentTransaction != null)
        {
            await dbContext.Database.RollbackTransactionAsync();
        }
        
        ExceptionDispatchInfo.Capture(ex).Throw();
    }
}