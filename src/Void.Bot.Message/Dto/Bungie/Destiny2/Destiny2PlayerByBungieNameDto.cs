using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Message.Dto.Bungie.Destiny2;

public class Destiny2PlayerByBungieNameRequest
{
    public string DisplayName { get; set; }
    
    public int DisplayNameCode { get; set; }
}

public class Destiny2PlayerByBungieNameResponse : BungieApiResponse
{
    public List<Destiny2PlayerByBungieNameDto> Response { get; set; }
}

public class Destiny2PlayerByBungieNameDto
{
    public string IconPath { get; set; } = string.Empty;
    
    public BungieMembershipTypeEnum CrossSaveOverride { get; set; }
    
    public List<BungieMembershipTypeEnum> ApplicableMembershipTypes { get; set; }
    
    public bool IsPublic { get; set; }
    
    public BungieMembershipTypeEnum MembershipTypeEnum { get; set; }
    
    public string MembershipId { get; set; } = string.Empty;
    
    public string DisplayName { get; set; } = string.Empty;
    
    public string BungieGlobalDisplayName { get; set; } = string.Empty;
    
    public int BungieGlobalDisplayNameCode { get; set; }
}