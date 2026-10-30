using Void.Bot.Message.Constants.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Validators.ERA;

public static class EraWeaponConfigurationValidator
{
    public static void Validate(EraWeaponCatalogDto catalog)
    {
        if (catalog == null || catalog.Templates == null || catalog.Modifications == null ||
            catalog.SlotTypes == null || catalog.SlotConfigurations == null || catalog.Frameworks == null ||
            new[] { catalog.Templates.Count, catalog.Modifications.Count,
                catalog.SlotTypes.Count, catalog.SlotConfigurations.Count, catalog.Frameworks.Count }.Any(count => count > EraWeaponConstants.MaximumDefinitions))
            throw new ArgumentException("Weapon catalog size is invalid.");
        if (catalog.Templates.Any(value => value == null) || catalog.Modifications.Any(value => value == null) ||
            catalog.SlotTypes.Any(value => value == null) ||
            catalog.SlotConfigurations.Any(value => value == null) || catalog.Frameworks.Any(value => value == null))
            throw new ArgumentException("Weapon catalog definitions cannot be null.");

        var frameworks = new Dictionary<Guid, EraWeaponFrameworkDto>();
        foreach (var framework in catalog.Frameworks)
        {
            if (framework.Id == Guid.Empty || !frameworks.TryAdd(framework.Id, framework))
                throw new ArgumentException("Weapon framework identity is invalid.");
            ValidateFramework(framework);
        }

        var typeIds = new HashSet<Guid>();
        foreach (var type in catalog.SlotTypes)
            if (type.Id == Guid.Empty || !typeIds.Add(type.Id) || string.IsNullOrWhiteSpace(type.Name) || type.Name.Length > 128)
                throw new ArgumentException("Weapon slot type identity or name is invalid.");
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
                !ValidPresentationKey(template.PresentationKey) ||
                !frameworks.ContainsKey(template.FrameworkId) ||
                (template.SlotConfigurationId.HasValue && !configurations.ContainsKey(template.SlotConfigurationId.Value)) ||
                compatibleSlots == null || compatibleSlots.Count == 0 ||
                compatibleSlots.Any(value => value < EraEquipmentSlot.Weapon1 || value > EraEquipmentSlot.Weapon3) ||
                compatibleSlots.Distinct().Count() != compatibleSlots.Count)
                throw new ArgumentException("Weapon template, configuration reference or equipment mapping is invalid.");
            var framework = frameworks[template.FrameworkId];
            ValidateStats(template.BaseStats, framework);
            if (template.SlotConfigurationId.HasValue && configurations[template.SlotConfigurationId.Value].Slots.SelectMany(slot => slot.Pool)
                .Any(entry => modifications[entry.ModDefinitionId].Modifiers.Any(value => !SupportsStat(template.BaseStats, value.Stat))))
                throw new ArgumentException("Weapon configuration uses an attribute unsupported by an associated template.");
        }
        if (System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(catalog).Length > EraWeaponConstants.MaximumCatalogBytes)
            throw new ArgumentException("Weapon catalog exceeds its publication byte budget.");
    }

    private static bool ValidPresentationKey(string? key) => !string.IsNullOrWhiteSpace(key) && key.Length <= 128 && key == key.Trim() &&
        key.All(character => char.IsAsciiLetterOrDigit(character) || character is '.' or '_' or '-');

    public static bool SupportsStat(IReadOnlyCollection<EraWeaponStatValueDto> stats, EraWeaponStat stat) =>
        stats.Any(value => value.Stat == stat);

    public static void ValidateStats(IReadOnlyCollection<EraWeaponStatValueDto> stats, EraWeaponFrameworkDto framework)
    {
        if (stats == null || stats.Count > 11 || stats.Any(value => value == null) ||
            stats.Select(value => value.Stat).Distinct().Count() != stats.Count)
            throw new ArgumentException("Weapon base stat entries are invalid.");
        var required = new HashSet<EraWeaponStat>
        {
            EraWeaponStat.Damage, EraWeaponStat.Frequency, EraWeaponStat.Handling,
            EraWeaponStat.LoadedCapacity, EraWeaponStat.ReserveCapacity, EraWeaponStat.Penetration
        };
        required.UnionWith(framework.SupplementaryStats);
        if (!required.SetEquals(stats.Select(value => value.Stat)))
            throw new ArgumentException("Weapon values must exactly match its framework's applicable attributes.");
        foreach (var entry in stats)
        {
            var value = entry.Value;
            var valid = entry.Stat switch
            {
                EraWeaponStat.Damage => value >= 0 && value <= 1000000,
                EraWeaponStat.Frequency or EraWeaponStat.ReloadSpeed or EraWeaponStat.ProjectileSpeed => value > 0 && value <= 1000000,
                EraWeaponStat.Handling or EraWeaponStat.Accuracy or EraWeaponStat.Stability => value >= 0 && value <= 100,
                EraWeaponStat.LoadedCapacity => value >= 1 && value <= 1000000 && value == Math.Truncate(value),
                EraWeaponStat.ReserveCapacity or EraWeaponStat.Penetration => value >= -1 && value <= 1000000 && value == Math.Truncate(value),
                EraWeaponStat.ChargeTime => value > 0 && value <= 3600,
                _ => false
            };
            if (!double.IsFinite(value) || !valid)
                throw new ArgumentException("Weapon base stat value is invalid.");
            if (entry.Stat is EraWeaponStat.Accuracy or EraWeaponStat.Stability or EraWeaponStat.Handling)
            {
                var zero = entry.EffectAtZero;
                var hundred = entry.EffectAtHundred;
                var maximum = entry.Stat == EraWeaponStat.Handling ? 3600 : 180;
                if (!zero.HasValue || !hundred.HasValue || !double.IsFinite(zero.Value) || !double.IsFinite(hundred.Value) ||
                    zero < hundred || zero > maximum || (entry.Stat == EraWeaponStat.Handling ? hundred <= 0 : hundred < 0))
                    throw new ArgumentException("Weapon score effect endpoints are invalid.");
            }
            else if (entry.EffectAtZero.HasValue || entry.EffectAtHundred.HasValue)
                throw new ArgumentException("Direct weapon attributes cannot contain score effect endpoints.");
        }
    }

    public static void ValidateFramework(EraWeaponFrameworkDto framework)
    {
        if (framework == null || string.IsNullOrWhiteSpace(framework.Name) || framework.Name.Length > 128 ||
            string.IsNullOrWhiteSpace(framework.Category) || framework.Category.Length > 64 || framework.Category != framework.Category.Trim() ||
            framework.Note is not { Length: <= 2048 } || !IsValidPresentationDriverKey(framework.PresentationDriverKey) ||
            framework.SupplementaryStats == null || framework.Capabilities == null || framework.Capabilities.Count > 8 ||
            framework.Capabilities.Any(capability => capability == null) ||
            framework.Capabilities.Select(capability => capability.Kind).Distinct().Count() != framework.Capabilities.Count ||
            framework.SupplementaryStats.Distinct().Count() != framework.SupplementaryStats.Count)
            throw new ArgumentException("Weapon framework identity, driver or capabilities are invalid.");
        var kinds = framework.Capabilities.Select(capability => capability.Kind).ToHashSet();
        var supplementary = new HashSet<EraWeaponStat>();
        foreach (var capability in framework.Capabilities)
        {
            var configurations = new Dictionary<EraWeaponCapabilityKind, object?>
            {
                [EraWeaponCapabilityKind.Aim] = capability.Aim, [EraWeaponCapabilityKind.Recoil] = capability.Recoil,
                [EraWeaponCapabilityKind.Reload] = capability.Reload, [EraWeaponCapabilityKind.Charge] = capability.Charge,
                [EraWeaponCapabilityKind.Projectile] = capability.Projectile, [EraWeaponCapabilityKind.MeleeHit] = capability.MeleeHit,
                [EraWeaponCapabilityKind.Firing] = capability.Firing, [EraWeaponCapabilityKind.Hitscan] = capability.Hitscan
            };
            if (!configurations.TryGetValue(capability.Kind, out var configuration) || configuration == null ||
                configurations.Count(pair => pair.Value != null) != 1)
                throw new ArgumentException("A weapon capability requires exactly its own typed configuration.");
            switch (capability.Kind)
            {
                case EraWeaponCapabilityKind.Aim: supplementary.Add(EraWeaponStat.Accuracy); break;
                case EraWeaponCapabilityKind.Recoil: supplementary.Add(EraWeaponStat.Stability); break;
                case EraWeaponCapabilityKind.Reload:
                    supplementary.Add(EraWeaponStat.ReloadSpeed);
                    if (!System.Enum.IsDefined(capability.Reload!.Mode) || !double.IsFinite(capability.Reload.BaseSeconds) ||
                        capability.Reload.BaseSeconds <= 0 || capability.Reload.BaseSeconds > 3600)
                        throw new ArgumentException("Weapon reload configuration is invalid.");
                    break;
                case EraWeaponCapabilityKind.Charge: supplementary.Add(EraWeaponStat.ChargeTime); break;
                case EraWeaponCapabilityKind.Projectile: supplementary.Add(EraWeaponStat.ProjectileSpeed); break;
                case EraWeaponCapabilityKind.Firing:
                    if (!System.Enum.IsDefined(capability.Firing!.Mode) || capability.Firing.BurstCount < 1 || capability.Firing.BurstCount > 64 ||
                        capability.Firing.Mode != EraWeaponFireMode.Burst && capability.Firing.BurstCount != 1 ||
                        (capability.Firing.Mode == EraWeaponFireMode.Charged) != kinds.Contains(EraWeaponCapabilityKind.Charge))
                        throw new ArgumentException("Weapon firing configuration is invalid.");
                    break;
            }
            var distance = capability.Projectile?.MaximumDistance ?? capability.Hitscan?.MaximumDistance;
            if (distance.HasValue && (!double.IsFinite(distance.Value) || distance <= 0 || distance > 1000000))
                throw new ArgumentException("Weapon maximum travel/detection distance is invalid.");
        }
        if (!supplementary.SetEquals(framework.SupplementaryStats))
            throw new ArgumentException("Framework attributes do not match its capabilities.");
        if (kinds.Contains(EraWeaponCapabilityKind.MeleeHit))
        {
            if (kinds.Except(new[] { EraWeaponCapabilityKind.MeleeHit, EraWeaponCapabilityKind.Reload, EraWeaponCapabilityKind.Charge }).Any())
                throw new ArgumentException("Melee hit capabilities are invalid.");
        }
        else if (!kinds.Contains(EraWeaponCapabilityKind.Firing) ||
            kinds.Contains(EraWeaponCapabilityKind.Projectile) == kinds.Contains(EraWeaponCapabilityKind.Hitscan))
            throw new ArgumentException("Ranged hit capabilities require firing and exactly one hit method.");
    }

    public static bool IsValidPresentationDriverKey(string? key) => key is { Length: >= 1 and <= 32 } &&
        key.All(character => char.IsAsciiLetterOrDigit(character) || character is '.' or '_' or '-');
}
