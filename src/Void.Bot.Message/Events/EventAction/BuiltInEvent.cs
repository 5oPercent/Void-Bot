using System.Text.Json.Serialization;
using Mediator.Net.Contracts;
using Void.Bot.Message.Enum;

namespace Void.Bot.Message.Events.EventAction;

public interface IBuiltInEvent : IEvent
{
    Guid EventId { get; }

    string Name { get; }

    DateTimeOffset OccurredAt { get; }
}

public interface IUserBuiltInEvent : IBuiltInEvent
{
    Guid UserId { get; }
}

public static class BuiltInEventNames
{
    public const string AccountRegistered = "Account.Registered";

    public const string AccountPasswordForceReset = "Account.PasswordForceReset";

    public const string AccountThirdPartyForceUnlinked = "Account.ThirdPartyForceUnlinked";
}

public sealed record AccountRegisteredEvent(Guid UserId) : IUserBuiltInEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public string Name => BuiltInEventNames.AccountRegistered;

    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record AccountPasswordForceResetEvent(
    Guid UserId,
    [property: JsonIgnore] string Password) : IUserBuiltInEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public string Name => BuiltInEventNames.AccountPasswordForceReset;

    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;

    public override string ToString()
    {
        return $"{nameof(AccountPasswordForceResetEvent)} {{ UserId = {UserId}, Password = [REDACTED] }}";
    }
}

public sealed record AccountThirdPartyForceUnlinkedEvent(
    Guid UserId,
    AppPlatformEnum Platform) : IUserBuiltInEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public string Name => BuiltInEventNames.AccountThirdPartyForceUnlinked;

    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
