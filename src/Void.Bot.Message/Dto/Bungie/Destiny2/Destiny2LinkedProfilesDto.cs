using System.Text.Json.Serialization;
using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Message.Dto.Bungie.Destiny2;

public class Destiny2LinkedProfilesResponse : BungieApiResponse
{
    public Destiny2LinkedProfilesDataDto Response { get; set; } = new();
}

public class Destiny2LinkedProfilesDataDto
{
    public List<Destiny2LinkedProfileItemDto> Profiles { get; set; } = [];

    [JsonPropertyName("bnetMembership")]
    public BungieApiLinkedProfileNetMembershipDto BungieNetMembershipDto { get; set; } = new();
}

/// <summary>
/// 游玩平台账号基本信息
/// </summary>
public class Destiny2LinkedProfileItemDto
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
    public BungieMembershipTypeEnum CrossSaveOverride { get; set; }

    /// <summary>
    /// 已知游玩平台列表
    /// </summary>
    public List<BungieMembershipTypeEnum> ApplicableMembershipTypes { get; set; } = [];
    
    public bool IsPublic { get; set; }
    
    public BungieMembershipTypeEnum MembershipTypeEnum { get; set; }

    public string MembershipId { get; set; } = string.Empty;
    
    public string DisplayName { get; set; } = string.Empty;
    
    public string BungieGlobalDisplayName { get; set; } = string.Empty;
    
    public int BungieGlobalDisplayNameCode { get; set; }
}