using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny;

/// <summary>
/// 命运活动模式枚举
/// </summary>
public enum DestinyActivityModeTypeEnum
{
    /// <summary>
    /// 无活动
    /// </summary>
    [Description("无活动")]
    None = 0,

    /// <summary>
    /// 故事
    /// </summary>
    [Description("故事")]
    Story = 2,

    /// <summary>
    /// 打击
    /// </summary>
    [Description("打击")]
    Strike = 3,

    /// <summary>
    /// 突袭
    /// </summary>
    [Description("突袭")]
    Raid = 4,

    /// <summary>
    /// 所有 PvP
    /// </summary>
    [Description("所有 PvP")]
    AllPvP = 5,

    /// <summary>
    /// 巡逻
    /// </summary>
    [Description("巡逻")]
    Patrol = 6,

    /// <summary>
    /// 所有 PvE
    /// </summary>
    [Description("所有 PvE")]
    AllPvE = 7,

    /// <summary>
    /// 保留 9
    /// </summary>
    [Description("保留 9")]
    Reserved9 = 9,

    /// <summary>
    /// 控制
    /// </summary>
    [Description("控制")]
    Control = 10,

    /// <summary>
    /// 保留 11
    /// </summary>
    [Description("保留 11")]
    Reserved11 = 11,

    /// <summary>
    /// 冲突
    /// </summary>
    [Description("冲突")]
    Clash = 12,

    /// <summary>
    /// 保留 13
    /// </summary>
    [Description("保留 13")]
    Reserved13 = 13,

    /// <summary>
    /// 深红双打
    /// </summary>
    [Description("深红双打")]
    CrimsonDoubles = 15,

    /// <summary>
    /// 夜幕
    /// </summary>
    [Description("夜幕")]
    Nightfall = 16,

    /// <summary>
    /// 英雄夜幕
    /// </summary>
    [Description("英雄夜幕")]
    HeroicNightfall = 17,

    /// <summary>
    /// 所有打击
    /// </summary>
    [Description("所有打击")]
    AllStrikes = 18,

    /// <summary>
    /// 铁旗
    /// </summary>
    [Description("铁旗")]
    IronBanner = 19,

    /// <summary>
    /// 保留 20
    /// </summary>
    [Description("保留 20")]
    Reserved20 = 20,

    /// <summary>
    /// 保留 21
    /// </summary>
    [Description("保留 21")]
    Reserved21 = 21,

    /// <summary>
    /// 保留 22
    /// </summary>
    [Description("保留 22")]
    Reserved22 = 22,

    /// <summary>
    /// 保留 24
    /// </summary>
    [Description("保留 24")]
    Reserved24 = 24,

    /// <summary>
    /// 所有混战
    /// </summary>
    [Description("所有混战")]
    AllMayhem = 25,

    /// <summary>
    /// 保留 26
    /// </summary>
    [Description("保留 26")]
    Reserved26 = 26,

    /// <summary>
    /// 保留 27
    /// </summary>
    [Description("保留 27")]
    Reserved27 = 27,

    /// <summary>
    /// 保留 28
    /// </summary>
    [Description("保留 28")]
    Reserved28 = 28,

    /// <summary>
    /// 保留 29
    /// </summary>
    [Description("保留 29")]
    Reserved29 = 29,

    /// <summary>
    /// 保留 30
    /// </summary>
    [Description("保留 30")]
    Reserved30 = 30,

    /// <summary>
    /// 至高无上
    /// </summary>
    [Description("至高无上")]
    Supremacy = 31,

    /// <summary>
    /// 所有私人比赛
    /// </summary>
    [Description("所有私人比赛")]
    PrivateMatchesAll = 32,

    /// <summary>
    /// 生存
    /// </summary>
    [Description("生存")]
    Survival = 37,

    /// <summary>
    /// 倒计时
    /// </summary>
    [Description("倒计时")]
    Countdown = 38,

    /// <summary>
    /// 玖的试炼
    /// </summary>
    [Description("玖的试炼")]
    TrialsOfTheNine = 39,

    /// <summary>
    /// 社交
    /// </summary>
    [Description("社交")]
    Social = 40,

    /// <summary>
    /// 试炼倒计时
    /// </summary>
    [Description("试炼倒计时")]
    TrialsCountdown = 41,

    /// <summary>
    /// 试炼生存
    /// </summary>
    [Description("试炼生存")]
    TrialsSurvival = 42,

    /// <summary>
    /// 铁旗控制
    /// </summary>
    [Description("铁旗控制")]
    IronBannerControl = 43,

    /// <summary>
    /// 铁旗冲突
    /// </summary>
    [Description("铁旗冲突")]
    IronBannerClash = 44,

    /// <summary>
    /// 铁旗至高无上
    /// </summary>
    [Description("铁旗至高无上")]
    IronBannerSupremacy = 45,

    /// <summary>
    /// 计分夜幕
    /// </summary>
    [Description("计分夜幕")]
    ScoredNightfall = 46,

    /// <summary>
    /// 计分英雄夜幕
    /// </summary>
    [Description("计分英雄夜幕")]
    ScoredHeroicNightfall = 47,

    /// <summary>
    /// 混战
    /// </summary>
    [Description("混战")]
    Rumble = 48,

    /// <summary>
    /// 所有双打
    /// </summary>
    [Description("所有双打")]
    AllDoubles = 49,

    /// <summary>
    /// 双打
    /// </summary>
    [Description("双打")]
    Doubles = 50,

    /// <summary>
    /// 私人比赛冲突
    /// </summary>
    [Description("私人比赛冲突")]
    PrivateMatchesClash = 51,

    /// <summary>
    /// 私人比赛控制
    /// </summary>
    [Description("私人比赛控制")]
    PrivateMatchesControl = 52,

    /// <summary>
    /// 私人比赛至高无上
    /// </summary>
    [Description("私人比赛至高无上")]
    PrivateMatchesSupremacy = 53,

    /// <summary>
    /// 私人比赛倒计时
    /// </summary>
    [Description("私人比赛倒计时")]
    PrivateMatchesCountdown = 54,

    /// <summary>
    /// 私人比赛生存
    /// </summary>
    [Description("私人比赛生存")]
    PrivateMatchesSurvival = 55,

    /// <summary>
    /// 私人比赛混战
    /// </summary>
    [Description("私人比赛混战")]
    PrivateMatchesMayhem = 56,

    /// <summary>
    /// 私人比赛混战
    /// </summary>
    [Description("私人比赛混战")]
    PrivateMatchesRumble = 57,

    /// <summary>
    /// 英雄冒险
    /// </summary>
    [Description("英雄冒险")]
    HeroicAdventure = 58,

    /// <summary>
    /// 对决
    /// </summary>
    [Description("对决")]
    Showdown = 59,

    /// <summary>
    /// 封锁
    /// </summary>
    [Description("封锁")]
    Lockdown = 60,

    /// <summary>
    /// 灼烧
    /// </summary>
    [Description("灼烧")]
    Scorched = 61,

    /// <summary>
    /// 团队灼烧
    /// </summary>
    [Description("团队灼烧")]
    ScorchedTeam = 62,

    /// <summary>
    /// 智谋
    /// </summary>
    [Description("智谋")]
    Gambit = 63,

    /// <summary>
    /// 所有 PvE 竞争
    /// </summary>
    [Description("所有 PvE 竞争")]
    AllPvECompetitive = 64,

    /// <summary>
    /// 突破
    /// </summary>
    [Description("突破")]
    Breakthrough = 65,

    /// <summary>
    /// 黑色军械库运行
    /// </summary>
    [Description("黑色军械库运行")]
    BlackArmoryRun = 66,

    /// <summary>
    /// 打捞
    /// </summary>
    [Description("打捞")]
    Salvage = 67,

    /// <summary>
    /// 铁旗打捞
    /// </summary>
    [Description("铁旗打捞")]
    IronBannerSalvage = 68,

    /// <summary>
    /// PvP 竞争
    /// </summary>
    [Description("PvP 竞争")]
    PvPCompetitive = 69,

    /// <summary>
    /// PvP 快速游戏
    /// </summary>
    [Description("PvP 快速游戏")]
    PvPQuickplay = 70,

    /// <summary>
    /// 冲突快速游戏
    /// </summary>
    [Description("冲突快速游戏")]
    ClashQuickplay = 71,

    /// <summary>
    /// 冲突竞争
    /// </summary>
    [Description("冲突竞争")]
    ClashCompetitive = 72,

    /// <summary>
    /// 控制快速游戏
    /// </summary>
    [Description("控制快速游戏")]
    ControlQuickplay = 73,

    /// <summary>
    /// 控制竞争
    /// </summary>
    [Description("控制竞争")]
    ControlCompetitive = 74,

    /// <summary>
    /// 智谋Prime
    /// </summary>
    [Description("智谋Prime")]
    GambitPrime = 75,

    /// <summary>
    /// 清算
    /// </summary>
    [Description("清算")]
    Reckoning = 76,

    /// <summary>
    /// 大熔炉
    /// </summary>
    [Description("大熔炉")]
    Menagerie = 77,

    /// <summary>
    /// Vex入侵
    /// </summary>
    [Description("Vex入侵")]
    VexOffensive = 78,

    /// <summary>
    /// 噩梦狩猎
    /// </summary>
    [Description("噩梦狩猎")]
    NightmareHunt = 79,

    /// <summary>
    /// 淘汰
    /// </summary>
    [Description("淘汰")]
    Elimination = 80,

    /// <summary>
    /// 动量控制
    /// </summary>
    [Description("动量控制")]
    Momentum = 81,

    /// <summary>
    /// 地牢
    /// </summary>
    [Description("地牢")]
    Dungeon = 82,

    /// <summary>
    /// 日晷
    /// </summary>
    [Description("日晷")]
    Sundial = 83,

    /// <summary>
    /// 奥西里斯试炼
    /// </summary>
    [Description("奥西里斯试炼")]
    TrialsOfOsiris = 84,

    /// <summary>
    /// 挑战
    /// </summary>
    [Description("挑战")]
    Dares = 85,

    /// <summary>
    /// 猛攻
    /// </summary>
    [Description("猛攻")]
    Offensive = 86,

    /// <summary>
    /// 遗失区域
    /// </summary>
    [Description("遗失区域")]
    LostSector = 87,

    /// <summary>
    /// 裂隙
    /// </summary>
    [Description("裂隙")]
    Rift = 88,

    /// <summary>
    /// 区域控制
    /// </summary>
    [Description("区域控制")]
    ZoneControl = 89,

    /// <summary>
    /// 铁旗裂隙
    /// </summary>
    [Description("铁旗裂隙")]
    IronBannerRift = 90,

    /// <summary>
    /// 铁旗区域控制
    /// </summary>
    [Description("铁旗区域控制")]
    IronBannerZoneControl = 91,

    /// <summary>
    /// 圣遗物
    /// </summary>
    [Description("圣遗物")]
    Relic = 92,
}