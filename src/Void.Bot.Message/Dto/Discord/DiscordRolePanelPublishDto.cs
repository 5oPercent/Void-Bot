namespace Void.Bot.Message.Dto.Discord;

public sealed class DiscordRolePanelPublishDto
{
    public string GuildId { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
    public bool Created { get; set; }
}
