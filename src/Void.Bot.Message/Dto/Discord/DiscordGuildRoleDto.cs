namespace Void.Bot.Message.Dto.Discord;

public sealed class DiscordGuildRoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#000000";
    public int Position { get; set; }
    public bool Managed { get; set; }
    public bool Assignable { get; set; }
    public string? UnassignableReason { get; set; }
}
