using System.Runtime.ExceptionServices;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Discord.Context;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Core.Middlewares.DiscordBinding;

public sealed class DiscordBindingSpecification<TContext> : IPipeSpecification<TContext>
    where TContext : IContext<IMessage>
{
    private readonly ICurrentUser _currentUser;
    private readonly IAccountDataProvider _accountDataProvider;
    private readonly IDiscordBindingContext _bindingContext;

    public DiscordBindingSpecification(
        ICurrentUser currentUser,
        IAccountDataProvider accountDataProvider,
        IDiscordBindingContext bindingContext)
    {
        _currentUser = currentUser;
        _accountDataProvider = accountDataProvider;
        _bindingContext = bindingContext;
    }

    public bool ShouldExecute(TContext context, CancellationToken cancellationToken)
    {
        return context.Message is IRequireDiscordBinding;
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

        var binding = await _accountDataProvider.GetThirdPartyByUserIdAsync(
            _currentUser.Id!.Value,
            nameof(AppPlatformEnum.Discord),
            cancellationToken).ConfigureAwait(false);
        if (binding == null || string.IsNullOrWhiteSpace(binding.AccessToken))
        {
            throw new DiscordBindingRequiredException();
        }

        _bindingContext.Binding = binding;
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
