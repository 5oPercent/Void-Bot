using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "Email")]
public sealed class EmailEndpointContextTemplate : IContextTemplate
{
    public string Authorize { get; set; } = "授权";
    public string Callback { get; set; } = "处理回调";
    public string Reauthorize { get; set; } = "重新授权";
    public string GetStatus { get; set; } = "获取状态";
    public string SendTestEmail { get; set; } = "发送测试邮件";

}
