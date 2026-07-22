using Void.Bot.Message.Dto.Discord.OAuth;

namespace Void.Bot.Message.Request.Discord.OAuth;

public sealed record AuthenticateDiscordOAuthRequest(string Code);

public abstract record AuthenticateDiscordOAuthResponse
{
    public sealed record Succeeded(DiscordOAuthIdentity Identity) : AuthenticateDiscordOAuthResponse;
    public sealed record Failed : AuthenticateDiscordOAuthResponse;
}
