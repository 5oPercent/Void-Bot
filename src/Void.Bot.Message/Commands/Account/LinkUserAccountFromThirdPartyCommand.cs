using System.Text.Json.Serialization;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Authorization;

namespace Void.Bot.Message.Commands.Account;

public class LinkUserAccountFromThirdPartyCommand : ICommand
{
    public required AuthorizationDto Authorization { get; set; }

    public string MasterDisplayName { get; set; } = string.Empty;
    
    public string AccessToken { get; set; } = string.Empty;

    public DateTimeOffset? AccessTokenExpiresAt { get; set; }

    public string RefreshToken { get; set; } = string.Empty;
    
    public DateTimeOffset? RefreshTokenExpiresAt { get; set; }
    
    public string ExtraData { get; set; } = string.Empty;
}

public class LinkUserAccountFromThirdPartyResponse : VoidBotResponse
{
    /// <summary>
    /// 用于 Handler 判断是否需要发布账号注册事件.
    /// </summary>
    [JsonIgnore]
    public Guid? RegisteredAccountId { get; set; }
}
