using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Discord;

[Language("Discord", "Component")]
public sealed class DiscordComponentContextTemplate : IContextTemplate
{
    public string ComponentExpired { get; set; } = "该操作已失效。";
    public string ComponentFailed { get; set; } = "操作失败，请稍后重试。";
    public string PaginationExpired { get; set; } = "分页结果已过期，请重新操作。";
    public string PaginationFailed { get; set; } = "翻页失败，请稍后重试。";
}
