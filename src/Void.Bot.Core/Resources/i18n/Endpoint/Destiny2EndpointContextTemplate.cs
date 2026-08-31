using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "Destiny2")]
public sealed class Destiny2EndpointContextTemplate : IContextTemplate
{
    public string GetPlayerBaseProfileByBungieName { get; set; } = "按 Bungie 名称获取玩家基础档案";
    public string GetPlayerProfileByBungieName { get; set; } = "按 Bungie 名称获取玩家档案";
    public string GetPlayerCareer { get; set; } = "获取玩家生涯";
    public string GetPlayerRaidDungeonStats { get; set; } = "获取玩家突袭地牢统计";
    public string GetPlayerSeason { get; set; } = "获取玩家赛季信息";
    public string GetWeaponTypes { get; set; } = "获取武器类型";
    public string GetWeaponPerks { get; set; } = "获取武器特性";
    public string FilterWeaponGodRoll { get; set; } = "筛选武器 God Roll";
    public string RebuildWeaponCache { get; set; } = "重建武器缓存";

}
