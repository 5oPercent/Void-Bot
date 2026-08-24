using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Account;

namespace Void.Bot.Message.Commands.Account;

public sealed class SendEmailBindingCodeCommand : ICommand
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }
}

public sealed class SendEmailBindingCodeResponse : VoidBotResponse<EmailVerificationChallengeDto>
{
}
