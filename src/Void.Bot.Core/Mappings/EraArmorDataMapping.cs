using System.Text.Json;
using AutoMapper;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Mappings;

/// <summary>组装护甲关系数据，不执行查询或写入。</summary>
internal static class EraArmorDataMapping
{
    internal static EraArmorCatalogDto CreateCatalog(IReadOnlyCollection<EraArmorSlotType> slotTypes, IReadOnlyCollection<EraArmorSlotConfiguration> configurations,
        IReadOnlyCollection<EraArmorTemplate> templates,
        IReadOnlyCollection<EraArmorModDefinition> definitions, IReadOnlyCollection<EraArmorModStatModifier> modifiers,
        IReadOnlyCollection<EraArmorConfigurationSlot> slots, IReadOnlyCollection<EraArmorConfigurationModPool> pools, IMapper mapper)
    {
        var result = new EraArmorCatalogDto
        {
            SlotTypes = mapper.Map<List<EraArmorSlotTypeDto>>(slotTypes)
        };
        foreach (var entity in configurations)
        {
            var configuration = mapper.Map<EraArmorSlotConfigurationDto>(entity);
            foreach (var row in slots.Where(row => row.ConfigurationId == entity.Id))
            {
                var slot = mapper.Map<EraArmorConfigurationSlotDto>(row);
                slot.Pool = mapper.Map<List<EraArmorModPoolEntryDto>>(pools.Where(pool => pool.SlotId == row.Id));
                configuration.Slots.Add(slot);
            }
            result.SlotConfigurations.Add(configuration);
        }
        foreach (var entity in templates)
        {
            var template = mapper.Map<EraArmorTemplateDto>(entity);
            template.CompatibleSlots = JsonSerializer.Deserialize<List<EraEquipmentSlot>>(entity.CompatibleSlotsPayload)
                ?? throw new ArgumentException("Armor equipment mapping is missing.");
            result.Templates.Add(template);
        }
        foreach (var entity in definitions)
        {
            var definition = mapper.Map<EraArmorModDefinitionDto>(entity);
            definition.Modifiers = mapper.Map<List<EraArmorModifierDto>>(modifiers.Where(row => row.ModDefinitionId == entity.Id));
            result.Modifications.Add(definition);
        }
        return result;
    }

    internal static Dictionary<Guid, EraArmorInstanceDto> CreateInstances(IReadOnlyCollection<EraArmorModCandidate> candidates,
        IReadOnlyCollection<EraArmorModSelection> selections, IMapper mapper, IEnumerable<Guid> equipmentIds)
    {
        var selectionKeys = selections.Select(row => (row.EquipmentId, row.SlotId)).ToHashSet();
        var slotsByEquipment = selections.ToLookup(row => row.EquipmentId);
        var candidatesBySlot = candidates.ToLookup(row => (row.EquipmentId, row.SlotId));
        if (candidates.Any(candidate => !selectionKeys.Contains((candidate.EquipmentId, candidate.SlotId))))
            throw new ArgumentException("Armor candidates have no corresponding selection.");
        return equipmentIds.Distinct().ToDictionary(id => id, id => new EraArmorInstanceDto
        {
            Slots = slotsByEquipment[id].OrderBy(row => row.DisplayOrder).ThenBy(row => row.SlotId).Select(row => new EraArmorInstanceSlotDto
            {
                SlotId = row.SlotId, Name = row.Name, SlotTypeId = row.SlotTypeId, DisplayOrder = row.DisplayOrder, SelectedCandidateId = row.SelectedCandidateId,
                Candidates = mapper.Map<List<EraArmorCandidateDto>>(candidatesBySlot[(id, row.SlotId)])
            }).ToList()
        });
    }

    internal static IEnumerable<EraArmorModCandidate> CreateCandidates(Guid equipmentId, EraArmorInstanceDto instance) =>
        instance.Slots.SelectMany(slot => slot.Candidates.Select(candidate => new EraArmorModCandidate
        {
            CandidateId = candidate.CandidateId, EquipmentId = equipmentId, SlotId = slot.SlotId,
            ModDefinitionId = candidate.ModDefinitionId, DisplayOrder = candidate.DisplayOrder
        }));

    internal static IEnumerable<EraArmorModSelection> CreateSelections(Guid equipmentId, EraArmorInstanceDto instance) =>
        instance.Slots.Select(slot => new EraArmorModSelection
        {
            EquipmentId = equipmentId, SlotId = slot.SlotId, Name = slot.Name, SlotTypeId = slot.SlotTypeId, DisplayOrder = slot.DisplayOrder, SelectedCandidateId = slot.SelectedCandidateId
        });
}
