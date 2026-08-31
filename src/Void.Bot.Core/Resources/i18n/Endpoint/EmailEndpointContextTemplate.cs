using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "Email")]
public sealed class EmailEndpointContextTemplate : IContextTemplate
{
    public string AuthorizeAsync { get; set; } = "授权";
    public string CallbackAsync { get; set; } = "处理回调";
    public string ReauthorizeAsync { get; set; } = "重新授权";
    public string GetStatusAsync { get; set; } = "获取状态";
    public string SendTestEmailAsync { get; set; } = "发送测试邮件";

}
