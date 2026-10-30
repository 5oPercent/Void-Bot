using System.Text.Json;
using AutoMapper;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Mappings;

/// <summary>组装武器关系数据，不执行查询或写入。</summary>
internal static class EraWeaponDataMapping
{
    internal static EraWeaponCatalogDto CreateCatalog(IReadOnlyCollection<EraWeaponSlotType> slotTypes, IReadOnlyCollection<EraWeaponSlotConfiguration> configurations,
        IReadOnlyCollection<EraWeaponTemplate> templates,
        IReadOnlyCollection<EraWeaponModDefinition> definitions, IReadOnlyCollection<EraWeaponModStatModifier> modifiers,
        IReadOnlyCollection<EraWeaponConfigurationSlot> slots, IReadOnlyCollection<EraWeaponConfigurationModPool> pools,
        IReadOnlyCollection<EraWeaponFramework> frameworks, IReadOnlyCollection<EraWeaponFrameworkStat> frameworkStats,
        IReadOnlyCollection<EraWeaponFrameworkCapability> capabilities, IReadOnlyCollection<EraWeaponTemplateStat> templateStats, IMapper mapper)
    {
        var result = new EraWeaponCatalogDto
        {
            SlotTypes = mapper.Map<List<EraWeaponSlotTypeDto>>(slotTypes)
        };
        foreach (var entity in frameworks)
        {
            var framework = mapper.Map<EraWeaponFrameworkDto>(entity);
            framework.SupplementaryStats = frameworkStats.Where(row => row.FrameworkId == entity.Id).Select(row => row.Stat).ToList();
            foreach (var row in capabilities.Where(row => row.FrameworkId == entity.Id))
            {
                var capability = JsonSerializer.Deserialize<EraWeaponCapabilityDto>(row.ConfigurationPayload)
                    ?? throw new ArgumentException("Weapon capability configuration is missing.");
                if (capability.Kind != row.CapabilityKind) throw new ArgumentException("Weapon capability identity is inconsistent.");
                framework.Capabilities.Add(capability);
            }
            result.Frameworks.Add(framework);
        }
        foreach (var entity in configurations)
        {
            var configuration = mapper.Map<EraWeaponSlotConfigurationDto>(entity);
            foreach (var row in slots.Where(row => row.ConfigurationId == entity.Id))
            {
                var slot = mapper.Map<EraWeaponConfigurationSlotDto>(row);
                slot.Pool = mapper.Map<List<EraWeaponModPoolEntryDto>>(pools.Where(pool => pool.SlotId == row.Id));
                configuration.Slots.Add(slot);
            }
            result.SlotConfigurations.Add(configuration);
        }
        foreach (var entity in templates)
        {
            var template = mapper.Map<EraWeaponTemplateDto>(entity);
            template.BaseStats = mapper.Map<List<EraWeaponStatValueDto>>(templateStats.Where(row => row.TemplateId == entity.Id));
            template.CompatibleSlots = JsonSerializer.Deserialize<List<EraEquipmentSlot>>(entity.CompatibleSlotsPayload)
                ?? throw new ArgumentException("Weapon equipment mapping is missing.");
            result.Templates.Add(template);
        }
        foreach (var entity in definitions)
        {
            var definition = mapper.Map<EraWeaponModDefinitionDto>(entity);
            definition.Modifiers = mapper.Map<List<EraWeaponModifierDto>>(modifiers.Where(row => row.ModDefinitionId == entity.Id));
            result.Modifications.Add(definition);
        }
        return result;
    }

    internal static Dictionary<Guid, EraWeaponInstanceDto> CreateInstances(IReadOnlyCollection<EraWeaponInstanceModCandidate> candidates,
        IReadOnlyCollection<EraWeaponInstanceModSelection> selections, IMapper mapper, IEnumerable<Guid> equipmentIds)
    {
        var selectionKeys = selections.Select(row => (row.EquipmentInstanceId, row.SlotId)).ToHashSet();
        var slotsByEquipment = selections.ToLookup(row => row.EquipmentInstanceId);
        var candidatesBySlot = candidates.ToLookup(row => (row.EquipmentInstanceId, row.SlotId));
        if (candidates.Any(candidate => !selectionKeys.Contains((candidate.EquipmentInstanceId, candidate.SlotId))))
            throw new ArgumentException("Weapon candidates have no corresponding selection.");
        return equipmentIds.Distinct().ToDictionary(id => id, id => new EraWeaponInstanceDto
        {
            Slots = slotsByEquipment[id].OrderBy(row => row.DisplayOrder).ThenBy(row => row.SlotId).Select(row => new EraWeaponInstanceSlotDto
            {
                SlotId = row.SlotId, Name = row.Name, SlotTypeId = row.SlotTypeId, DisplayOrder = row.DisplayOrder, SelectedCandidateId = row.SelectedCandidateId,
                Candidates = mapper.Map<List<EraWeaponCandidateDto>>(candidatesBySlot[(id, row.SlotId)])
            }).ToList()
        });
    }

    internal static IEnumerable<EraWeaponInstanceModCandidate> CreateCandidates(Guid equipmentId, EraWeaponInstanceDto instance) =>
        instance.Slots.SelectMany(slot => slot.Candidates.Select(candidate => new EraWeaponInstanceModCandidate
        {
            CandidateId = candidate.CandidateId, EquipmentInstanceId = equipmentId, SlotId = slot.SlotId,
            ModDefinitionId = candidate.ModDefinitionId, DisplayOrder = candidate.DisplayOrder
        }));

    internal static IEnumerable<EraWeaponInstanceModSelection> CreateSelections(Guid equipmentId, EraWeaponInstanceDto instance) =>
        instance.Slots.Select(slot => new EraWeaponInstanceModSelection
        {
            EquipmentInstanceId = equipmentId, SlotId = slot.SlotId, Name = slot.Name, SlotTypeId = slot.SlotTypeId, DisplayOrder = slot.DisplayOrder, SelectedCandidateId = slot.SelectedCandidateId
        });
}
