namespace Void.Bot.Message.Dto.Discord;

public sealed class DiscordRolePanelItemDto
{
    public Guid Id { get; set; }
    public string Emoji { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;
    public string Style { get; set; } = "Secondary";
    public string Mode { get; set; } = "Toggle";
    public int Sort { get; set; }
    public bool Enabled { get; set; }
}
