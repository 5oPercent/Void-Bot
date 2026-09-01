using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Discord;

namespace Void.Bot.Message.Request.Discord;

public sealed class GetDiscordGuildChannelListRequest : IRequest, IRequireDiscordBinding
{
    [Required]
    public string GuildId { get; set; } = string.Empty;
}

public sealed class GetDiscordGuildChannelListResponse : VoidBotResponse<List<DiscordGuildChannelDto>>
{
}
