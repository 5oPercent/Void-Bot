using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Account;

public sealed class ResetUserAccountPasswordByLinkCommand : ICommand
{
    public required string Token { get; init; }
    public required string NewPassword { get; init; }
    public required string NewPasswordConfirm { get; init; }
}

public sealed class ResetUserAccountPasswordByLinkResponse : VoidBotResponse
{
}
