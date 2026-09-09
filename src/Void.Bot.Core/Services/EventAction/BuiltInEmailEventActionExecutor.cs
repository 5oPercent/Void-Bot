using Void.Bot.Core.Resources.i18n.Email;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Email;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Dto.Email;
using Void.Bot.Message.Dto.EventAction;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Events.EventAction;

namespace Void.Bot.Core.Services.EventAction;

public abstract class BuiltInEmailEventActionExecutor(
    IAccountDataProvider accountDataProvider,
    IEmailService emailService) : IConfiguredEventActionExecutor
{
    public abstract string Type { get; }

    public int Version => 1;

    protected abstract string SupportedEvent { get; }

    public abstract EventActionDefinitionDto Definition { get; }

    public Task<string?> ValidateAsync(
        string eventName,
        IReadOnlyDictionary<string, string[]> arguments,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(eventName, SupportedEvent, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<string?>(
                $"Event action '{Type}' does not support event '{eventName}'.");
        }

        return Task.FromResult<string?>(arguments.Count == 0
            ? null
            : $"Event action '{Type}' does not accept arguments.");
    }

    public async Task ExecuteAsync(
        IBuiltInEvent builtInEvent,
        IReadOnlyDictionary<string, string[]> arguments,
        CancellationToken cancellationToken)
    {
        var validationMessage = await ValidateAsync(
            builtInEvent.Name, arguments, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(validationMessage) || builtInEvent is not IUserBuiltInEvent userEvent)
        {
            throw new InvalidOperationException(
                validationMessage ?? $"Event action '{Type}' requires a user event.");
        }

        var account = await accountDataProvider.GetUserAccountAsync(
            userEvent.UserId, string.Empty, cancellationToken).ConfigureAwait(false);
        if (account is null)
        {
            throw new InvalidOperationException(
                $"Event action '{Type}' could not find account '{userEvent.UserId}'.");
        }

        if (string.IsNullOrWhiteSpace(account.Email) || !account.EmailVerifiedAt.HasValue)
        {
            throw new InvalidOperationException(
                $"Event action '{Type}' requires the event user to have a verified email address.");
        }

        await emailService.SendAsync(
            CreateMessage(builtInEvent, account), cancellationToken).ConfigureAwait(false);
    }

    protected abstract EmailMessageDto CreateMessage(
        IBuiltInEvent builtInEvent,
        UserAccountDto account);
}

public sealed class SendAccountRegisteredEmailEventActionExecutor(
    IAccountDataProvider accountDataProvider,
    IEmailService emailService,
    ILocalizationService localizationService)
    : BuiltInEmailEventActionExecutor(accountDataProvider, emailService)
{
    public const string ActionType = "SendAccountRegisteredEmail";

    public override string Type => ActionType;

    protected override string SupportedEvent => BuiltInEventNames.AccountRegistered;

    public override EventActionDefinitionDto Definition => new()
    {
        Type = Type,
        Version = Version,
        DisplayName = localizationService.GetValue<BuiltInEmailContextTemplate>(
            x => x.AccountRegisteredActionName),
        Description = localizationService.GetValue<BuiltInEmailContextTemplate>(
            x => x.AccountRegisteredActionDescription),
        SupportedEvents = [SupportedEvent]
    };

    protected override EmailMessageDto CreateMessage(
        IBuiltInEvent builtInEvent,
        UserAccountDto account)
    {
        if (builtInEvent is not AccountRegisteredEvent)
        {
            throw new InvalidOperationException(
                $"Event action '{Type}' does not support event '{builtInEvent.Name}'.");
        }

        return new EmailMessageDto
        {
            To = account.Email!,
            Subject = localizationService.GetValue<BuiltInEmailContextTemplate>(
                x => x.AccountRegisteredSubject),
            TextContent = localizationService.GetValue<BuiltInEmailContextTemplate>(
                x => x.AccountRegisteredContent,
                LanguageEnum.Default,
                account.DisplayName)
        };
    }
}

public sealed class SendPasswordForceResetEmailEventActionExecutor(
    IAccountDataProvider accountDataProvider,
    IEmailService emailService,
    ILocalizationService localizationService)
    : BuiltInEmailEventActionExecutor(accountDataProvider, emailService)
{
    public const string ActionType = "SendPasswordForceResetEmail";

    public override string Type => ActionType;

    protected override string SupportedEvent => BuiltInEventNames.AccountPasswordForceReset;

    public override EventActionDefinitionDto Definition => new()
    {
        Type = Type,
        Version = Version,
        DisplayName = localizationService.GetValue<BuiltInEmailContextTemplate>(
            x => x.PasswordForceResetActionName),
        Description = localizationService.GetValue<BuiltInEmailContextTemplate>(
            x => x.PasswordForceResetActionDescription),
        SupportedEvents = [SupportedEvent]
    };

    protected override EmailMessageDto CreateMessage(
        IBuiltInEvent builtInEvent,
        UserAccountDto account)
    {
        if (builtInEvent is not AccountPasswordForceResetEvent passwordForceReset)
        {
            throw new InvalidOperationException(
                $"Event action '{Type}' does not support event '{builtInEvent.Name}'.");
        }

        return new EmailMessageDto
        {
            To = account.Email!,
            Subject = localizationService.GetValue<BuiltInEmailContextTemplate>(
                x => x.PasswordForceResetSubject),
            TextContent = localizationService.GetValue<BuiltInEmailContextTemplate>(
                x => x.PasswordForceResetContent,
                LanguageEnum.Default,
                account.DisplayName,
                passwordForceReset.Password)
        };
    }
}

public sealed class SendThirdPartyForceUnlinkedEmailEventActionExecutor(
    IAccountDataProvider accountDataProvider,
    IEmailService emailService,
    ILocalizationService localizationService)
    : BuiltInEmailEventActionExecutor(accountDataProvider, emailService)
{
    public const string ActionType = "SendThirdPartyForceUnlinkedEmail";

    public override string Type => ActionType;

    protected override string SupportedEvent => BuiltInEventNames.AccountThirdPartyForceUnlinked;

    public override EventActionDefinitionDto Definition => new()
    {
        Type = Type,
        Version = Version,
        DisplayName = localizationService.GetValue<BuiltInEmailContextTemplate>(
            x => x.ThirdPartyForceUnlinkedActionName),
        Description = localizationService.GetValue<BuiltInEmailContextTemplate>(
            x => x.ThirdPartyForceUnlinkedActionDescription),
        SupportedEvents = [SupportedEvent]
    };

    protected override EmailMessageDto CreateMessage(
        IBuiltInEvent builtInEvent,
        UserAccountDto account)
    {
        if (builtInEvent is not AccountThirdPartyForceUnlinkedEvent thirdPartyForceUnlinked)
        {
            throw new InvalidOperationException(
                $"Event action '{Type}' does not support event '{builtInEvent.Name}'.");
        }

        return new EmailMessageDto
        {
            To = account.Email!,
            Subject = localizationService.GetValue<BuiltInEmailContextTemplate>(
                x => x.ThirdPartyForceUnlinkedSubject),
            TextContent = localizationService.GetValue<BuiltInEmailContextTemplate>(
                x => x.ThirdPartyForceUnlinkedContent,
                LanguageEnum.Default,
                account.DisplayName,
                thirdPartyForceUnlinked.Platform)
        };
    }
}
