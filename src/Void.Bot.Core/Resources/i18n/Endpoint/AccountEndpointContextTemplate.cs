using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "Account")]
public sealed class AccountEndpointContextTemplate : IContextTemplate
{
    public string SendUserAccountPasswordResetLinkAsync { get; set; } = "发送用户账户密码重置链接";
    public string ForceResetPasswordAsync { get; set; } = "强制重置密码";
    public string ForceUnlinkThirdPartyAsync { get; set; } = "强制解除第三方账户绑定";

}
