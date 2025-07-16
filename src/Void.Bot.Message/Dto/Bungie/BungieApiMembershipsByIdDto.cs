using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Message.Dto.Bungie;

public class BungieApiMembershipsByIdRequest
{
    public BungieMembershipTypeEnum MembershipType { get; set; }
    
    public string MembershipId { get; set; } = string.Empty;
}

public class BungieApiMembershipsByIdResponse : BungieApiResponse
{
    public BungieApiMembershipsByIdData? Response { get; set; }
}

public class BungieApiMembershipsByIdData
{
    public List<BungieApiDestinyMembershipDto> DestinyMemberships { get; set; } = [];

    public string PrimaryMembershipId { get; set; } = string.Empty;

    public BungieNetUserDto? BungieNetUser { get; set; }
}

public class BungieApiDestinyMembershipDto
{
    public string LastSeenDisplayName { get; set; } = string.Empty;

    public BungieMembershipTypeEnum LastSeenDisplayNameType { get; set; }
    
    public string IconPath { get; set; } = string.Empty;
    
    public BungieMembershipTypeEnum CrossSaveOverride { get; set; }
    
    public List<BungieMembershipTypeEnum> ApplicableMembershipTypes { get; set; } = [];
    
    public bool IsPublic { get; set; }
    
    public BungieMembershipTypeEnum MembershipType { get; set; }
    
    public string MembershipId { get; set; } = string.Empty;
    
    public string DisplayName { get; set; } = string.Empty;
    
    public string BungieGlobalDisplayName { get; set; } = string.Empty;
    
    public int BungieGlobalDisplayNameCode { get; set; }
}