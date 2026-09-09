using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Message.Commands.Identity;

public class CreateApiKeyCommand : ICommand, IRequireTwoFactorAuthentication
{
    [Required]
    [MinLength(1)]
    public string ApiKey { get; set; } = string.Empty;
}

public class CreateApiKeyResponse : VoidBotResponse<string>
{
}
