using Void.Bot.Message.Enum.Bungie;
using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Dto.Bungie.Destiny2.Component;

public class Destiny2ComponentProfilesDto
{
    public DestinyComponentProfilesDataDto Data { get; set; } = new();
    
    public Destiny2ComponentPrivacyEnum Privacy { get; set; }
}

public class DestinyComponentProfilesDataDto
{
    /// <summary>
    /// 用户信息
    /// </summary>
    public DestinyComponentProfilesUserInfoDto UserInfo { get; set; } = new();
    
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
    public List<string> CharacterIds { get; set; } = [];

    /// <summary>
    /// 所有赛季Hash
    /// </summary>
    public List<long> SeasonHashes { get; set; } = [];

    /// <summary>
    /// 季节活动卡片Hash
    /// </summary>
    public List<long> EventCardHashesOwned { get; set; } = [];
    
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

public class DestinyComponentProfilesUserInfoDto
{
    public BungieMembershipTypeEnum CrossSaveOverride { get; set; }

    public List<BungieMembershipTypeEnum> ApplicableMembershipTypes { get; set; } = [];
    
    public bool IsPublic { get; set; }
    
    public BungieMembershipTypeEnum MembershipTypeEnum { get; set; }
    
    public string MembershipId { get; set; } = string.Empty;
    
    public string DisplayName { get; set; } = string.Empty;
    
    public string BungieGlobalDisplayName { get; set; } = string.Empty;
    
    public int BungieGlobalDisplayNameCode { get; set; }
}