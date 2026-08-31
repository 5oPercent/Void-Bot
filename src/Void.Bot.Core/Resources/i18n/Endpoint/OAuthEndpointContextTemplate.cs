using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "OAuth")]
public sealed class OAuthEndpointContextTemplate : IContextTemplate
{
    public string Authorize { get; set; } = "授权";
    public string Callback { get; set; } = "处理回调";
    public string Exchange { get; set; } = "交换 OAuth 授权码";
    public string Register { get; set; } = "注册 OAuth 应用";
    public string Bind { get; set; } = "绑定 OAuth 账户";
    public string Unlink { get; set; } = "解除 OAuth 账户绑定";

}
