using System.Text.Json.Serialization;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Identity;
using Void.Bot.Message.Enum;

namespace Void.Bot.Message.Commands.Identity;

public sealed class OAuthCallbackCommand : ICommand
{
    [JsonIgnore]
    public AppPlatformEnum Platform { get; set; }

    [JsonIgnore]
    public string? Code { get; init; }

    [JsonIgnore]
    public string State { get; init; } = string.Empty;

    [JsonIgnore]
    public string? Error { get; init; }
}

public sealed class OAuthCallbackData : ThirdPartyOAuthData
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public string ProviderUserId { get; init; } = string.Empty;

    public string ProviderUserName { get; init; } = string.Empty;
}

public sealed class OAuthCallbackResponse
    : VoidBotResponse<OAuthCallbackData>, IThirdPartyOAuthResponse
{
}
