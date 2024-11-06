using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Authorization;

namespace Void.Bot.Message.Commands.Account;

public class LinkUserAccountFromThirdPartyCommand : ICommand
{
    public required AuthorizationDto Authorization { get; set; }
    
    public string? MasterDisplayName { get; set; }
    
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;
    
    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.Now;

    public string ExtraData { get; set; } = string.Empty;
}

public class LinkUserAccountFromThirdPartyResponse : VoidBotResponse
{
}