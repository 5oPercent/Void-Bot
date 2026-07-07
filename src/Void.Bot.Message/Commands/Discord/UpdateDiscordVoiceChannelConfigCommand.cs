using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Discord;

public class UpdateDiscordVoiceChannelConfigCommand : ICommand
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public ulong GuildId { get; set; }

    [Required]
    public ulong TriggerChannelId { get; set; }

    [Required]
    public ulong CreateCategoryId { get; set; }

    [Range(3, 50)]
    public int DefaultUserLimit { get; set; } = 3;

    [StringLength(128)]
    public string ChannelNameTemplate { get; set; } = "{user} 的频道";

    public bool Enabled { get; set; } = true;
}

public class UpdateDiscordVoiceChannelConfigResponse : VoidBotResponse
{
}
