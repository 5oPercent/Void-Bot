using Void.Bot.Message.Dto.Bungie;
using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Message.Request.Bungie.Domain;

/// <summary>
/// GetProfile 接口返回结构
/// </summary>
public class BungieApiGetProfileFullResponse : BungieApiResponse
{
    public BungieApiGetProfileResponseDto Response { get; set; }
}

public class BungieApiGetProfileResponseDto
{
    /// <summary>
    /// 基本档案
    /// </summary>
    public BungieApiProfileComponentDto Profile { get; set; }
    
    /// <summary>
    /// 社交表扬
    /// </summary>
    public BungieApiSocialCommendationsComponentDto ProfileCommendations { get; set; }
}

#region [E-100]Profiles 基础档案 

public class BungieApiProfileComponentDto
{
    public BungieApiProfileComponentDataDto Data { get; set; }
    
    public BungieComponentPrivacySetting Privacy { get; set; }
}

public class BungieApiProfileComponentDataDto
{
    /// <summary>
    /// 用户信息
    /// </summary>
    public BungieApiProfileComponentUserInfoDto UserInfo { get; set; }
    
    /// <summary>
    /// 上次游玩时间
    /// </summary>
    public DateTime DateLastPlayed { get; set; }
    
    /// <summary>
    /// 拥有版本
    /// </summary>
    public int VersionsOwned { get; set; }
    
    /// <summary>
    /// 角色列表
    /// </summary>
    public List<string> CharacterIds { get; set; }
    
    /// <summary>
    /// 所有赛季Hash
    /// </summary>
    public List<long> SeasonHashes { get; set; }
    
    /// <summary>
    /// 季节活动卡片Hash
    /// </summary>
    public List<long> EventCardHashesOwned { get; set; }
    
    /// <summary>
    /// 当前赛季Hash
    /// </summary>
    public long CurrentSeasonHash { get; set; }
    
    /// <summary>
    /// 当前赛季能量上限
    /// </summary>
    public long CurrentSeasonRewardPowerCap { get; set; }
    
    /// <summary>
    /// 当前守护者等级
    /// </summary>
    public int CurrentGuardianRank { get; set; }
    
    /// <summary>
    /// 生涯最高守护者等级
    /// </summary>
    public int LifetimeHighestGuardianRank { get; set; }
    
    /// <summary>
    /// 当前赛季最新守护者等级
    /// </summary>
    public int RenewedGuardianRank { get; set; }
}

public class BungieApiProfileComponentUserInfoDto
{
    public BungieMembershipType CrossSaveOverride { get; set; }
    
    public List<BungieMembershipType> ApplicableMembershipTypes { get; set; }
    
    public bool IsPublic { get; set; }
    
    public BungieMembershipType MembershipType { get; set; }
    
    public string MembershipId { get; set; }
    
    public string DisplayName { get; set; }
    
    public string BungieGlobalDisplayName { get; set; }
    
    public int BungieGlobalDisplayNameCode { get; set; }
}

#endregion

#region [E-1400]SocialCommendations 社交表扬

public class BungieApiSocialCommendationsComponentDto
{
    public BungieApiSocialCommendationsComponentDataDto Data { get; set; }
}

public class BungieApiSocialCommendationsComponentDataDto
{
    public int TotalScore { get; set; }
    
    public Dictionary<long, long> CommendationNodePercentagesByHash { get; set; }
    
    public List<long> ScoreDetailValues { get; set; }
    
    public Dictionary<long, long> CommendationNodeScoresByHash { get; set; }
    
    public Dictionary<long, long> CommendationScoresByHash { get; set; }
    
    public BungieComponentPrivacySetting Privacy { get; set; }
}

#endregion