using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Email;
using Void.Bot.Message.Dto.Email;
using Void.Bot.Message.Dto.EventAction;
using Void.Bot.Message.Events.EventAction;

namespace Void.Bot.Core.Services.EventAction;

public sealed class SendEmailEventActionExecutor(
    IAccountDataProvider accountDataProvider,
    IEmailService emailService) : IConfiguredEventActionExecutor
{
    public const string ActionType = "SendEmail";

    public string Type => ActionType;

    public int Version => 1;

    public EventActionDefinitionDto Definition => new()
    {
        Type = ActionType,
        Version = Version,
        DisplayName = "发送 Email",
        Description = "向事件关联用户的已验证邮箱发送纯文本邮件。",
        SupportedEvents =
        [
            BuiltInEventNames.AccountRegistered,
            BuiltInEventNames.AccountPasswordForceReset,
            BuiltInEventNames.AccountThirdPartyForceUnlinked
        ],
        Arguments =
        [
            new EventActionArgumentDefinitionDto
            {
                Name = "Subject",
                DisplayName = "邮件主题",
                Description = "必填，最多 200 个字符；支持 {UserId}、{DisplayName}、{Platform} 占位符。",
                ValueType = "String",
                Required = true,
                Multiple = false,
                MinLength = 1,
                MaxLength = 1,
                ValueSource = "Text"
            },
            new EventActionArgumentDefinitionDto
            {
                Name = "Content",
                DisplayName = "邮件内容",
                Description = "纯文本内容，最多 10000 个字符；支持 {UserId}、{DisplayName}、{Platform}、{Password} 占位符。",
                ValueType = "String",
                Required = true,
                Multiple = false,
                MinLength = 1,
                MaxLength = 1,
                ValueSource = "Textarea"
            }
        ]
    };

    public Task<string?> ValidateAsync(
        string eventName,
        IReadOnlyDictionary<string, string[]> arguments,
        CancellationToken cancellationToken)
    {
        if (!Definition.SupportedEvents.Contains(eventName, StringComparer.OrdinalIgnoreCase))
        {
            return Task.FromResult<string?>(
                $"Event action '{ActionType}' does not support event '{eventName}'.");
        }

        var (_, _, error) = ResolveArguments(arguments);
        return Task.FromResult(error);
    }

    public async Task ExecuteAsync(
        IBuiltInEvent builtInEvent,
        IReadOnlyDictionary<string, string[]> arguments,
        CancellationToken cancellationToken)
    {
        if (builtInEvent is not IUserBuiltInEvent userEvent ||
            !Definition.SupportedEvents.Contains(builtInEvent.Name, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Event action '{ActionType}' does not support event '{builtInEvent.Name}'.");
        }

        var (subject, content, error) = ResolveArguments(arguments);
        if (!string.IsNullOrEmpty(error))
        {
            throw new InvalidOperationException(error);
        }

        var account = await accountDataProvider.GetUserAccountAsync(
            userEvent.UserId, string.Empty, cancellationToken).ConfigureAwait(false);
        if (account is null)
        {
            throw new InvalidOperationException(
                $"Event action '{ActionType}' could not find account '{userEvent.UserId}'.");
        }

        if (string.IsNullOrWhiteSpace(account.Email) || !account.EmailVerifiedAt.HasValue)
        {
            throw new InvalidOperationException(
                $"Event action '{ActionType}' requires the event user to have a verified email address.");
        }

        var platform = builtInEvent is AccountThirdPartyForceUnlinkedEvent thirdPartyForceUnlinked
            ? thirdPartyForceUnlinked.Platform.ToString()
            : string.Empty;
        var password = builtInEvent is AccountPasswordForceResetEvent passwordForceReset
            ? passwordForceReset.Password
            : string.Empty;
        subject = ReplacePlaceholders(subject!, account.Id, account.DisplayName, platform, string.Empty);
        subject = subject.Replace('\r', ' ').Replace('\n', ' ');
        content = ReplacePlaceholders(content!, account.Id, account.DisplayName, platform, password);
        if (subject.Length > 200 || content.Length > 10000)
        {
            throw new InvalidOperationException(
                $"Event action '{ActionType}' email exceeds the allowed length after replacing placeholders.");
        }

        await emailService.SendAsync(
            new EmailMessageDto
            {
                To = account.Email,
                Subject = subject,
                TextContent = content
            },
            cancellationToken).ConfigureAwait(false);
    }

    private static (string? Subject, string? Content, string? Error) ResolveArguments(
        IReadOnlyDictionary<string, string[]> arguments)
    {
        var unsupportedArguments = arguments.Keys
            .Where(x => !string.Equals(x, "Subject", StringComparison.Ordinal) &&
                        !string.Equals(x, "Content", StringComparison.Ordinal))
            .ToList();
        if (unsupportedArguments.Count != 0)
        {
            return (null, null,
                $"Event action '{ActionType}' contains unsupported arguments: {string.Join(", ", unsupportedArguments)}.");
        }

        arguments.TryGetValue("Subject", out var subjects);
        var subject = subjects is { Length: 1 } ? subjects[0] : null;
        if (subjects is not { Length: 1 } || string.IsNullOrWhiteSpace(subject))
        {
            return (null, null, $"Event action '{ActionType}' requires exactly one Subject.");
        }

        if (subject.Length > 200)
        {
            return (null, null, $"Event action '{ActionType}' Subject cannot exceed 200 characters.");
        }

        if (subject.Contains("{Password}", StringComparison.OrdinalIgnoreCase))
        {
            return (null, null,
                $"Event action '{ActionType}' Subject cannot contain the Password placeholder.");
        }

        arguments.TryGetValue("Content", out var contents);
        var content = contents is { Length: 1 } ? contents[0] : null;
        if (contents is not { Length: 1 } || string.IsNullOrWhiteSpace(content))
        {
            return (null, null, $"Event action '{ActionType}' requires exactly one Content.");
        }

        if (content.Length > 10000)
        {
            return (null, null, $"Event action '{ActionType}' Content cannot exceed 10000 characters.");
        }

        return (subject.Trim(), content, null);
    }

    private static string ReplacePlaceholders(
        string template,
        Guid userId,
        string displayName,
        string platform,
        string password)
    {
        return template
            .Replace("{UserId}", userId.ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("{DisplayName}", displayName, StringComparison.OrdinalIgnoreCase)
            .Replace("{Platform}", platform, StringComparison.OrdinalIgnoreCase)
            .Replace("{Password}", password, StringComparison.OrdinalIgnoreCase);
    }
}
