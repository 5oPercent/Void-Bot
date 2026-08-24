using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Email;

public sealed class CompleteEmailAuthorizationCommand : ICommand
{
    public string? Code { get; init; }

    public string? State { get; init; }

    public string? Error { get; init; }
}

public sealed class CompleteEmailAuthorizationResponse : VoidBotResponse
{
}
