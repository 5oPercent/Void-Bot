using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Email;

[Language("Email", "BuiltIn")]
public sealed class BuiltInEmailContextTemplate : IContextTemplate
{
    public string VerificationSubject { get; set; } = "Void Bot 邮箱验证码";
    public string PasswordResetCodeSubject { get; set; } = "Void Bot 密码重置验证码";
    public string EmailUnbindingCodeSubject { get; set; } = "Void Bot 邮箱解绑验证码";
    public string VerificationIntroduction { get; set; } = "你的 Void Bot 验证码是：";
    public string VerificationExpirationNotice { get; set; } = "验证码将在 {0} 分钟后失效。";
    public string PasswordResetLinkSubject { get; set; } = "Void Bot 密码重置";
    public string PasswordResetLinkContent { get; set; } = "请打开以下链接重置密码：{0}\n此链接将于 {1:O} 失效。";
    public string AccountRegisteredActionName { get; set; } = "发送账号注册成功邮件";
    public string AccountRegisteredActionDescription { get; set; } = "向注册成功的账号发送内置欢迎邮件。";
    public string AccountRegisteredSubject { get; set; } = "欢迎使用 Void Bot";
    public string AccountRegisteredContent { get; set; } = "你好，{0}：\n你的 Void Bot 账号已注册成功。";
    public string PasswordForceResetActionName { get; set; } = "发送密码强制重置邮件";
    public string PasswordForceResetActionDescription { get; set; } = "向密码被强制重置的账号发送内置通知邮件。";
    public string PasswordForceResetSubject { get; set; } = "Void Bot 密码已被重置";
    public string PasswordForceResetContent { get; set; } = "你好，{0}：\n管理员已强制重置你的 Void Bot 密码。\n临时密码：{1}\n请尽快登录并修改密码。";
    public string ThirdPartyForceUnlinkedActionName { get; set; } = "发送第三方授权强制解绑邮件";
    public string ThirdPartyForceUnlinkedActionDescription { get; set; } = "向第三方授权被强制解绑的账号发送内置通知邮件。";
    public string ThirdPartyForceUnlinkedSubject { get; set; } = "Void Bot 第三方授权已被解绑";
    public string ThirdPartyForceUnlinkedContent { get; set; } = "你好，{0}：\n管理员已强制解绑你的 {1} 授权。\n如非预期，请联系管理员。";
}
