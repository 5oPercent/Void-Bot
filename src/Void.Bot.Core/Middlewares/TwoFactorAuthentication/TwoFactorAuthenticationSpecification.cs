using System.Runtime.ExceptionServices;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Core.Middlewares.TwoFactorAuthentication;

public sealed class TwoFactorAuthenticationSpecification<TContext>(
    ICurrentUser currentUser,
    IAccountDataProvider accountDataProvider) : IPipeSpecification<TContext>
    where TContext : IContext<IMessage>
{
    public bool ShouldExecute(TContext context, CancellationToken cancellationToken)
    {
        return context.Message is IRequireTwoFactorAuthentication;
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

        if (!currentUser.Id.HasValue || currentUser.Id == Guid.Empty)
        {
            throw new TwoFactorAuthenticationRequiredException();
        }

        var account = await accountDataProvider.GetUserAccountAsync(
            currentUser.Id, string.Empty, cancellationToken).ConfigureAwait(false);
        if (account is null ||
            (account.TwoFactorEnabledAt.HasValue && !currentUser.IsTwoFactorAuthenticated))
        {
            throw new TwoFactorAuthenticationRequiredException();
        }
    }

    public Task AfterExecute(TContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task OnException(Exception ex, TContext context)
    {
        ExceptionDispatchInfo.Capture(ex).Throw();
        return Task.CompletedTask;
    }
}
