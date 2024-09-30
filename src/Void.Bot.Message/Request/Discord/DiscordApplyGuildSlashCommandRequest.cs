using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Request.Discord;

public class DiscordApplyGuildSlashCommandRequest : IRequest
{
    public ulong GuildId { get; set; }
}

public class DiscordApplyGuildSlashCommandResponse : VoidBotResponse
{
}