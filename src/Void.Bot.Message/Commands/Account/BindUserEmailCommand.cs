using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Account;

namespace Void.Bot.Message.Commands.Account;

public sealed class BindUserEmailCommand : ICommand
{
    public required EmailVerificationDto EmailVerification { get; init; }
}

public sealed class BindUserEmailResponse : VoidBotResponse<UserEmailStatusDto>
{
}
