namespace Void.Bot.Message.Constants.Bungie;

public static class BungieConstants
{
    public static readonly string Domain = "https://www.bungie.net";
    
    public static readonly string AuthorizeUrl = Domain + "/zh-chs/OAuth/Authorize";
    
    public static readonly string GenerateTokenUrl = Domain + "/Platform/App/OAuth/token";
    
    public static readonly string RefreshTokenUrl = Domain + "/Platform/App/OAuth/token";
    
    public static readonly string Destiny2ProfileUrl = Domain + "/Platform/Destiny2/{membershipType}/Profile/{membershipId}";
    
    public static readonly string Destiny2LinkedProfiles = Destiny2ProfileUrl + "/LinkedProfiles";
}