using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Email;

[Language("Email", "Layout")]
public sealed class EmailLayoutContextTemplate : IContextTemplate
{
    public string TeamSignature { get; set; } = "Void Bot 团队";
    public string AutomatedNotice { get; set; } = "此邮件由 Void Bot 自动发送，请勿直接回复。";
}
