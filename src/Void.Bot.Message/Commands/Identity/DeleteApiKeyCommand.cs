using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Message.Commands.Identity;

public class DeleteApiKeyCommand : ICommand, IRequireTwoFactorAuthentication
{
    [Required]
    [MinLength(1)]
    public required string ApiKey { get; set; }
    
    public Guid? UserId { get; set; }
}

public class DeleteApiKeyResponse : VoidBotResponse
{
}
