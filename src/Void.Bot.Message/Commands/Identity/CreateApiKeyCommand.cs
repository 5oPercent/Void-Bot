using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Identity;

public class CreateApiKeyCommand : ICommand
{
    [Required]
    [MinLength(1)]
    public string ApiKey { get; set; } = string.Empty;
}

public class CreateApiKeyResponse : VoidBotResponse<string>
{
}