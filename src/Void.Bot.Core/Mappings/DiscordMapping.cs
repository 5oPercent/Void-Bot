using AutoMapper;
using Void.Bot.Core.Domain.Discord;
using Void.Bot.Message.Commands.Discord;
using Void.Bot.Message.Dto.Discord;

namespace Void.Bot.Core.Mappings;

public class DiscordMapping : Profile
{
    public DiscordMapping()
    {
        CreateMap<DiscordVoiceChannelConfig, DiscordVoiceChannelConfigDto>().ReverseMap();
        CreateMap<DiscordTemporaryVoiceChannel, DiscordTemporaryVoiceChannelDto>().ReverseMap();

        CreateMap<AddDiscordVoiceChannelConfigCommand, DiscordVoiceChannelConfig>();
        CreateMap<UpdateDiscordVoiceChannelConfigCommand, DiscordVoiceChannelConfig>();
    }
}
