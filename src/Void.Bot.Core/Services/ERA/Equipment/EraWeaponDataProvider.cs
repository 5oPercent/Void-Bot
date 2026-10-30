using Void.Bot.Core.Mappings;
using Void.Bot.Core.Exceptions;
using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment;

public interface IEraWeaponDataProvider : IScope
{
    Task<EraWeaponCatalogDto> GetCatalogAsync(CancellationToken cancellationToken);
    Task<Dictionary<Guid, EraWeaponInstanceDto>> GetInstancesAsync(IEnumerable<Guid> equipmentIds, CancellationToken cancellationToken);
    Task<EraEquipmentConfiguration?> LockPublicationAsync(CancellationToken cancellationToken);
    Task<List<EraEquipmentInstance>> GetReferencedWeaponsAsync(EraEquipmentReferenceChanges changes, CancellationToken cancellationToken);
    Task<List<string>> GetPendingRewardPlansAsync(CancellationToken cancellationToken);
    Task<string?> GetRewardConfigurationPayloadAsync(CancellationToken cancellationToken);
    Task SaveCatalogAsync(EraWeaponCatalogDto previous, EraWeaponCatalogDto catalog, CancellationToken cancellationToken);
    void AddInstance(Guid equipmentId, EraWeaponInstanceDto instance);
    Task UpdateSelectionAsync(Guid equipmentId, Guid slotId, Guid previous, Guid candidate, long expectedItemVersion, CancellationToken cancellationToken);
}

/// <summary>武器关系表访问；配置和资产写入均由调用方工作单元提交。</summary>
public sealed class EraWeaponDataProvider : IEraWeaponDataProvider
{
    private readonly IRepository<EraWeaponTemplate> _eraWeaponTemplateRepository;
    private readonly IRepository<EraWeaponTemplateStat> _eraWeaponTemplateStatRepository;
    private readonly IRepository<EraWeaponModDefinition> _eraWeaponModDefinitionRepository;
    private readonly IRepository<EraWeaponModStatModifier> _eraWeaponModStatModifierRepository;
    private readonly IRepository<EraWeaponConfigurationSlot> _eraWeaponConfigurationSlotRepository;
    private readonly IRepository<EraWeaponConfigurationModPool> _eraWeaponConfigurationModPoolRepository;
    private readonly IRepository<EraWeaponInstanceModCandidate> _eraWeaponInstanceModCandidateRepository;
    private readonly IRepository<EraWeaponInstanceModSelection> _eraWeaponInstanceModSelectionRepository;
    private readonly IRepository<EraEquipmentConfiguration> _eraEquipmentConfigurationRepository;
    private readonly IRepository<EraEquipmentInstance> _eraEquipmentInstanceRepository;
    private readonly IRepository<EraRewardEvent> _eraRewardEventRepository;
    private readonly IRepository<EraRewardConfiguration> _eraRewardConfigurationRepository;
    private readonly IRepository<EraWeaponSlotType> _eraWeaponSlotTypeRepository;
    private readonly IRepository<EraWeaponSlotConfiguration> _eraWeaponSlotConfigurationRepository;
    private readonly IRepository<EraWeaponFramework> _eraWeaponFrameworkRepository;
    private readonly IRepository<EraWeaponFrameworkStat> _eraWeaponFrameworkStatRepository;
    private readonly IRepository<EraWeaponFrameworkCapability> _eraWeaponFrameworkCapabilityRepository;
    private readonly IMapper _mapper;

    public EraWeaponDataProvider(
        IRepository<EraWeaponTemplate> eraWeaponTemplateRepository,
        IRepository<EraWeaponTemplateStat> eraWeaponTemplateStatRepository,
        IRepository<EraWeaponModDefinition> eraWeaponModDefinitionRepository,
        IRepository<EraWeaponModStatModifier> eraWeaponModStatModifierRepository,
        IRepository<EraWeaponConfigurationSlot> eraWeaponConfigurationSlotRepository,
        IRepository<EraWeaponConfigurationModPool> eraWeaponConfigurationModPoolRepository,
        IRepository<EraWeaponInstanceModCandidate> eraWeaponInstanceModCandidateRepository,
        IRepository<EraWeaponInstanceModSelection> eraWeaponInstanceModSelectionRepository,
        IRepository<EraEquipmentConfiguration> eraEquipmentConfigurationRepository,
        IRepository<EraEquipmentInstance> eraEquipmentInstanceRepository,
        IRepository<EraRewardEvent> eraRewardEventRepository,
        IRepository<EraRewardConfiguration> eraRewardConfigurationRepository,
        IRepository<EraWeaponSlotType> eraWeaponSlotTypeRepository,
        IRepository<EraWeaponSlotConfiguration> eraWeaponSlotConfigurationRepository,
        IRepository<EraWeaponFramework> eraWeaponFrameworkRepository,
        IRepository<EraWeaponFrameworkStat> eraWeaponFrameworkStatRepository,
        IRepository<EraWeaponFrameworkCapability> eraWeaponFrameworkCapabilityRepository,
        IMapper mapper)
    {
        _eraWeaponTemplateRepository = eraWeaponTemplateRepository;
        _eraWeaponTemplateStatRepository = eraWeaponTemplateStatRepository;
        _eraWeaponModDefinitionRepository = eraWeaponModDefinitionRepository;
        _eraWeaponModStatModifierRepository = eraWeaponModStatModifierRepository;
        _eraWeaponConfigurationSlotRepository = eraWeaponConfigurationSlotRepository;
        _eraWeaponConfigurationModPoolRepository = eraWeaponConfigurationModPoolRepository;
        _eraWeaponInstanceModCandidateRepository = eraWeaponInstanceModCandidateRepository;
        _eraWeaponInstanceModSelectionRepository = eraWeaponInstanceModSelectionRepository;
        _eraEquipmentConfigurationRepository = eraEquipmentConfigurationRepository;
        _eraEquipmentInstanceRepository = eraEquipmentInstanceRepository;
        _eraRewardEventRepository = eraRewardEventRepository;
        _eraRewardConfigurationRepository = eraRewardConfigurationRepository;
        _eraWeaponSlotTypeRepository = eraWeaponSlotTypeRepository;
        _eraWeaponSlotConfigurationRepository = eraWeaponSlotConfigurationRepository;
        _eraWeaponFrameworkRepository = eraWeaponFrameworkRepository;
        _eraWeaponFrameworkStatRepository = eraWeaponFrameworkStatRepository;
        _eraWeaponFrameworkCapabilityRepository = eraWeaponFrameworkCapabilityRepository;
        _mapper = mapper;
    }

    public async Task<EraWeaponCatalogDto> GetCatalogAsync(CancellationToken cancellationToken)
    {
        var templates = await _eraWeaponTemplateRepository.QueryNoTracking().OrderBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var templateStats = await _eraWeaponTemplateStatRepository.QueryNoTracking().OrderBy(row => row.Stat).ToListAsync(cancellationToken).ConfigureAwait(false);
        var definitions = await _eraWeaponModDefinitionRepository.QueryNoTracking().OrderBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var modifiers = await _eraWeaponModStatModifierRepository.QueryNoTracking().OrderBy(row => row.Sequence).ToListAsync(cancellationToken).ConfigureAwait(false);
        var slots = await _eraWeaponConfigurationSlotRepository.QueryNoTracking().OrderBy(row => row.DisplayOrder).ThenBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var pools = await _eraWeaponConfigurationModPoolRepository.QueryNoTracking().OrderBy(row => row.ModDefinitionId).ToListAsync(cancellationToken).ConfigureAwait(false);
        var slotTypes = await _eraWeaponSlotTypeRepository.QueryNoTracking().OrderBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var configurations = await _eraWeaponSlotConfigurationRepository.QueryNoTracking().OrderBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var frameworks = await _eraWeaponFrameworkRepository.QueryNoTracking().OrderBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var frameworkStats = await _eraWeaponFrameworkStatRepository.QueryNoTracking().OrderBy(row => row.DisplayOrder).ThenBy(row => row.Stat).ToListAsync(cancellationToken).ConfigureAwait(false);
        var capabilities = await _eraWeaponFrameworkCapabilityRepository.QueryNoTracking().OrderBy(row => row.CapabilityKind).ToListAsync(cancellationToken).ConfigureAwait(false);
        return EraWeaponDataMapping.CreateCatalog(slotTypes, configurations, templates, definitions, modifiers, slots, pools,
            frameworks, frameworkStats, capabilities, templateStats, _mapper);
    }

    public async Task<Dictionary<Guid, EraWeaponInstanceDto>> GetInstancesAsync(IEnumerable<Guid> equipmentIds, CancellationToken cancellationToken)
    {
        var ids = equipmentIds.Distinct().ToArray();
        if (ids.Length == 0) return new Dictionary<Guid, EraWeaponInstanceDto>();
        var candidates = await _eraWeaponInstanceModCandidateRepository.QueryNoTracking().Where(row => ids.Contains(row.EquipmentInstanceId))
            .OrderBy(row => row.DisplayOrder).ThenBy(row => row.CandidateId).ToListAsync(cancellationToken).ConfigureAwait(false);
        var selections = await _eraWeaponInstanceModSelectionRepository.QueryNoTracking().Where(row => ids.Contains(row.EquipmentInstanceId))
            .OrderBy(row => row.DisplayOrder).ThenBy(row => row.SlotId).ToListAsync(cancellationToken).ConfigureAwait(false);
        return EraWeaponDataMapping.CreateInstances(candidates, selections, _mapper, ids);
    }

    public async Task<EraEquipmentConfiguration?> LockPublicationAsync(CancellationToken cancellationToken)
    {
        _eraWeaponTemplateRepository.RequireTransaction();
        var id = Void.Bot.Message.Constants.ERA.EraEquipmentConstants.ConfigurationId;
        var rows = await _eraEquipmentConfigurationRepository.QuerySql(
            $"SELECT * FROM era_equipment_configuration WHERE id = {id.ToString()} FOR UPDATE")
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public Task<List<EraEquipmentInstance>> GetReferencedWeaponsAsync(EraEquipmentReferenceChanges changes, CancellationToken cancellationToken)
    {
        var candidates = _eraWeaponInstanceModCandidateRepository.QueryNoTracking();
        var slots = _eraWeaponInstanceModSelectionRepository.QueryNoTracking();
        return _eraEquipmentInstanceRepository.QueryNoTracking().Where(row => row.EquipmentType == EraEquipmentType.Weapon &&
            (changes.TemplateIds.Contains(row.TemplateId) ||
                candidates.Any(candidate => candidate.EquipmentInstanceId == row.Id &&
                    changes.ModificationIds.Contains(candidate.ModDefinitionId)) ||
                slots.Any(slot => slot.EquipmentInstanceId == row.Id &&
                    (changes.SlotTypeIds.Contains(slot.SlotTypeId) || changes.SlotIds.Contains(slot.SlotId)))))
            .ToListAsync(cancellationToken);
    }

    public Task<List<string>> GetPendingRewardPlansAsync(CancellationToken cancellationToken) =>
        _eraRewardEventRepository.QueryNoTracking().Where(row => row.ResultPayload == null)
            .Select(row => row.PlanPayload).ToListAsync(cancellationToken);

    public Task<string?> GetRewardConfigurationPayloadAsync(CancellationToken cancellationToken) =>
        _eraRewardConfigurationRepository.QueryNoTracking()
            .Where(row => row.Id == Void.Bot.Message.Constants.ERA.EraRewardConstants.ConfigurationId)
            .Select(row => row.Payload).SingleOrDefaultAsync(cancellationToken);

    public async Task SaveCatalogAsync(EraWeaponCatalogDto previous, EraWeaponCatalogDto catalog, CancellationToken cancellationToken)
    {
        _eraWeaponTemplateRepository.RequireTransaction();
        var changedFrameworks = EraEquipmentCatalogChanges.ChangedIds(previous.Frameworks, catalog.Frameworks, row => row.Id);
        var changedFrameworkStats = EraEquipmentCatalogChanges.ChangedIds(previous.Frameworks, catalog.Frameworks, row => row.Id, row => row.SupplementaryStats);
        var changedCapabilities = EraEquipmentCatalogChanges.ChangedIds(previous.Frameworks, catalog.Frameworks, row => row.Id, row => row.Capabilities);
        if (changedFrameworkStats.Count > 0)
            await _eraWeaponFrameworkStatRepository.ExecuteDeleteAsync(row => changedFrameworkStats.Contains(row.FrameworkId), cancellationToken).ConfigureAwait(false);
        if (changedCapabilities.Count > 0)
            await _eraWeaponFrameworkCapabilityRepository.ExecuteDeleteAsync(row => changedCapabilities.Contains(row.FrameworkId), cancellationToken).ConfigureAwait(false);
        var frameworks = changedFrameworks.Count == 0 ? new Dictionary<Guid, EraWeaponFramework>() :
            await _eraWeaponFrameworkRepository.Query().Where(row => changedFrameworks.Contains(row.Id)).ToDictionaryAsync(row => row.Id, cancellationToken).ConfigureAwait(false);
        foreach (var entity in frameworks.Values.Where(row => !catalog.Frameworks.Any(framework => framework.Id == row.Id)))
            _eraWeaponFrameworkRepository.Delete(entity);
        foreach (var framework in catalog.Frameworks.Where(row => changedFrameworks.Contains(row.Id)))
        {
            if (!frameworks.TryGetValue(framework.Id, out var entity))
            {
                entity = new EraWeaponFramework { Id = framework.Id };
                _eraWeaponFrameworkRepository.Insert(entity);
            }
            _mapper.Map(framework, entity);
            if (changedFrameworkStats.Contains(framework.Id))
                for (var index = 0; index < framework.SupplementaryStats.Count; index++)
                    _eraWeaponFrameworkStatRepository.Insert(new EraWeaponFrameworkStat
                    {
                        FrameworkId = framework.Id, Stat = framework.SupplementaryStats[index], DisplayOrder = index
                    });
            if (changedCapabilities.Contains(framework.Id))
                foreach (var capability in framework.Capabilities)
                    _eraWeaponFrameworkCapabilityRepository.Insert(new EraWeaponFrameworkCapability
                    {
                        FrameworkId = framework.Id, CapabilityKind = capability.Kind, ConfigurationPayload = JsonSerializer.Serialize(capability)
                    });
        }
        var changedTypes = EraEquipmentCatalogChanges.ChangedIds(previous.SlotTypes, catalog.SlotTypes, row => row.Id);
        var changedConfigurations = EraEquipmentCatalogChanges.ChangedIds(previous.SlotConfigurations, catalog.SlotConfigurations, row => row.Id);
        var changedTemplates = EraEquipmentCatalogChanges.ChangedIds(previous.Templates, catalog.Templates, row => row.Id);
        var changedTemplateStats = EraEquipmentCatalogChanges.ChangedIds(previous.Templates, catalog.Templates, row => row.Id, row => row.BaseStats);
        if (changedTemplateStats.Count > 0)
            await _eraWeaponTemplateStatRepository.ExecuteDeleteAsync(row => changedTemplateStats.Contains(row.TemplateId), cancellationToken).ConfigureAwait(false);
        var changedModifications = EraEquipmentCatalogChanges.ChangedIds(previous.Modifications, catalog.Modifications, row => row.Id);
        var changedModifiers = EraEquipmentCatalogChanges.ChangedIds(previous.Modifications, catalog.Modifications, row => row.Id, row => row.Modifiers);
        var previousSlots = previous.SlotConfigurations.SelectMany(configuration => configuration.Slots.Select(slot => new { configuration.Id, Slot = slot })).ToList();
        var currentSlots = catalog.SlotConfigurations.SelectMany(configuration => configuration.Slots.Select(slot => new { configuration.Id, Slot = slot })).ToList();
        var changedSlots = EraEquipmentCatalogChanges.ChangedIds(previousSlots, currentSlots, row => row.Slot.Id);
        // 立即删除仅涉及变化的子记录；后续异常由外层事务回滚。
        if (changedSlots.Count > 0)
        {
            await _eraWeaponConfigurationModPoolRepository.ExecuteDeleteAsync(row => changedSlots.Contains(row.SlotId), cancellationToken).ConfigureAwait(false);
            await _eraWeaponConfigurationSlotRepository.ExecuteDeleteAsync(row => changedSlots.Contains(row.Id), cancellationToken).ConfigureAwait(false);
        }
        if (changedModifiers.Count > 0)
            await _eraWeaponModStatModifierRepository.ExecuteDeleteAsync(row => changedModifiers.Contains(row.ModDefinitionId), cancellationToken).ConfigureAwait(false);
        var types = changedTypes.Count == 0 ? new Dictionary<Guid, EraWeaponSlotType>() :
            await _eraWeaponSlotTypeRepository.Query().Where(row => changedTypes.Contains(row.Id)).ToDictionaryAsync(row => row.Id, cancellationToken).ConfigureAwait(false);
        var configurations = changedConfigurations.Count == 0 ? new Dictionary<Guid, EraWeaponSlotConfiguration>() :
            await _eraWeaponSlotConfigurationRepository.Query().Where(row => changedConfigurations.Contains(row.Id)).ToDictionaryAsync(row => row.Id, cancellationToken).ConfigureAwait(false);
        foreach (var entity in types.Values.Where(row => !catalog.SlotTypes.Any(type => type.Id == row.Id))) _eraWeaponSlotTypeRepository.Delete(entity);
        foreach (var type in catalog.SlotTypes.Where(row => changedTypes.Contains(row.Id)))
        {
            if (!types.TryGetValue(type.Id, out var entity))
            {
                entity = new EraWeaponSlotType { Id = type.Id };
                _eraWeaponSlotTypeRepository.Insert(entity);
            }
            _mapper.Map(type, entity);
        }
        foreach (var entity in configurations.Values.Where(row => !catalog.SlotConfigurations.Any(configuration => configuration.Id == row.Id)))
            _eraWeaponSlotConfigurationRepository.Delete(entity);
        foreach (var configuration in catalog.SlotConfigurations.Where(row => changedConfigurations.Contains(row.Id)))
        {
            if (!configurations.TryGetValue(configuration.Id, out var entity))
            {
                entity = new EraWeaponSlotConfiguration { Id = configuration.Id };
                _eraWeaponSlotConfigurationRepository.Insert(entity);
            }
            _mapper.Map(configuration, entity);
        }
        foreach (var configuration in catalog.SlotConfigurations)
        {
            foreach (var slot in configuration.Slots.Where(row => changedSlots.Contains(row.Id)))
            {
                _eraWeaponConfigurationSlotRepository.Insert(new EraWeaponConfigurationSlot
                {
                    Id = slot.Id, ConfigurationId = configuration.Id, Name = slot.Name, SlotTypeId = slot.SlotTypeId,
                    CandidateCount = slot.CandidateCount, DisplayOrder = slot.DisplayOrder
                });
                foreach (var entry in slot.Pool) _eraWeaponConfigurationModPoolRepository.Insert(new EraWeaponConfigurationModPool
                {
                    SlotId = slot.Id, ModDefinitionId = entry.ModDefinitionId, Weight = entry.Weight
                });
            }
        }
        var templates = changedTemplates.Count == 0 ? new Dictionary<Guid, EraWeaponTemplate>() :
            await _eraWeaponTemplateRepository.Query().Where(row => changedTemplates.Contains(row.Id)).ToDictionaryAsync(row => row.Id, cancellationToken).ConfigureAwait(false);
        var modifications = changedModifications.Count == 0 ? new Dictionary<Guid, EraWeaponModDefinition>() :
            await _eraWeaponModDefinitionRepository.Query().Where(row => changedModifications.Contains(row.Id)).ToDictionaryAsync(row => row.Id, cancellationToken).ConfigureAwait(false);
        foreach (var entity in templates.Values.Where(row => !catalog.Templates.Any(template => template.Id == row.Id))) _eraWeaponTemplateRepository.Delete(entity);
        foreach (var entity in modifications.Values.Where(row => !catalog.Modifications.Any(modification => modification.Id == row.Id))) _eraWeaponModDefinitionRepository.Delete(entity);
        foreach (var template in catalog.Templates.Where(row => changedTemplates.Contains(row.Id)))
        {
            if (!templates.TryGetValue(template.Id, out var entity))
            {
                entity = new EraWeaponTemplate { Id = template.Id };
                _eraWeaponTemplateRepository.Insert(entity);
            }
            entity.Name = template.Name; entity.Icon = template.Icon; entity.Note = template.Note;
            entity.IsGenerationEnabled = template.IsGenerationEnabled; entity.PresentationKey = template.PresentationKey;
            entity.FrameworkId = template.FrameworkId;
            entity.SlotConfigurationId = template.SlotConfigurationId;
            if (changedTemplateStats.Contains(template.Id))
                foreach (var stat in template.BaseStats)
                    _eraWeaponTemplateStatRepository.Insert(new EraWeaponTemplateStat
                    {
                        TemplateId = template.Id, Stat = stat.Stat, Value = stat.Value,
                        EffectAtZero = stat.EffectAtZero, EffectAtHundred = stat.EffectAtHundred
                    });
            entity.CompatibleSlotsPayload = JsonSerializer.Serialize(template.CompatibleSlots);
        }
        foreach (var modification in catalog.Modifications.Where(row => changedModifications.Contains(row.Id)))
        {
            if (!modifications.TryGetValue(modification.Id, out var entity))
            {
                entity = new EraWeaponModDefinition { Id = modification.Id };
                _eraWeaponModDefinitionRepository.Insert(entity);
            }
            entity.Name = modification.Name; entity.SlotTypeId = modification.SlotTypeId; entity.Description = modification.Description; entity.Icon = modification.Icon;
            if (!changedModifiers.Contains(modification.Id)) continue;
            for (var index = 0; index < modification.Modifiers.Count; index++)
            {
                var modifier = modification.Modifiers[index];
                _eraWeaponModStatModifierRepository.Insert(new EraWeaponModStatModifier
                {
                    ModDefinitionId = modification.Id, Sequence = index, Stat = modifier.Stat, Operation = modifier.Operation, Value = modifier.Value
                });
            }
        }
    }

    public void AddInstance(Guid equipmentId, EraWeaponInstanceDto instance)
    {
        _eraWeaponInstanceModCandidateRepository.InsertMany(EraWeaponDataMapping.CreateCandidates(equipmentId, instance));
        _eraWeaponInstanceModSelectionRepository.InsertMany(EraWeaponDataMapping.CreateSelections(equipmentId, instance));
    }

    public async Task UpdateSelectionAsync(Guid equipmentId, Guid slotId, Guid previous, Guid candidate, long expectedItemVersion, CancellationToken cancellationToken)
    {
        _eraWeaponTemplateRepository.RequireTransaction();
        var changed = await _eraWeaponInstanceModSelectionRepository.ExecuteUpdateAsync(
            row => row.EquipmentInstanceId == equipmentId && row.SlotId == slotId && row.SelectedCandidateId == previous,
            setters => setters.SetProperty(row => row.SelectedCandidateId, candidate), cancellationToken).ConfigureAwait(false);
        if (changed != 1) throw new EraEquipmentConcurrencyException("Weapon selection changed during the operation.");
        var touched = await _eraEquipmentInstanceRepository.ExecuteUpdateAsync(
            row => row.Id == equipmentId && row.Version == expectedItemVersion,
            setters => setters.SetProperty(row => row.Version, expectedItemVersion + 1)
                .SetProperty(row => row.UpdatedAt, (DateTimeOffset?)DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        if (touched != 1) throw new EraEquipmentConcurrencyException("Weapon instance changed during the operation.");
    }
}
