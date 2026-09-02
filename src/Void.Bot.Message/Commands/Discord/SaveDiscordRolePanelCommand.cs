using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Discord;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Message.Commands.Discord;

public sealed class SaveDiscordRolePanelCommand : ICommand, IRequireDiscordBinding, IRequireDiscordGuild
{
    [Required]
    public string GuildId { get; set; } = string.Empty;

    [Required]
    public string ChannelId { get; set; } = string.Empty;

    [StringLength(256)]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000)]
    public string Content { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    [Required]
    [MinLength(1)]
    [MaxLength(25)]
    public List<DiscordRolePanelItemDto> Items { get; set; } = [];
}

public sealed class SaveDiscordRolePanelResponse : VoidBotResponse<DiscordRolePanelDto>
{
}
