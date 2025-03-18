using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Identity;

public class DeleteApiKeyCommand : ICommand
{
    public string ApiKey { get; set; }
    
    public Guid? UserId { get; set; }
}

public class DeleteApiKeyResponse : VoidBotResponse
{
}