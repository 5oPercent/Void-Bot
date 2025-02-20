using Void.Bot.Message.Dto.Bungie;
using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Message.Request.Bungie.Domain;

public class BungieApiSearchDestinyPlayerByBungieNameRequest
{
    public string DisplayName { get; set; }
    
    public int DisplayNameCode { get; set; }
}

public class BungieApiSearchDestinyPlayerByBungieNameResponse : BungieApiResponse
{
    public List<BungieApiSearchDestinyPlayerByBungieNameDto> Response { get; set; }
}

public class BungieApiSearchDestinyPlayerByBungieNameDto
{
    public string IconPath { get; set; } = string.Empty;
    
    public BungieMembershipType CrossSaveOverride { get; set; }
    
    public List<BungieMembershipType> ApplicableMembershipTypes { get; set; }
    
    public bool IsPublic { get; set; }
    
    public BungieMembershipType MembershipType { get; set; }
    
    public string MembershipId { get; set; } = string.Empty;
    
    public string DisplayName { get; set; } = string.Empty;
    
    public string BungieGlobalDisplayName { get; set; } = string.Empty;
    
    public int BungieGlobalDisplayNameCode { get; set; }
}