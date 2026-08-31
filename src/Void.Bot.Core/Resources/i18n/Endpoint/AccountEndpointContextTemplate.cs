using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "Account")]
public sealed class AccountEndpointContextTemplate : IContextTemplate
{
    public string SendUserAccountPasswordResetLink { get; set; } = "发送用户账户密码重置链接";
    public string ForceResetPassword { get; set; } = "强制重置密码";
    public string ForceUnlinkThirdParty { get; set; } = "强制解除第三方账户绑定";

}
