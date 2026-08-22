using Mediator.Net.Contracts;

namespace Void.Bot.Message.Events.EventAction;

public interface IBuiltInEvent : IEvent
{
    Guid EventId { get; }

    string Name { get; }

    DateTimeOffset OccurredAt { get; }
}

public static class BuiltInEventNames
{
    public const string AccountRegistered = "Account.Registered";
}

public sealed record AccountRegisteredEvent(Guid UserId) : IBuiltInEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public string Name => BuiltInEventNames.AccountRegistered;

    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
