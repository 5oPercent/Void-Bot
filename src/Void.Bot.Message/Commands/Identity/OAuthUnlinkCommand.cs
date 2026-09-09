using System.Text.Json.Serialization;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Identity;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Message.Commands.Identity;

public sealed class OAuthUnlinkCommand : ICommand, IRequireTwoFactorAuthentication
{
    [JsonIgnore]
    public AppPlatformEnum Platform { get; set; }
}

public sealed class OAuthUnlinkResponse : VoidBotResponse, IThirdPartyOAuthResponse
{
}
