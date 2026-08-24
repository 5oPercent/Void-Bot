using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Account;

public sealed class ResetUserAccountPasswordCommand : ICommand
{
    public required string Email { get; init; }

    public required Guid ChallengeId { get; init; }

    public required string Code { get; init; }

    public required string NewPassword { get; init; }

    public required string NewPasswordConfirm { get; init; }
}

public sealed class ResetUserAccountPasswordResponse : VoidBotResponse
{
}
