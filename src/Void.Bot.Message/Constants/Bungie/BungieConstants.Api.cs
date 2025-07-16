namespace Void.Bot.Message.Constants.Bungie;

public partial class BungieConstants
{
    public const string BungieUrl = "https://www.bungie.net";
    
    public const string AuthorizeUrl = BungieUrl + "/zh-chs/OAuth/Authorize";
    
    public const string GenerateTokenUrl = BungieUrl + "/Platform/App/OAuth/token";
    
    public const string RefreshTokenUrl = BungieUrl + "/Platform/App/OAuth/token";

    public const string NetUserUrl = BungieUrl + "/platform/User/GetBungieNetUserById/{id}";
    
    public const string MembershipsUrl = BungieUrl + "/Platform/User/GetMembershipsById/{membershipId}/{membershipType}";
}