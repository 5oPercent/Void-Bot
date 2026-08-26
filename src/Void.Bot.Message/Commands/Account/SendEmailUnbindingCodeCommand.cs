using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Account;

namespace Void.Bot.Message.Commands.Account;

public sealed class SendEmailUnbindingCodeCommand : ICommand
{
}

public sealed class SendEmailUnbindingCodeResponse : VoidBotResponse<EmailVerificationChallengeDto>
{
}
