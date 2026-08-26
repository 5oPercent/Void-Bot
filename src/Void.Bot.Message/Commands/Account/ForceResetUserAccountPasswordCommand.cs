using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Account;

public sealed class ForceResetUserAccountPasswordCommand : ICommand
{
    public Guid UserId { get; init; }

    public required string NewPassword { get; init; }

    public required string NewPasswordConfirm { get; init; }
}

public sealed class ForceResetUserAccountPasswordResponse : VoidBotResponse
{
}
