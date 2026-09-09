using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Account;

public sealed class EnableTwoFactorCommand : ICommand
{
    public required string Code { get; init; }
}

public sealed class EnableTwoFactorResponse : VoidBotResponse
{
}
