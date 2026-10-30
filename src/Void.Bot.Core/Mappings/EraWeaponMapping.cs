using AutoMapper;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Mappings;

public sealed class EraWeaponMapping : VoidBotMappingProfile
{
    public EraWeaponMapping()
    {
        CreateMap<EraWeaponSlotType, EraWeaponSlotTypeDto>();
        CreateMap<EraWeaponSlotTypeDto, EraWeaponSlotType>(MemberList.None);
        CreateMap<EraWeaponSlotConfiguration, EraWeaponSlotConfigurationDto>()
            .ForMember(row => row.Slots, option => option.Ignore());
        CreateMap<EraWeaponSlotConfigurationDto, EraWeaponSlotConfiguration>(MemberList.None);
        CreateMap<EraWeaponFramework, EraWeaponFrameworkDto>()
            .ForMember(row => row.SupplementaryStats, option => option.Ignore())
            .ForMember(row => row.Capabilities, option => option.Ignore());
        CreateMap<EraWeaponFrameworkDto, EraWeaponFramework>(MemberList.None);
        CreateMap<EraWeaponTemplateStat, EraWeaponStatValueDto>();
        CreateMap<EraWeaponTemplate, EraWeaponTemplateDto>()
            .ForMember(row => row.BaseStats, option => option.Ignore())
            .ForMember(row => row.CompatibleSlots, option => option.Ignore());
        CreateMap<EraWeaponModDefinition, EraWeaponModDefinitionDto>()
            .ForMember(row => row.Modifiers, option => option.Ignore());
        CreateMap<EraWeaponModStatModifier, EraWeaponModifierDto>();
        CreateMap<EraWeaponConfigurationModPool, EraWeaponModPoolEntryDto>();
        CreateMap<EraWeaponConfigurationSlot, EraWeaponConfigurationSlotDto>()
            .ForMember(row => row.Pool, option => option.Ignore());
        CreateMap<EraWeaponInstanceModCandidate, EraWeaponCandidateDto>();
    }
}
