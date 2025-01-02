using System.Text.Json.Serialization;
using Void.Bot.Message.Dto.Bungie;
using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Message.Request.Bungie.Domain;

public class BungieApiLinkedProfilesResponse : BungieApiResponse
{
    public BungieApiLinkedProfilesDataDto Response { get; set; }
}

public class BungieApiLinkedProfilesDataDto
{
    public List<BungieApiLinkedProfileItemDto> Profiles { get; set; }
    
    [JsonPropertyName("bnetMembership")]
    public BungieApiLinkedProfileNetMembershipDto BungieNetMembershipDto { get; set; }
}

/// <summary>
/// 游玩平台账号基本信息
/// </summary>
public class BungieApiLinkedProfileItemDto
{
    public DateTime DateLastPlayed { get; set; }
    
    /// <summary>
    /// 是否已覆盖当前游戏平台数据
    /// </summary>
    public bool IsOverridden { get; set; }
    
    /// <summary>
    /// 是否当前档案为主要跨平台数据
    /// </summary>
    public bool IsCrossSavePrimary { get; set; }
    
    /// <summary>
    /// 主要跨平台数据游戏平台
    /// </summary>
    public BungieMembershipType CrossSaveOverride { get; set; }
    
    /// <summary>
    /// 已知游玩平台列表
    /// </summary>
    public List<BungieMembershipType> ApplicableMembershipTypes { get; set; }
    
    public bool IsPublic { get; set; }
    
    public BungieMembershipType MembershipType { get; set; }

    public string MembershipId { get; set; } = string.Empty;
    
    public string DisplayName { get; set; } = string.Empty;
    
    public string BungieGlobalDisplayName { get; set; } = string.Empty;
    
    public int BungieGlobalDisplayNameCode { get; set; }
}

/// <summary>
/// BungieNet 会员信息
/// </summary>
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
    
    public BungieMembershipType CrossSaveOverride { get; set; }
    
    public bool IsPublic { get; set; }
    
    public BungieMembershipType MembershipType { get; set; }
    
    public long MembershipId { get; set; }

    public string DisplayName { get; set; } = string.Empty;
    
    public string BungieGlobalDisplayName { get; set; } = string.Empty;
    
    public int BungieGlobalDisplayNameCode { get; set; }
}