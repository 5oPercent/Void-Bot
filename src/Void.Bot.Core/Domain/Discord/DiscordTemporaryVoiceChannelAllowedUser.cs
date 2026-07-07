using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.Discord;

[Table("discord_temporary_voice_channel_allowed_user")]
public class DiscordTemporaryVoiceChannelAllowedUser : IEntity<Guid>, IEntityCreated
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TemporaryVoiceChannelId { get; set; }

    public ulong GuildId { get; set; }

    public ulong ChannelId { get; set; }

    public ulong UserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
