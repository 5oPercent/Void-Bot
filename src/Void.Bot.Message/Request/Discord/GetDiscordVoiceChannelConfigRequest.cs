using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Discord;

namespace Void.Bot.Message.Request.Discord;

public class GetDiscordVoiceChannelConfigRequest : IRequest
{
    [Required]
    public Guid Id { get; set; }
}

public class GetDiscordVoiceChannelConfigResponse : VoidBotResponse<DiscordVoiceChannelConfigDto>
{
}
