namespace Void.Bot.Message.Dto.Bungie;

public class BungieNetUserDto
{
    public string MembershipId { get; set; } = string.Empty;

    public string UniqueName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public int ProfilePicture { get; set; }

    public int ProfileTheme { get; set; }

    public int UserTitle { get; set; }

    public string SuccessMessageFlags { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public string About { get; set; } = string.Empty;

    public DateTime FirstAccess { get; set; }

    public DateTime LastUpdate { get; set; }

    public bool ShowActivity { get; set; }

    public string Locale { get; set; } = string.Empty;

    public bool LocaleInheritDefault { get; set; }

    public bool ShowGroupMessaging { get; set; }

    public string ProfilePicturePath { get; set; } = string.Empty;

    public string ProfileThemeName { get; set; } = string.Empty;

    public string UserTitleDisplay { get; set; } = string.Empty;

    public string StatusText { get; set; } = string.Empty;

    public DateTime StatusDate { get; set; }

    public string CachedBungieGlobalDisplayName { get; set; } = string.Empty;
    
    public int CachedBungieGlobalDisplayNameCode { get; set; }
    
    public string SteamDisplayName { get; set; } = string.Empty;

    public string EgsDisplayName { get; set; } = string.Empty;
}