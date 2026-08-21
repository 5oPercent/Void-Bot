using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Discord;

[Language("Discord", "Destiny2GodRollSlashCommand")]
public class Destiny2GodRollSlashCommandContextTemplate : IContextTemplate
{
    public string CommandDescription { get; set; } = "按照 Perk 条件筛选 God Roll 武器.";

    public string WeaponTypeOptionDescription { get; set; } = "选择要筛选的武器类型.";

    public string PerkOptionDescription { get; set; } = "第 {0} 组 Perk 名称或 Hash，多个值用逗号分隔.";

    public string PerkInputRequiredResponse { get; set; } = "请至少提供一个 Perk 名称或 Hash.";

    public string PerkNotFoundResponse { get; set; } = "未在所选武器类型中找到 Perk：`{0}`.";

    public string PerkNameAmbiguousResponse { get; set; } = "Perk 名称 `{0}` 对应多个结果，请改用 Hash：{1}.";

    public string NoMatchingWeaponsResponse { get; set; } = "没有找到同时满足这些 Perk 条件的武器.";

    public string MatchingWeaponsTruncatedResponse { get; set; } = "找到 {0} 把武器，显示前 {1} 把.";

    public string MatchingWeaponsResponse { get; set; } = "找到 {0} 把武器.";
}
