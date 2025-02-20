namespace Void.Bot.Message.Constants.Bungie;

public partial class BungieConstants
{
    public const string Domain = "https://www.bungie.net";
    
    public const string AuthorizeUrl = Domain + "/zh-chs/OAuth/Authorize";
    
    public const string GenerateTokenUrl = Domain + "/Platform/App/OAuth/token";
    
    public const string RefreshTokenUrl = Domain + "/Platform/App/OAuth/token";
    
    public const string Destiny2ProfileUrl = Domain + "/Platform/Destiny2/{membershipType}/Profile/{membershipId}";
    
    public const string Destiny2LinkedProfiles = Destiny2ProfileUrl + "/LinkedProfiles";
    
    public const string Destiny2SearchPlayerProfile = Domain + "/platform/Destiny2/SearchDestinyPlayerByBungieName/{membershipType}/";
}