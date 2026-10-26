using System.Text.Json;
using SystemEnum = System.Enum;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Constants.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Validators.ERA;

public static class EraArmorConfigurationValidator
{
    public static void Validate(EraArmorCatalogDto catalog)
    {
        if (catalog == null || catalog.Templates == null || catalog.Modifications == null ||
            catalog.SlotTypes == null || catalog.SlotConfigurations == null ||
            new[] { catalog.Templates.Count, catalog.Modifications.Count, catalog.SlotTypes.Count, catalog.SlotConfigurations.Count }.Any(count => count > EraArmorConstants.MaximumDefinitions))
            throw new ArgumentException("Armor catalog size is invalid.");
        if (catalog.Templates.Any(value => value == null) || catalog.Modifications.Any(value => value == null) ||
            catalog.SlotTypes.Any(value => value == null) || catalog.SlotConfigurations.Any(value => value == null))
            throw new ArgumentException("Armor catalog definitions cannot be null.");

        var typeIds = new HashSet<Guid>();
        foreach (var type in catalog.SlotTypes)
            if (type.Id == Guid.Empty || !typeIds.Add(type.Id) || string.IsNullOrWhiteSpace(type.Name) || type.Name.Length > 128)
                throw new ArgumentException("Armor slot type identity or name is invalid.");
        var modifications = new Dictionary<Guid, EraArmorModDefinitionDto>();
        foreach (var modification in catalog.Modifications)
        {
            var modifiers = modification.Modifiers;
            if (modifiers == null || modifiers.Count > EraArmorConstants.MaximumModifiers || modifiers.Any(value => value == null))
                throw new ArgumentException("Armor attribute modifiers are invalid.");
            if (modification.Id == Guid.Empty || !modifications.TryAdd(modification.Id, modification) ||
                string.IsNullOrWhiteSpace(modification.Name) || modification.Name.Length > 128 || !typeIds.Contains(modification.SlotTypeId) ||
                modification.Description is not { Length: <= 2048 } || modification.Icon is not { Length: <= 256 })
                throw new ArgumentException("Armor modification is invalid.");
            if (modifiers.Any(value => !SystemEnum.IsDefined(value.Stat) || !SystemEnum.IsDefined(value.Operation) ||
                    !double.IsFinite(value.Value) || Math.Abs(value.Value) > 1000000 ||
                    value.Operation == EraArmorModifierOperation.AddPercent && value.Value <= -1) ||
                modifiers.Select(value => (value.Stat, value.Operation)).Distinct().Count() != modifiers.Count)
                throw new ArgumentException("Armor attribute modifier is invalid.");
        }

        var configurations = new Dictionary<Guid, EraArmorSlotConfigurationDto>();
        var slotIds = new HashSet<Guid>();
        foreach (var configuration in catalog.SlotConfigurations)
        {
            if (configuration.Id == Guid.Empty || !configurations.TryAdd(configuration.Id, configuration) ||
                string.IsNullOrWhiteSpace(configuration.Name) || configuration.Name.Length > 128 ||
                configuration.Note is not { Length: <= 2048 } || configuration.Slots == null ||
                configuration.Slots.Count < 1 || configuration.Slots.Count > EraArmorConstants.MaximumDefinitions || configuration.Slots.Any(slot => slot == null))
                throw new ArgumentException("Armor slot configuration is invalid.");
            if (configuration.Slots.Select(slot => slot.SlotTypeId).Distinct().Count() != configuration.Slots.Count)
                throw new ArgumentException("Armor slot types must be unique within a scheme.");
            foreach (var slot in configuration.Slots)
            {
                if (slot.Id == Guid.Empty || !slotIds.Add(slot.Id) || !typeIds.Contains(slot.SlotTypeId) ||
                    string.IsNullOrWhiteSpace(slot.Name) || slot.Name.Length > 128 || slot.DisplayOrder < 0)
                    throw new ArgumentException("Armor configuration slot identity, type or name is invalid.");
                var pool = slot.Pool;
                if (pool == null || pool.Count > EraArmorConstants.MaximumPoolEntries || pool.Any(entry => entry == null) ||
                    slot.CandidateCount < 1 || slot.CandidateCount > EraArmorConstants.MaximumCandidatesPerSlot ||
                    pool.Count < slot.CandidateCount || pool.Sum(entry => (long)entry.Weight) > int.MaxValue ||
                    pool.Select(entry => entry.ModDefinitionId).Distinct().Count() != pool.Count)
                    throw new ArgumentException("Armor CandidateCount exceeds the distinct available pool or its limit.");
                if (pool.Any(entry => entry.Weight <= 0 || !modifications.TryGetValue(entry.ModDefinitionId, out var modification) ||
                    modification.SlotTypeId != slot.SlotTypeId))
                    throw new ArgumentException("Armor pool reference, weight or slot type is invalid.");
            }
        }

        var ids = new HashSet<Guid>();
        foreach (var template in catalog.Templates)
        {
            var compatibleSlots = template.CompatibleSlots;
            if (template.Id == Guid.Empty || !ids.Add(template.Id) || string.IsNullOrWhiteSpace(template.Name) || template.Name.Length > 128 ||
                template.Icon is not { Length: <= 256 } || template.Note is not { Length: <= 2048 } ||
                template.PresentationKey is not { Length: >= 1 and <= 128 } ||
                template.PresentationKey.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not ('.' or '_' or '-')) ||
                (template.SlotConfigurationId.HasValue && !configurations.ContainsKey(template.SlotConfigurationId.Value)) ||
                compatibleSlots == null || compatibleSlots.Count == 0 ||
                compatibleSlots.Any(value => !EraEquipmentRules.ArmorSlots.Contains(value)) ||
                compatibleSlots.Distinct().Count() != compatibleSlots.Count)
                throw new ArgumentException("Armor template, configuration reference or equipment mapping is invalid.");

        }
        if (JsonSerializer.SerializeToUtf8Bytes(catalog).Length > EraArmorConstants.MaximumCatalogBytes)
            throw new ArgumentException("Armor catalog exceeds its publication byte budget.");
    }

}
