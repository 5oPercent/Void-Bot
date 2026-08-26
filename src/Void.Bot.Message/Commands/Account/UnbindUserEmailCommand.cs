using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Account;

namespace Void.Bot.Message.Commands.Account;

public sealed class UnbindUserEmailCommand : ICommand
{
    public Guid ChallengeId { get; init; }

    public required string Code { get; init; }
}

public sealed class UnbindUserEmailResponse : VoidBotResponse<UserEmailStatusDto>
{
}
