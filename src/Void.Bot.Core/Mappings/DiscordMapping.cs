using AutoMapper;
using Void.Bot.Core.Domain.Discord;
using Void.Bot.Message.Commands.Discord;
using Void.Bot.Message.Dto.Discord;

namespace Void.Bot.Core.Mappings;

public class DiscordMapping : Profile
{
    public DiscordMapping()
    {
        CreateMap<DiscordVoiceChannelConfig, DiscordVoiceChannelConfigDto>()
            .ForMember(destination => destination.GuildId, options => options.MapFrom(source => source.GuildId.ToString()))
            .ForMember(destination => destination.TriggerChannelId, options => options.MapFrom(source => source.TriggerChannelId.ToString()))
            .ForMember(destination => destination.CreateCategoryId, options => options.MapFrom(source => source.CreateCategoryId.ToString()));
        CreateMap<DiscordTemporaryVoiceChannel, DiscordTemporaryVoiceChannelDto>().ReverseMap();

        CreateMap<AddDiscordVoiceChannelConfigCommand, DiscordVoiceChannelConfig>();
        CreateMap<UpdateDiscordVoiceChannelConfigCommand, DiscordVoiceChannelConfig>();
    }
}
