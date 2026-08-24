using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Email;

public sealed class SendTestEmailCommand : ICommand
{
    [Required]
    [EmailAddress]
    public string To { get; init; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Subject { get; init; } = "Void Bot email test";

    [Required]
    [StringLength(10000)]
    public string Content { get; init; } = "Void Bot email service is configured correctly.";
}

public sealed class SendTestEmailResponse : VoidBotResponse
{
}
