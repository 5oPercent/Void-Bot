namespace Void.Bot.Message.Constants.Bungie.Destiny2;

public partial class Destiny2Constants
{
    public const string BaseUrl = BungieConstants.BungieUrl + "/Platform/Destiny2";
    
    public const string ManifestUrl = BaseUrl + "/Manifest";
    
    public const string ProfileUrl = BaseUrl + "/{membershipType}/Profile/{membershipId}";
    
    public const string LinkedProfilesUrl = ProfileUrl + "/LinkedProfiles";
    
    public const string SearchPlayerProfileUrl = BaseUrl + "/SearchDestinyPlayerByBungieName/{membershipType}/";
}