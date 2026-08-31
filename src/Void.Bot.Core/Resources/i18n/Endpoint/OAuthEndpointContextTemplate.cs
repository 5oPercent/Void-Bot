using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "OAuth")]
public sealed class OAuthEndpointContextTemplate : IContextTemplate
{
    public string AuthorizeAsync { get; set; } = "授权";
    public string CallbackAsync { get; set; } = "处理回调";
    public string ExchangeAsync { get; set; } = "交换 OAuth 授权码";
    public string RegisterAsync { get; set; } = "注册 OAuth 应用";
    public string BindAsync { get; set; } = "绑定 OAuth 账户";
    public string UnlinkAsync { get; set; } = "解除 OAuth 账户绑定";

}
