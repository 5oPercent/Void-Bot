using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Discord;

[Language("Discord", "Command")]
public sealed class DiscordCommandContextTemplate : IContextTemplate
{
    public string LinkCredentialsRequired { get; set; } = "请填写账号名称和密码。";
    public string AccountNotFound { get; set; } = "账号不存在。";
    public string VerificationFailed { get; set; } = "验证失败。";
    public string StatusUpdated { get; set; } = "更改状态完成！";
    public string PingResponse { get; set; } = "Pong!";
}
