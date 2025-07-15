using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Core.Services.Bungie;

public class Destiny2PlayerData : IScope
{
    /// <summary>
    /// BungieId
    /// </summary>
    public string BungieMembershipId { get; set; } = string.Empty;
    
    /// <summary>
    /// 全局玩家名称
    /// </summary>
    public string GlobalDisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// 全局玩家代码
    /// </summary>
    public string GlobalDisplayNameCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 唯一名称
    /// </summary>
    public string UniqueName => $"{GlobalDisplayName}#{GlobalDisplayNameCode}";
    
    /// <summary>
    /// 注册时间
    /// </summary>
    public DateTime FirstAccess { get; set; }
    
    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdate { get; set; }
    
    /// <summary>
    /// 展示活动
    /// </summary>
    public bool ShowActivity { get; set; }
    
    /// <summary>
    /// 语言
    /// </summary>
    public string Language { get; set; } = "zh-chs";
    
    /// <summary>
    /// 仅存在单个有个平台信息
    /// </summary>
    public bool IsSinglePlatform { get; set; }
    
    /// <summary>
    /// 启用跨平台存档
    /// </summary>
    public bool OnCrossSave { get; set; }
    
    /// <summary>
    /// 主要会员平台类型.
    /// <br/>具有以下情况:
    /// <br/>1. 启用跨平台，该值为主要平台类型.
    /// <br/>2. 未启用跨平台 + 单平台游玩，该值为单平台类型.
    /// <br/>3. 未启用跨平台 + 多平台游玩，该值为None.
    /// </summary>
    public BungieMembershipTypeEnum PrimaryMembershipType { get; set; } = BungieMembershipTypeEnum.None;
    
    /// <summary>
    /// 主要会员平台ID
    /// <br/>具有以下情况:
    /// <br/>1. 启用跨平台，该值为主要平台会员ID.
    /// <br/>2. 未启用跨平台 + 单平台游玩，该值为单平台会员ID.
    /// <br/>3. 未启用跨平台 + 多平台游玩，该值为None.
    /// </summary>
    public string PrimaryMembershipId { get; set; } = string.Empty;
    
    /// <summary>
    /// 所有平台会员信息
    /// </summary>
    public List<Destiny2PlayerDataMembershipItem> Profiles { get; set; } = [];
}

/// <summary>
/// 游玩平台会员信息
/// </summary>
public class Destiny2PlayerDataMembershipItem
{
    /// <summary>
    /// 平台会员类型
    /// </summary>
    public BungieMembershipTypeEnum MembershipType { get; set; }
    
    /// <summary>
    /// 会员平台ID
    /// </summary>
    public string MembershipId { get; set; } = string.Empty;
    
    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
}