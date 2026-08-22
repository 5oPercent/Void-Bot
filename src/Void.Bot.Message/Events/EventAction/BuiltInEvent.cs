using Mediator.Net.Contracts;

namespace Void.Bot.Message.Events.EventAction;

public interface IBuiltInEvent : IEvent
{
    string Name { get; }
}

public static class BuiltInEventNames
{
    public const string AccountRegistered = "Account.Registered";
}

public sealed record AccountRegisteredEvent(Guid UserId) : IBuiltInEvent
{
    public string Name => BuiltInEventNames.AccountRegistered;
}
