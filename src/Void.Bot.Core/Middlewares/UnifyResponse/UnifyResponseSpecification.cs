using System.Net;
using System.Runtime.ExceptionServices;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Middlewares.UnifyResponse;

public class UnifyResponseSpecification<TContext> : IPipeSpecification<TContext> where TContext : IContext<IMessage>
{
    public bool ShouldExecute(TContext context, CancellationToken cancellationToken)
    {
        return true;
    }

    public Task BeforeExecute(TContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task Execute(TContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task AfterExecute(TContext context, CancellationToken cancellationToken)
    {
        if (!ShouldExecute(context, cancellationToken) || context.Result is not VoidBotResponse) return Task.CompletedTask;
        
        var response = (dynamic)context.Result;
        
        if (response.Code == 0) response.Code = HttpStatusCode.OK;
        if (string.IsNullOrEmpty(response.Message)) response.Message = nameof(HttpStatusCode.OK).ToLower();

        return Task.CompletedTask;
    }

    public async Task OnException(Exception ex, TContext context)
    {
        ExceptionDispatchInfo.Capture(ex).Throw();
    }
}