namespace Void.Bot.Message.Constants.Bungie;

public partial class BungieConstants
{
    public const string Domain = "https://www.bungie.net";
    
    public const string AuthorizeUrl = Domain + "/zh-chs/OAuth/Authorize";
    
    public const string GenerateTokenUrl = Domain + "/Platform/App/OAuth/token";
    
    public const string RefreshTokenUrl = Domain + "/Platform/App/OAuth/token";
    
    public const string Destiny2ApiUrl = Domain + "/Platform/Destiny2";
    
    public const string Destiny2Manifest = Destiny2ApiUrl + "/Manifest";
    
    public const string Destiny2ProfileUrl = Destiny2ApiUrl + "/{membershipType}/Profile/{membershipId}";
    
    public const string Destiny2LinkedProfiles = Destiny2ProfileUrl + "/LinkedProfiles";
    
    public const string Destiny2SearchPlayerProfile = Destiny2ApiUrl + "/SearchDestinyPlayerByBungieName/{membershipType}/";
}