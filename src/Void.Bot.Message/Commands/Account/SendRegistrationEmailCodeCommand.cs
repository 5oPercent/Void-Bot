using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Account;

namespace Void.Bot.Message.Commands.Account;

public sealed class SendRegistrationEmailCodeCommand : ICommand
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }
}

public sealed class SendRegistrationEmailCodeResponse : VoidBotResponse<EmailVerificationChallengeDto>
{
}
