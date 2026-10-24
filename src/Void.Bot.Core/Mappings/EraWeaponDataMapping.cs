using System.Text.Json;
using AutoMapper;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Mappings;

/// <summary>组装武器关系数据，不执行查询或写入。</summary>
internal static class EraWeaponDataMapping
{
    internal static EraWeaponCatalogDto CreateCatalog(IReadOnlyCollection<EraWeaponTemplate> templates, IReadOnlyCollection<EraWeaponStatMapping> statMappings,
        IReadOnlyCollection<EraWeaponModDefinition> definitions, IReadOnlyCollection<EraWeaponModStatModifier> modifiers,
        IReadOnlyCollection<EraWeaponTemplateModSlot> slots, IReadOnlyCollection<EraWeaponTemplateModPool> pools, IMapper mapper)
    {
        var result = new EraWeaponCatalogDto { StatMappings = mapper.Map<List<EraWeaponStatMappingDto>>(statMappings) };
        foreach (var entity in templates)
        {
            var template = mapper.Map<EraWeaponTemplateDto>(entity);
            template.Behavior = JsonSerializer.Deserialize<EraWeaponBehaviorDto>(entity.BehaviorPayload)
                ?? throw new ArgumentException("Weapon behavior is missing.");
            template.CompatibleSlots = JsonSerializer.Deserialize<List<EraEquipmentSlot>>(entity.CompatibleSlotsPayload)
                ?? throw new ArgumentException("Weapon equipment mapping is missing.");
            foreach (var row in slots.Where(row => row.TemplateId == entity.Id))
            {
                var slot = mapper.Map<EraWeaponTemplateSlotDto>(row);
                slot.Pool = mapper.Map<List<EraWeaponModPoolEntryDto>>(pools.Where(pool => pool.TemplateId == entity.Id && pool.Slot == row.Slot));
                template.Slots.Add(slot);
            }
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

    internal static Dictionary<Guid, EraWeaponInstanceDto> CreateInstances(IReadOnlyCollection<EraWeaponModCandidate> candidates,
        IReadOnlyCollection<EraWeaponModSelection> selections, IMapper mapper)
    {
        if (candidates.Any(candidate => !selections.Any(selection => selection.EquipmentId == candidate.EquipmentId && selection.Slot == candidate.Slot)))
            throw new ArgumentException("Weapon candidates have no corresponding selection.");
        return selections.Select(row => row.EquipmentId).Distinct().ToDictionary(id => id, id => new EraWeaponInstanceDto
        {
            Slots = selections.Where(row => row.EquipmentId == id).Select(row => new EraWeaponInstanceSlotDto
            {
                Slot = row.Slot, SelectedCandidateId = row.SelectedCandidateId,
                Candidates = mapper.Map<List<EraWeaponCandidateDto>>(candidates.Where(candidate => candidate.EquipmentId == id && candidate.Slot == row.Slot))
            }).ToList()
        });
    }

    internal static IEnumerable<EraWeaponModCandidate> CreateCandidates(Guid equipmentId, EraWeaponInstanceDto instance) =>
        instance.Slots.SelectMany(slot => slot.Candidates.Select(candidate => new EraWeaponModCandidate
        {
            CandidateId = candidate.CandidateId, EquipmentId = equipmentId, Slot = slot.Slot,
            ModDefinitionId = candidate.ModDefinitionId, DisplayOrder = candidate.DisplayOrder
        }));

    internal static IEnumerable<EraWeaponModSelection> CreateSelections(Guid equipmentId, EraWeaponInstanceDto instance) =>
        instance.Slots.Select(slot => new EraWeaponModSelection
        {
            EquipmentId = equipmentId, Slot = slot.Slot, SelectedCandidateId = slot.SelectedCandidateId
        });
}
