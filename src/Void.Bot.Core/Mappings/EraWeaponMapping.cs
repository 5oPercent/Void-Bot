using AutoMapper;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Mappings;

public sealed class EraWeaponMapping : VoidBotMappingProfile
{
    public EraWeaponMapping()
    {
        CreateMap<EraWeaponStatMapping, EraWeaponStatMappingDto>();
        CreateMap<EraWeaponStatMappingDto, EraWeaponStatMapping>(MemberList.None);
        CreateMap<EraWeaponTemplate, EraWeaponStatsDto>();
        CreateMap<EraWeaponStatsDto, EraWeaponTemplate>(MemberList.None);
        CreateMap<EraWeaponTemplate, EraWeaponTemplateDto>()
            .ForMember(row => row.BaseStats, option => option.MapFrom(row => row))
            .ForMember(row => row.Behavior, option => option.Ignore())
            .ForMember(row => row.CompatibleSlots, option => option.Ignore())
            .ForMember(row => row.Slots, option => option.Ignore());
        CreateMap<EraWeaponModDefinition, EraWeaponModDefinitionDto>()
            .ForMember(row => row.Modifiers, option => option.Ignore());
        CreateMap<EraWeaponModStatModifier, EraWeaponModifierDto>();
        CreateMap<EraWeaponTemplateModPool, EraWeaponModPoolEntryDto>();
        CreateMap<EraWeaponTemplateModSlot, EraWeaponTemplateSlotDto>()
            .ForMember(row => row.Pool, option => option.Ignore());
        CreateMap<EraWeaponModCandidate, EraWeaponCandidateDto>();
    }
}
