using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "EventAction")]
public sealed class EventActionEndpointContextTemplate : IContextTemplate
{
    public string GetDefinitionsAsync { get; set; } = "获取事件动作定义";
    public string GetRulesAsync { get; set; } = "获取事件动作规则";
    public string GetExecutionsAsync { get; set; } = "获取事件动作执行记录";
    public string AddRuleAsync { get; set; } = "新增事件动作规则";
    public string UpdateRuleAsync { get; set; } = "更新事件动作规则";
    public string DeleteRulesAsync { get; set; } = "删除事件动作规则";

}
