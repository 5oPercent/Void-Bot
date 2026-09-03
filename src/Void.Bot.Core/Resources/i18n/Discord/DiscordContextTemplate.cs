using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Discord;

[Language("Discord", "Common")]
public sealed class DiscordContextTemplate : IContextTemplate
{
    public string ComponentExpired { get; set; } = "该操作已失效。";
    public string ComponentFailed { get; set; } = "操作失败，请稍后重试。";
    public string UnknownCommand { get; set; } = "未知命令！";
    public string CommandFailed { get; set; } = "指令执行失败，请稍后重试。";
    public string GuildOnlyCommand { get; set; } = "该指令只能在服务器内使用。";
}
