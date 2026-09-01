namespace Void.Bot.Message.Dto.Discord;

public sealed class DiscordManagedGuildDto
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? IconUrl { get; set; }

    public bool IsOwner { get; set; }

    public bool IsBotInstalled { get; set; }

    public bool CanManageGuild { get; set; }
}
