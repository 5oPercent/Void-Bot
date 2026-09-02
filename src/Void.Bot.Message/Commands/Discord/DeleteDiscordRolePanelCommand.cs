using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Message.Commands.Discord;

public sealed class DeleteDiscordRolePanelCommand : ICommand, IRequireDiscordBinding, IRequireDiscordGuild
{
    [Required]
    public string GuildId { get; set; } = string.Empty;
}

public sealed class DeleteDiscordRolePanelResponse : VoidBotResponse
{
}
