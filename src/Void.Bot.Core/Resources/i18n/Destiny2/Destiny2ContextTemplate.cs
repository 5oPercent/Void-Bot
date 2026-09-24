using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Destiny2;

[Language("Destiny2", "Destiny2")]
public class Destiny2ContextTemplate : IContextTemplate
{
    public string HunterField { get; set; } = "猎人";
    public string TitanField { get; set; } = "泰坦";
    public string WarlockField { get; set; } = "术士";
    public string CareerTitle { get; set; } = "{0} 的生涯信息";
    public string CareerField { get; set; } = "当前赛季";
    public string CareerBody { get; set; } = """
                                             > 嘉奖得分: `{0}`
                                             > 生涯守护者等级: `{1}`
                                             > 赛季守护者等级: `{2}`
                                             > 最后上线: `{3}`
                                             """;
    public string SeasonField { get; set; } = "历史生涯";
    public string SeasonBody { get; set; } = """
                                             > 名称: `{0}`
                                             > 时间: `{1}`
                                             > 天数: 剩余 `{2}` 天 | 总 `{3}` 天
                                             > 光等上限: `{4}`
                                             """;
    public string SeasonLatestTitle { get; set; } = "{0} 的赛季游玩时长";
    public string SeasonLatestTotalField { get; set; } = "总时长";
    public string SeasonNoData { get; set; } = "未查询到本赛季游玩数据";
    public string PlaytimeHourUnit { get; set; } = "小时";
}