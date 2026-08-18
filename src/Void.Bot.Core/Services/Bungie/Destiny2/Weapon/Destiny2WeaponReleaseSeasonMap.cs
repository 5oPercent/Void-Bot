namespace Void.Bot.Core.Services.Bungie.Destiny2.Weapon;

/// <summary>
/// 将 Bungie 清单中武器 traitIds 里的内容版本标记 (releases.vXXX.*) 映射到赛季号。
/// </summary>
/// <remarks>
/// <para>
/// 当前 Bungie 清单的 DestinyInventoryItemDefinition 不会为武器填充 seasonHash
/// (该字段仅存在于赛季任务道具等物品上), 因此武器的赛季只能通过其内容版本标记推断。
/// </para>
/// <para>
/// 语义说明: 映射结果表示武器 <b>当前内容版本</b> 所属赛季。复刻/回归武器
/// (如 Trials 复刻、Nightfall Adept、Into the Light 复刻、仍在掉落池的旧地牢武器)
/// 的 traitIds 会携带最新内容版本号, 因此显示的是复刻赛季而非原始登场赛季。
/// </para>
/// <para>
/// 版本号依据 Bungie 各资料片发布节奏划分:
/// v300~v320 = 年一(S1~S3), v400~v490 = 遗落之族(S4~S11),
/// v500~v540 = 凌光之刻(S12~S15), v600~v630 = 邪姬魅影(S16~S19),
/// v700~v730 = 光陨之秋(S20~S23), v800~v820 = 终焉之形(S24~S26),
/// v900~v970 = Frontiers 时代(S27~S28)。.annual/.season/.core/.dlc 后缀
/// 表示内容类型(年度/赛季/核心活动/DLC), 不影响版本所属赛季。
/// </para>
/// </remarks>
internal static class Destiny2WeaponReleaseSeasonMap
{
    /// <summary>
    /// 完整 trait id (如 releases.v450.season) 到赛季号的映射。
    /// </summary>
    public static IReadOnlyDictionary<string, int> Map { get; } = new Dictionary<string, int>
    {
        // 年一 (Year 1)
        ["releases.v300.annual"] = 1,
        ["releases.v310.season"] = 2,
        ["releases.v320.season"] = 3,

        // 遗落之族 (Forsaken)
        ["releases.v400.annual"] = 4,
        ["releases.v400.season"] = 4,
        ["releases.v410.season"] = 5,
        ["releases.v420.season"] = 6,
        ["releases.v450.season"] = 7,
        ["releases.v460.season"] = 8,
        ["releases.v470.season"] = 9,
        ["releases.v480.season"] = 10,
        ["releases.v490.season"] = 11,

        // 凌光之刻 (Beyond Light)
        ["releases.v500.annual"] = 12,
        ["releases.v500.season"] = 12,
        ["releases.v510.season"] = 13,
        ["releases.v520.season"] = 14,
        ["releases.v530.season"] = 15,
        ["releases.v540.season"] = 15,

        // 邪姬魅影 (The Witch Queen)
        ["releases.v600.annual"] = 16,
        ["releases.v600.season"] = 16,
        ["releases.v610.season"] = 17,
        ["releases.v620.season"] = 18,
        ["releases.v630.season"] = 19,

        // 光陨之秋 (Lightfall)
        ["releases.v700.annual"] = 20,
        ["releases.v700.season"] = 20,
        ["releases.v710.season"] = 21,
        ["releases.v720.season"] = 22,
        ["releases.v730.season"] = 23,

        // 终焉之形 (The Final Shape)
        ["releases.v800.annual"] = 24,
        ["releases.v800.season"] = 24,
        ["releases.v810.season"] = 25,
        ["releases.v820.season"] = 26,

        // Frontiers 时代
        ["releases.v900.core"] = 27,
        ["releases.v900.dlc"] = 27,
        ["releases.v910"] = 27,
        ["releases.v910.core"] = 27,
        ["releases.v950"] = 28,
        ["releases.v950.core"] = 28,
        ["releases.v950.dlc"] = 28,
        ["releases.v960.core"] = 28,
        ["releases.v970.core"] = 28
    };

    /// <summary>
    /// 从物品定义的 traitIds 中解析内容版本标记对应的赛季号。
    /// </summary>
    public static bool TryResolveSeason(string traitId, out int seasonNumber)
    {
        if (!string.IsNullOrWhiteSpace(traitId) &&
            traitId.StartsWith("releases.", StringComparison.Ordinal) &&
            Map.TryGetValue(traitId, out seasonNumber))
        {
            return true;
        }

        seasonNumber = 0;
        return false;
    }
}
