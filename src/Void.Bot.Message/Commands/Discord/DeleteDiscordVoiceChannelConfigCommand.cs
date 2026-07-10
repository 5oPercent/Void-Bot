using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Discord;

public class DeleteDiscordVoiceChannelConfigCommand : ICommand
{
    [Required]
    [MinLength(1)]
    public List<Guid> IdList { get; set; } = [];
}

public class DeleteDiscordVoiceChannelConfigResponse : VoidBotResponse
{
}
