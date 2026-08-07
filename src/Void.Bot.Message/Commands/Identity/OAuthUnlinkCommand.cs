using System.Text.Json.Serialization;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Identity;
using Void.Bot.Message.Enum;

namespace Void.Bot.Message.Commands.Identity;

public sealed class OAuthUnlinkCommand : ICommand
{
    [JsonIgnore]
    public AppPlatformEnum Platform { get; set; }
}

public sealed class OAuthUnlinkResponse : VoidBotResponse, IThirdPartyOAuthResponse
{
}
