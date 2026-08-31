using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "EventAction")]
public sealed class EventActionEndpointContextTemplate : IContextTemplate
{
    public string GetDefinitions { get; set; } = "获取事件动作定义";
    public string GetRules { get; set; } = "获取事件动作规则";
    public string GetExecutions { get; set; } = "获取事件动作执行记录";
    public string AddRule { get; set; } = "新增事件动作规则";
    public string UpdateRule { get; set; } = "更新事件动作规则";
    public string DeleteRules { get; set; } = "删除事件动作规则";

}
