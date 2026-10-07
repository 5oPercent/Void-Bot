using AutoMapper;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Mappings;

public class EraMapping : Profile
{
    public EraMapping()
    {
        CreateMap<EraConfiguration, EraConfigurationItemDto>()
            .ForMember(item => item.Key, options => options.MapFrom(configuration => configuration.ConfigKey));
        CreateMap<EraTeam, EraTeamDto>();
        CreateMap<EraTeamMember, EraTeamMemberDto>();
        CreateMap<EraPlayer, EraPlayerDto>();
        CreateMap<EraCharacter, EraCharacterDto>();
        CreateMap<EraCapacityChange, EraCapacityReceiptDto>();
    }
}
