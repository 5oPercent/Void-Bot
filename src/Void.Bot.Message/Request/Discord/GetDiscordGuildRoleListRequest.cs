using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Discord;

namespace Void.Bot.Message.Request.Discord;

public sealed class GetDiscordGuildRoleListRequest : IRequest, IRequireDiscordBinding, IRequireDiscordGuild
{
    [Required]
    public string GuildId { get; set; } = string.Empty;
}

public sealed class GetDiscordGuildRoleListResponse : VoidBotResponse<List<DiscordGuildRoleDto>>
{
}
