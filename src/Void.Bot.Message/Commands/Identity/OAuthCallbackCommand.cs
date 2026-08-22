using System.Text.Json.Serialization;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Identity;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Identity;

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
    [JsonIgnore]
    public required OAuthStateFlow Flow { get; init; }

    [JsonIgnore]
    public string AccessToken { get; set; } = string.Empty;

    [JsonIgnore]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonIgnore]
    public string ExchangeCode { get; set; } = string.Empty;

    public string ProviderUserId { get; init; } = string.Empty;

    public string ProviderUserName { get; init; } = string.Empty;
}

public sealed class OAuthCallbackResponse
    : VoidBotResponse<OAuthCallbackData>, IThirdPartyOAuthResponse
{
}
