using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Authorization;

namespace Void.Bot.Message.Commands.Account;

public class LinkUserAccountFromThirdPartyCommand : ICommand
{
    public AuthorizationDto Authorization { get; set; }
    
    public string? MasterDisplayName { get; set; }
}

public class LinkUserAccountFromThirdPartyResponse : VoidBotResponse
{
}