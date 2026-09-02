namespace Void.Bot.Message.Dto.Discord;

public sealed class DiscordRolePanelDto
{
    public Guid Id { get; set; }
    public string GuildId { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public string? MessageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public bool Published => !string.IsNullOrWhiteSpace(MessageId);
    public List<DiscordRolePanelItemDto> Items { get; set; } = [];
}
