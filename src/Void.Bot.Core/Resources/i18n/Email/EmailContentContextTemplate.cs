using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Email;

[Language("Email", "Content")]
public sealed class EmailContentContextTemplate : IContextTemplate
{
    public string TeamSignature { get; set; } = "Void Bot 团队";
    public string AutomatedNotice { get; set; } = "此邮件由 Void Bot 自动发送，请勿直接回复。";
    public string VerificationSubject { get; set; } = "Void Bot 邮箱验证码";
    public string PasswordResetCodeSubject { get; set; } = "Void Bot 密码重置验证码";
    public string EmailUnbindingCodeSubject { get; set; } = "Void Bot 邮箱解绑验证码";
    public string VerificationIntroduction { get; set; } = "你的 Void Bot 验证码是：";
    public string VerificationExpirationNotice { get; set; } = "验证码将在 {0} 分钟后失效。";
    public string PasswordResetLinkSubject { get; set; } = "Void Bot 密码重置";
    public string PasswordResetLinkContent { get; set; } = "请打开以下链接重置密码：{0}\n此链接将于 {1:O} 失效。";
}
