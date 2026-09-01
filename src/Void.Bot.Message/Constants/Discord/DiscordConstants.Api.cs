namespace Void.Bot.Message.Constants.Discord;

public partial class DiscordConstants
{
    public const string DiscordUrl = "https://discord.com";

    public const string ApiUrl = DiscordUrl + "/api/v10";

    public const string AuthorizeUrl = DiscordUrl + "/oauth2/authorize";

    public const string GenerateTokenUrl = ApiUrl + "/oauth2/token";

    public const string RefreshTokenUrl = GenerateTokenUrl;

    public const string CurrentUserUrl = ApiUrl + "/users/@me";

    public const string CurrentUserGuildsUrl = ApiUrl + "/users/@me/guilds";
}
