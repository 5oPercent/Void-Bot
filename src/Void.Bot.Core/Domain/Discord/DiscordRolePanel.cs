using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.Discord;

[Table("discord_role_panel")]
public sealed class DiscordRolePanel : IEntity<Guid>, IEntityCreated, IEntityModified
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public ulong GuildId { get; set; }

    public ulong ChannelId { get; set; }

    public ulong? MessageId { get; set; }

    [StringLength(256)]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000)]
    public string Content { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public List<DiscordRolePanelItem> Items { get; set; } = [];
}

[Table("discord_role_panel_item")]
public sealed class DiscordRolePanelItem : IEntity<Guid>, IEntityCreated, IEntityModified
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PanelId { get; set; }

    public DiscordRolePanel Panel { get; set; } = null!;

    public ulong RoleId { get; set; }

    [StringLength(64)]
    public string Emoji { get; set; } = string.Empty;

    [StringLength(80)]
    public string Label { get; set; } = string.Empty;

    [StringLength(16)]
    public string Style { get; set; } = "Secondary";

    [StringLength(16)]
    public string Mode { get; set; } = "Toggle";

    public int Sort { get; set; }

    public bool Enabled { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
