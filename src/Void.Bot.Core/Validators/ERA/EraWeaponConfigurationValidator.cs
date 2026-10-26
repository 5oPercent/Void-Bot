using Void.Bot.Message.Constants.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Validators.ERA;

public static class EraWeaponConfigurationValidator
{
    public static void Validate(EraWeaponCatalogDto catalog)
    {
        if (catalog == null || catalog.Templates == null || catalog.Modifications == null || catalog.StatMappings == null ||
            catalog.SlotTypes == null || catalog.SlotConfigurations == null ||
            new[] { catalog.Templates.Count, catalog.Modifications.Count, catalog.StatMappings.Count,
                catalog.SlotTypes.Count, catalog.SlotConfigurations.Count }.Any(count => count > EraWeaponConstants.MaximumDefinitions))
            throw new ArgumentException("Weapon catalog size is invalid.");
        if (catalog.Templates.Any(value => value == null) || catalog.Modifications.Any(value => value == null) ||
            catalog.StatMappings.Any(value => value == null) || catalog.SlotTypes.Any(value => value == null) ||
            catalog.SlotConfigurations.Any(value => value == null))
            throw new ArgumentException("Weapon catalog definitions cannot be null.");

        var typeIds = new HashSet<Guid>();
        foreach (var type in catalog.SlotTypes)
            if (type.Id == Guid.Empty || !typeIds.Add(type.Id) || string.IsNullOrWhiteSpace(type.Name) || type.Name.Length > 128)
                throw new ArgumentException("Weapon slot type identity or name is invalid.");
        var mappingIds = new HashSet<Guid>();
        foreach (var mapping in catalog.StatMappings)
        {
            if (mapping.Id == Guid.Empty || !mappingIds.Add(mapping.Id) || string.IsNullOrWhiteSpace(mapping.Name) || mapping.Name.Length > 128)
                throw new ArgumentException("Weapon stat mapping identity is invalid.");
            foreach (var (zero, hundred) in new[] { (mapping.SpreadDegreesAtZero, mapping.SpreadDegreesAtHundred),
                (mapping.RecoilDegreesAtZero, mapping.RecoilDegreesAtHundred) })
                if (!double.IsFinite(zero) || !double.IsFinite(hundred) || zero < hundred || zero > 180 || hundred < 0)
                    throw new ArgumentException("Weapon angle effect endpoints are invalid.");
            foreach (var (zero, hundred) in new[] { (mapping.ReadySecondsAtZero, mapping.ReadySecondsAtHundred),
                (mapping.AimSecondsAtZero, mapping.AimSecondsAtHundred) })
                if (!double.IsFinite(zero) || !double.IsFinite(hundred) || zero < hundred || zero > 3600 || hundred <= 0)
                    throw new ArgumentException("Weapon action effect endpoints are invalid.");
        }

        var modifications = new Dictionary<Guid, EraWeaponModDefinitionDto>();
        foreach (var modification in catalog.Modifications)
        {
            var modifiers = modification.Modifiers;
            if (modifiers == null || modifiers.Count > EraWeaponConstants.MaximumModifiers || modifiers.Any(value => value == null))
                throw new ArgumentException("Weapon attribute modifiers are invalid.");
            if (modification.Id == Guid.Empty || !modifications.TryAdd(modification.Id, modification) ||
                string.IsNullOrWhiteSpace(modification.Name) || modification.Name.Length > 128 || !typeIds.Contains(modification.SlotTypeId) ||
                modification.Description is not { Length: <= 2048 } || modification.Icon is not { Length: <= 256 })
                throw new ArgumentException("Weapon modification is invalid.");
            if (modifiers.Any(value => !System.Enum.IsDefined(value.Stat) || !System.Enum.IsDefined(value.Operation) ||
                    !double.IsFinite(value.Value) || Math.Abs(value.Value) > 1000000 ||
                    value.Operation == EraWeaponModifierOperation.AddPercent && value.Value <= -1) ||
                modifiers.Select(value => (value.Stat, value.Operation)).Distinct().Count() != modifiers.Count)
                throw new ArgumentException("Weapon attribute modifier is invalid.");
        }

        var configurations = new Dictionary<Guid, EraWeaponSlotConfigurationDto>();
        var slotIds = new HashSet<Guid>();
        foreach (var configuration in catalog.SlotConfigurations)
        {
            if (configuration.Id == Guid.Empty || !configurations.TryAdd(configuration.Id, configuration) ||
                string.IsNullOrWhiteSpace(configuration.Name) || configuration.Name.Length > 128 ||
                configuration.Note is not { Length: <= 2048 } || configuration.Slots == null ||
                configuration.Slots.Count < 1 || configuration.Slots.Count > EraWeaponConstants.MaximumDefinitions || configuration.Slots.Any(slot => slot == null))
                throw new ArgumentException("Weapon slot configuration is invalid.");
            foreach (var slot in configuration.Slots)
            {
                if (slot.Id == Guid.Empty || !slotIds.Add(slot.Id) || !typeIds.Contains(slot.SlotTypeId) ||
                    string.IsNullOrWhiteSpace(slot.Name) || slot.Name.Length > 128 || slot.DisplayOrder < 0)
                    throw new ArgumentException("Weapon configuration slot identity, type or name is invalid.");
                var pool = slot.Pool;
                if (pool == null || pool.Count > EraWeaponConstants.MaximumPoolEntries || pool.Any(entry => entry == null) ||
                    slot.CandidateCount < 1 || slot.CandidateCount > EraWeaponConstants.MaximumCandidatesPerSlot ||
                    pool.Count < slot.CandidateCount || pool.Sum(entry => (long)entry.Weight) > int.MaxValue ||
                    pool.Select(entry => entry.ModDefinitionId).Distinct().Count() != pool.Count)
                    throw new ArgumentException("Weapon CandidateCount exceeds the distinct available pool or its limit.");
                if (pool.Any(entry => entry.Weight <= 0 || !modifications.TryGetValue(entry.ModDefinitionId, out var modification) ||
                    modification.SlotTypeId != slot.SlotTypeId))
                    throw new ArgumentException("Weapon pool reference, weight or slot type is invalid.");
            }
        }

        var ids = new HashSet<Guid>();
        foreach (var template in catalog.Templates)
        {
            var compatibleSlots = template.CompatibleSlots;
            if (template.Id == Guid.Empty || !ids.Add(template.Id) || string.IsNullOrWhiteSpace(template.Name) || template.Name.Length > 128 ||
                template.Icon is not { Length: <= 256 } || template.Note is not { Length: <= 2048 } ||
                !ValidPresentationKey(template.PresentationKey) || !mappingIds.Contains(template.StatMappingId) ||
                (template.SlotConfigurationId.HasValue && !configurations.ContainsKey(template.SlotConfigurationId.Value)) ||
                compatibleSlots == null || compatibleSlots.Count == 0 ||
                compatibleSlots.Any(value => value < EraEquipmentSlot.Weapon1 || value > EraEquipmentSlot.Weapon3) ||
                compatibleSlots.Distinct().Count() != compatibleSlots.Count)
                throw new ArgumentException("Weapon template, configuration reference or equipment mapping is invalid.");
            ValidateStats(template.BaseStats, template.Behavior);
            if (template.SlotConfigurationId.HasValue && configurations[template.SlotConfigurationId.Value].Slots.SelectMany(slot => slot.Pool).Any(entry => modifications[entry.ModDefinitionId].Modifiers.Any(value =>
                value.Stat == EraWeaponStat.ProjectileSpeed && !template.BaseStats.ProjectileSpeed.HasValue ||
                value.Stat == EraWeaponStat.ChargeTime && !template.BaseStats.ChargeTime.HasValue)))
                throw new ArgumentException("Weapon configuration uses an attribute unsupported by an associated template.");
        }
        if (System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(catalog).Length > EraWeaponConstants.MaximumCatalogBytes)
            throw new ArgumentException("Weapon catalog exceeds its publication byte budget.");
    }

    private static bool ValidPresentationKey(string? key) => !string.IsNullOrWhiteSpace(key) && key.Length <= 128 && key == key.Trim() &&
        key.All(character => char.IsAsciiLetterOrDigit(character) || character is '.' or '_' or '-');

    public static void ValidateStats(EraWeaponStatsDto stats, EraWeaponBehaviorDto behavior)
    {
        if (stats == null || behavior == null || new[] { stats.Damage, stats.Frequency, stats.ReloadSpeed, stats.Range }.Any(value => !double.IsFinite(value) || value <= 0 || value > 1000000) ||
            stats.LoadedCapacity < 1 || stats.LoadedCapacity > 1000000 || stats.ReserveCapacity < 0 || stats.ReserveCapacity > 1000000 ||
            new[] { stats.Accuracy, stats.Stability, stats.Handling }.Any(value => !double.IsFinite(value) || value < 0 || value > 100) ||
            !System.Enum.IsDefined(behavior.FireMode) || !System.Enum.IsDefined(behavior.HitMode) || !System.Enum.IsDefined(behavior.ReloadMode) ||
            !double.IsFinite(behavior.BaseReloadSeconds) || behavior.BaseReloadSeconds <= 0 || behavior.BaseReloadSeconds > 3600 ||
            behavior.BurstCount < 1 || behavior.BurstCount > 64 ||
            (behavior.HitMode == EraWeaponHitMode.Projectile) != stats.ProjectileSpeed.HasValue ||
            stats.ProjectileSpeed.HasValue && (!double.IsFinite(stats.ProjectileSpeed.Value) || stats.ProjectileSpeed.Value <= 0 || stats.ProjectileSpeed.Value > 1000000) ||
            (behavior.FireMode == EraWeaponFireMode.Charged) != stats.ChargeTime.HasValue ||
            stats.ChargeTime.HasValue && (!double.IsFinite(stats.ChargeTime.Value) || stats.ChargeTime.Value <= 0 || stats.ChargeTime.Value > 3600))
            throw new ArgumentException("Weapon base stats or typed behavior parameters are invalid.");
    }
}
