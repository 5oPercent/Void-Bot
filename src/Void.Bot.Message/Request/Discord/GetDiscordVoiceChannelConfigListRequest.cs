using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Discord;

namespace Void.Bot.Message.Request.Discord;

public class GetDiscordVoiceChannelConfigListRequest : IRequest
{
    public ulong? GuildId { get; set; }
}

public class GetDiscordVoiceChannelConfigListResponse : VoidBotResponse<List<DiscordVoiceChannelConfigDto>>
{
}
