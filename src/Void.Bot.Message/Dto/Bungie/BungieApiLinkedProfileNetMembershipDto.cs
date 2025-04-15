using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Message.Dto.Bungie;

public class BungieApiLinkedProfileNetMembershipDto
{
    /// <summary>
    /// 完整名称, 例 PlayerName#1234
    /// </summary>
    public string SupplementalDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 注意: 平台图标路径, 例 /img/profile/avatars/avatar28.jpg
    /// </summary>
    public string IconPath { get; set; } = string.Empty;
    
    public BungieMembershipTypeEnum CrossSaveOverride { get; set; }
    
    public bool IsPublic { get; set; }
    
    public BungieMembershipTypeEnum MembershipTypeEnum { get; set; }
    
    public long MembershipId { get; set; }

    public string DisplayName { get; set; } = string.Empty;
    
    public string BungieGlobalDisplayName { get; set; } = string.Empty;
    
    public int BungieGlobalDisplayNameCode { get; set; }
}