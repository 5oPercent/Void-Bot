using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Discord;

namespace Void.Bot.Message.Request.Discord;

public class GetDiscordTemporaryVoiceChannelListRequest : IRequest
{
    public ulong? GuildId { get; set; }

    public bool IncludeDeleted { get; set; }
}

public class GetDiscordTemporaryVoiceChannelListResponse : VoidBotResponse<List<DiscordTemporaryVoiceChannelDto>>
{
}
