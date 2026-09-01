using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Discord;

namespace Void.Bot.Message.Request.Discord;

public sealed class GetDiscordManagedGuildListRequest : IRequest, IRequireDiscordBinding
{
}

public sealed class GetDiscordManagedGuildListResponse : VoidBotResponse<List<DiscordManagedGuildDto>>
{
}
