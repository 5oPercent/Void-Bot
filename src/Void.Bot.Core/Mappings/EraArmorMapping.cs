using AutoMapper;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Mappings;

public sealed class EraArmorMapping : VoidBotMappingProfile
{
    public EraArmorMapping()
    {
        CreateMap<EraArmorSlotType, EraArmorSlotTypeDto>();
        CreateMap<EraArmorSlotTypeDto, EraArmorSlotType>(MemberList.None);
        CreateMap<EraArmorSlotConfiguration, EraArmorSlotConfigurationDto>()
            .ForMember(row => row.Slots, option => option.Ignore());
        CreateMap<EraArmorSlotConfigurationDto, EraArmorSlotConfiguration>(MemberList.None);
        CreateMap<EraArmorTemplate, EraArmorTemplateDto>()
            .ForMember(row => row.CompatibleSlots, option => option.Ignore());
        CreateMap<EraArmorModDefinition, EraArmorModDefinitionDto>()
            .ForMember(row => row.Modifiers, option => option.Ignore());
        CreateMap<EraArmorModStatModifier, EraArmorModifierDto>();
        CreateMap<EraArmorConfigurationModPool, EraArmorModPoolEntryDto>();
        CreateMap<EraArmorConfigurationSlot, EraArmorConfigurationSlotDto>()
            .ForMember(row => row.Pool, option => option.Ignore());
        CreateMap<EraArmorModCandidate, EraArmorCandidateDto>();
    }
}
