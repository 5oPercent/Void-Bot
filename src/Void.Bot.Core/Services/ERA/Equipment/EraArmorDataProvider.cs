using Void.Bot.Message.Constants.ERA;
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

public interface IEraArmorDataProvider : IScope
{
    Task<EraArmorCatalogDto> GetCatalogAsync(CancellationToken cancellationToken);
    Task<Dictionary<Guid, EraArmorInstanceDto>> GetInstancesAsync(IEnumerable<Guid> equipmentIds, CancellationToken cancellationToken);
    Task<EraEquipmentConfiguration?> LockPublicationAsync(CancellationToken cancellationToken);
    Task<List<EraEquipmentInstance>> GetReferencedArmorsAsync(EraEquipmentReferenceChanges changes, CancellationToken cancellationToken);
    Task<List<string>> GetPendingRewardPlansAsync(CancellationToken cancellationToken);
    Task<string?> GetRewardConfigurationPayloadAsync(CancellationToken cancellationToken);
    Task SaveCatalogAsync(EraArmorCatalogDto previous, EraArmorCatalogDto catalog, CancellationToken cancellationToken);
    void AddInstance(Guid equipmentId, EraArmorInstanceDto instance);
    Task UpdateSelectionAsync(Guid equipmentId, Guid slotId, Guid previous, Guid candidate, long expectedItemVersion, CancellationToken cancellationToken);
}

/// <summary>护甲关系表访问；配置和资产写入均由调用方工作单元提交。</summary>
public sealed class EraArmorDataProvider : IEraArmorDataProvider
{
    private readonly IRepository<EraArmorTemplate> _eraArmorTemplateRepository;
    private readonly IRepository<EraArmorModDefinition> _eraArmorModDefinitionRepository;
    private readonly IRepository<EraArmorModStatModifier> _eraArmorModStatModifierRepository;
    private readonly IRepository<EraArmorConfigurationSlot> _eraArmorConfigurationSlotRepository;
    private readonly IRepository<EraArmorConfigurationModPool> _eraArmorConfigurationModPoolRepository;
    private readonly IRepository<EraArmorModCandidate> _eraArmorModCandidateRepository;
    private readonly IRepository<EraArmorModSelection> _eraArmorModSelectionRepository;
    private readonly IRepository<EraEquipmentConfiguration> _eraEquipmentConfigurationRepository;
    private readonly IRepository<EraEquipmentInstance> _eraEquipmentInstanceRepository;
    private readonly IRepository<EraRewardEvent> _eraRewardEventRepository;
    private readonly IRepository<EraRewardConfiguration> _eraRewardConfigurationRepository;
    private readonly IRepository<EraArmorSlotType> _eraArmorSlotTypeRepository;
    private readonly IRepository<EraArmorSlotConfiguration> _eraArmorSlotConfigurationRepository;
    private readonly IMapper _mapper;

    public EraArmorDataProvider(
        IRepository<EraArmorTemplate> eraArmorTemplateRepository,
        IRepository<EraArmorModDefinition> eraArmorModDefinitionRepository,
        IRepository<EraArmorModStatModifier> eraArmorModStatModifierRepository,
        IRepository<EraArmorConfigurationSlot> eraArmorConfigurationSlotRepository,
        IRepository<EraArmorConfigurationModPool> eraArmorConfigurationModPoolRepository,
        IRepository<EraArmorModCandidate> eraArmorModCandidateRepository,
        IRepository<EraArmorModSelection> eraArmorModSelectionRepository,
        IRepository<EraEquipmentConfiguration> eraEquipmentConfigurationRepository,
        IRepository<EraEquipmentInstance> eraEquipmentInstanceRepository,
        IRepository<EraRewardEvent> eraRewardEventRepository,
        IRepository<EraRewardConfiguration> eraRewardConfigurationRepository,
        IRepository<EraArmorSlotType> eraArmorSlotTypeRepository,
        IRepository<EraArmorSlotConfiguration> eraArmorSlotConfigurationRepository,
        IMapper mapper)
    {
        _eraArmorTemplateRepository = eraArmorTemplateRepository;
        _eraArmorModDefinitionRepository = eraArmorModDefinitionRepository;
        _eraArmorModStatModifierRepository = eraArmorModStatModifierRepository;
        _eraArmorConfigurationSlotRepository = eraArmorConfigurationSlotRepository;
        _eraArmorConfigurationModPoolRepository = eraArmorConfigurationModPoolRepository;
        _eraArmorModCandidateRepository = eraArmorModCandidateRepository;
        _eraArmorModSelectionRepository = eraArmorModSelectionRepository;
        _eraEquipmentConfigurationRepository = eraEquipmentConfigurationRepository;
        _eraEquipmentInstanceRepository = eraEquipmentInstanceRepository;
        _eraRewardEventRepository = eraRewardEventRepository;
        _eraRewardConfigurationRepository = eraRewardConfigurationRepository;
        _eraArmorSlotTypeRepository = eraArmorSlotTypeRepository;
        _eraArmorSlotConfigurationRepository = eraArmorSlotConfigurationRepository;
        _mapper = mapper;
    }

    public async Task<EraArmorCatalogDto> GetCatalogAsync(CancellationToken cancellationToken)
    {
        var templates = await _eraArmorTemplateRepository.QueryNoTracking().OrderBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var definitions = await _eraArmorModDefinitionRepository.QueryNoTracking().OrderBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var modifiers = await _eraArmorModStatModifierRepository.QueryNoTracking().OrderBy(row => row.Sequence).ToListAsync(cancellationToken).ConfigureAwait(false);
        var slots = await _eraArmorConfigurationSlotRepository.QueryNoTracking().OrderBy(row => row.DisplayOrder).ThenBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var pools = await _eraArmorConfigurationModPoolRepository.QueryNoTracking().OrderBy(row => row.ModDefinitionId).ToListAsync(cancellationToken).ConfigureAwait(false);
        var slotTypes = await _eraArmorSlotTypeRepository.QueryNoTracking().OrderBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var configurations = await _eraArmorSlotConfigurationRepository.QueryNoTracking().OrderBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        return EraArmorDataMapping.CreateCatalog(slotTypes, configurations, templates, definitions, modifiers, slots, pools, _mapper);
    }

    public async Task<Dictionary<Guid, EraArmorInstanceDto>> GetInstancesAsync(IEnumerable<Guid> equipmentIds, CancellationToken cancellationToken)
    {
        var ids = equipmentIds.Distinct().ToArray();
        if (ids.Length == 0) return new Dictionary<Guid, EraArmorInstanceDto>();
        var candidates = await _eraArmorModCandidateRepository.QueryNoTracking().Where(row => ids.Contains(row.EquipmentId))
            .OrderBy(row => row.DisplayOrder).ThenBy(row => row.CandidateId).ToListAsync(cancellationToken).ConfigureAwait(false);
        var selections = await _eraArmorModSelectionRepository.QueryNoTracking().Where(row => ids.Contains(row.EquipmentId))
            .OrderBy(row => row.DisplayOrder).ThenBy(row => row.SlotId).ToListAsync(cancellationToken).ConfigureAwait(false);
        return EraArmorDataMapping.CreateInstances(candidates, selections, _mapper, ids);
    }

    public async Task<EraEquipmentConfiguration?> LockPublicationAsync(CancellationToken cancellationToken)
    {
        _eraArmorTemplateRepository.RequireTransaction();
        var id = EraEquipmentConstants.ConfigurationId;
        var rows = await _eraEquipmentConfigurationRepository.QuerySql(
            $"SELECT * FROM era_equipment_configuration WHERE id = {id.ToString()} FOR UPDATE")
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public Task<List<EraEquipmentInstance>> GetReferencedArmorsAsync(EraEquipmentReferenceChanges changes, CancellationToken cancellationToken)
    {
        var candidates = _eraArmorModCandidateRepository.QueryNoTracking();
        var slots = _eraArmorModSelectionRepository.QueryNoTracking();
        return _eraEquipmentInstanceRepository.QueryNoTracking().Where(row => row.EquipmentType == EraEquipmentType.Armor &&
            (changes.TemplateIds.Contains(row.DefinitionId) ||
                candidates.Any(candidate => candidate.EquipmentId == row.Id &&
                    changes.ModificationIds.Contains(candidate.ModDefinitionId)) ||
                slots.Any(slot => slot.EquipmentId == row.Id &&
                    (changes.SlotTypeIds.Contains(slot.SlotTypeId) || changes.SlotIds.Contains(slot.SlotId)))))
            .ToListAsync(cancellationToken);
    }

    public Task<List<string>> GetPendingRewardPlansAsync(CancellationToken cancellationToken) =>
        _eraRewardEventRepository.QueryNoTracking().Where(row => row.ResultPayload == null)
            .Select(row => row.PlanPayload).ToListAsync(cancellationToken);

    public Task<string?> GetRewardConfigurationPayloadAsync(CancellationToken cancellationToken) =>
        _eraRewardConfigurationRepository.QueryNoTracking()
            .Where(row => row.Id == EraRewardConstants.ConfigurationId)
            .Select(row => row.Payload).SingleOrDefaultAsync(cancellationToken);

    public async Task SaveCatalogAsync(EraArmorCatalogDto previous, EraArmorCatalogDto catalog, CancellationToken cancellationToken)
    {
        _eraArmorTemplateRepository.RequireTransaction();
        var changedTypes = EraEquipmentCatalogChanges.ChangedIds(previous.SlotTypes, catalog.SlotTypes, row => row.Id);
        var changedConfigurations = EraEquipmentCatalogChanges.ChangedIds(previous.SlotConfigurations, catalog.SlotConfigurations, row => row.Id);
        var changedTemplates = EraEquipmentCatalogChanges.ChangedIds(previous.Templates, catalog.Templates, row => row.Id);
        var changedModifications = EraEquipmentCatalogChanges.ChangedIds(previous.Modifications, catalog.Modifications, row => row.Id);
        var changedModifiers = EraEquipmentCatalogChanges.ChangedIds(previous.Modifications, catalog.Modifications, row => row.Id, row => row.Modifiers);
        var previousSlots = previous.SlotConfigurations.SelectMany(configuration => configuration.Slots.Select(slot => new { configuration.Id, Slot = slot })).ToList();
        var currentSlots = catalog.SlotConfigurations.SelectMany(configuration => configuration.Slots.Select(slot => new { configuration.Id, Slot = slot })).ToList();
        var changedSlots = EraEquipmentCatalogChanges.ChangedIds(previousSlots, currentSlots, row => row.Slot.Id);
        // 立即删除仅涉及变化的子记录；后续异常由外层事务回滚。
        if (changedSlots.Count > 0)
        {
            await _eraArmorConfigurationModPoolRepository.ExecuteDeleteAsync(row => changedSlots.Contains(row.SlotId), cancellationToken).ConfigureAwait(false);
            await _eraArmorConfigurationSlotRepository.ExecuteDeleteAsync(row => changedSlots.Contains(row.Id), cancellationToken).ConfigureAwait(false);
        }
        if (changedModifiers.Count > 0)
            await _eraArmorModStatModifierRepository.ExecuteDeleteAsync(row => changedModifiers.Contains(row.ModDefinitionId), cancellationToken).ConfigureAwait(false);
        var types = changedTypes.Count == 0 ? new Dictionary<Guid, EraArmorSlotType>() :
            await _eraArmorSlotTypeRepository.Query().Where(row => changedTypes.Contains(row.Id)).ToDictionaryAsync(row => row.Id, cancellationToken).ConfigureAwait(false);
        var configurations = changedConfigurations.Count == 0 ? new Dictionary<Guid, EraArmorSlotConfiguration>() :
            await _eraArmorSlotConfigurationRepository.Query().Where(row => changedConfigurations.Contains(row.Id)).ToDictionaryAsync(row => row.Id, cancellationToken).ConfigureAwait(false);
        foreach (var entity in types.Values.Where(row => !catalog.SlotTypes.Any(type => type.Id == row.Id))) _eraArmorSlotTypeRepository.Delete(entity);
        foreach (var type in catalog.SlotTypes.Where(row => changedTypes.Contains(row.Id)))
        {
            if (!types.TryGetValue(type.Id, out var entity))
            {
                entity = new EraArmorSlotType { Id = type.Id };
                _eraArmorSlotTypeRepository.Insert(entity);
            }
            _mapper.Map(type, entity);
        }
        foreach (var entity in configurations.Values.Where(row => !catalog.SlotConfigurations.Any(configuration => configuration.Id == row.Id)))
            _eraArmorSlotConfigurationRepository.Delete(entity);
        foreach (var configuration in catalog.SlotConfigurations.Where(row => changedConfigurations.Contains(row.Id)))
        {
            if (!configurations.TryGetValue(configuration.Id, out var entity))
            {
                entity = new EraArmorSlotConfiguration { Id = configuration.Id };
                _eraArmorSlotConfigurationRepository.Insert(entity);
            }
            _mapper.Map(configuration, entity);
        }
        foreach (var configuration in catalog.SlotConfigurations)
        {
            foreach (var slot in configuration.Slots.Where(row => changedSlots.Contains(row.Id)))
            {
                _eraArmorConfigurationSlotRepository.Insert(new EraArmorConfigurationSlot
                {
                    Id = slot.Id, ConfigurationId = configuration.Id, Name = slot.Name, SlotTypeId = slot.SlotTypeId,
                    CandidateCount = slot.CandidateCount, DisplayOrder = slot.DisplayOrder
                });
                foreach (var entry in slot.Pool) _eraArmorConfigurationModPoolRepository.Insert(new EraArmorConfigurationModPool
                {
                    SlotId = slot.Id, ModDefinitionId = entry.ModDefinitionId, Weight = entry.Weight
                });
            }
        }
        var templates = changedTemplates.Count == 0 ? new Dictionary<Guid, EraArmorTemplate>() :
            await _eraArmorTemplateRepository.Query().Where(row => changedTemplates.Contains(row.Id)).ToDictionaryAsync(row => row.Id, cancellationToken).ConfigureAwait(false);
        var modifications = changedModifications.Count == 0 ? new Dictionary<Guid, EraArmorModDefinition>() :
            await _eraArmorModDefinitionRepository.Query().Where(row => changedModifications.Contains(row.Id)).ToDictionaryAsync(row => row.Id, cancellationToken).ConfigureAwait(false);
        foreach (var entity in templates.Values.Where(row => !catalog.Templates.Any(template => template.Id == row.Id))) _eraArmorTemplateRepository.Delete(entity);
        foreach (var entity in modifications.Values.Where(row => !catalog.Modifications.Any(modification => modification.Id == row.Id))) _eraArmorModDefinitionRepository.Delete(entity);
        foreach (var template in catalog.Templates.Where(row => changedTemplates.Contains(row.Id)))
        {
            if (!templates.TryGetValue(template.Id, out var entity))
            {
                entity = new EraArmorTemplate { Id = template.Id };
                _eraArmorTemplateRepository.Insert(entity);
            }
            entity.Name = template.Name; entity.Icon = template.Icon; entity.Note = template.Note;
            entity.IsGenerationEnabled = template.IsGenerationEnabled;
            entity.PresentationKey = template.PresentationKey;
            entity.SlotConfigurationId = template.SlotConfigurationId;
            entity.CompatibleSlotsPayload = JsonSerializer.Serialize(template.CompatibleSlots);
        }
        foreach (var modification in catalog.Modifications.Where(row => changedModifications.Contains(row.Id)))
        {
            if (!modifications.TryGetValue(modification.Id, out var entity))
            {
                entity = new EraArmorModDefinition { Id = modification.Id };
                _eraArmorModDefinitionRepository.Insert(entity);
            }
            entity.Name = modification.Name; entity.SlotTypeId = modification.SlotTypeId; entity.Description = modification.Description; entity.Icon = modification.Icon;
            if (!changedModifiers.Contains(modification.Id)) continue;
            for (var index = 0; index < modification.Modifiers.Count; index++)
            {
                var modifier = modification.Modifiers[index];
                _eraArmorModStatModifierRepository.Insert(new EraArmorModStatModifier
                {
                    ModDefinitionId = modification.Id, Sequence = index, Stat = modifier.Stat, Operation = modifier.Operation, Value = modifier.Value
                });
            }
        }
    }

    public void AddInstance(Guid equipmentId, EraArmorInstanceDto instance)
    {
        _eraArmorModCandidateRepository.InsertMany(EraArmorDataMapping.CreateCandidates(equipmentId, instance));
        _eraArmorModSelectionRepository.InsertMany(EraArmorDataMapping.CreateSelections(equipmentId, instance));
    }

    public async Task UpdateSelectionAsync(Guid equipmentId, Guid slotId, Guid previous, Guid candidate, long expectedItemVersion, CancellationToken cancellationToken)
    {
        _eraArmorTemplateRepository.RequireTransaction();
        var changed = await _eraArmorModSelectionRepository.ExecuteUpdateAsync(
            row => row.EquipmentId == equipmentId && row.SlotId == slotId && row.SelectedCandidateId == previous,
            setters => setters.SetProperty(row => row.SelectedCandidateId, candidate), cancellationToken).ConfigureAwait(false);
        if (changed != 1) throw new EraEquipmentConcurrencyException("Armor selection changed during the operation.");
        var touched = await _eraEquipmentInstanceRepository.ExecuteUpdateAsync(
            row => row.Id == equipmentId && row.Version == expectedItemVersion,
            setters => setters.SetProperty(row => row.Version, expectedItemVersion + 1)
                .SetProperty(row => row.UpdatedAt, (DateTimeOffset?)DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        if (touched != 1) throw new EraEquipmentConcurrencyException("Armor instance changed during the operation.");
    }
}
