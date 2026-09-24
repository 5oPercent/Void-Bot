using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Message.Commands.Account;

public sealed class DisableTwoFactorCommand : ICommand, IRequireTwoFactorAuthentication
{
    public required string Password { get; init; }

    public required string Code { get; init; }
}

public sealed class DisableTwoFactorResponse : VoidBotResponse
{
}
