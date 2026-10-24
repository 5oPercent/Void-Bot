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
    Task<List<EraEquipmentInstance>> GetReferencedWeaponsAsync(CancellationToken cancellationToken);
    Task<List<EraWeaponModCandidate>> GetReferencedCandidatesAsync(CancellationToken cancellationToken);
    Task<List<string>> GetPendingRewardPlansAsync(CancellationToken cancellationToken);
    Task<string?> GetRewardConfigurationPayloadAsync(CancellationToken cancellationToken);
    Task ReplaceCatalogAsync(EraWeaponCatalogDto catalog, CancellationToken cancellationToken);
    void AddInstance(Guid equipmentId, EraWeaponInstanceDto instance);
    Task UpdateSelectionAsync(Guid equipmentId, EraWeaponModSlot slot, Guid previous, Guid candidate, long expectedItemVersion, CancellationToken cancellationToken);
}

/// <summary>武器关系表访问；配置和资产写入均由调用方工作单元提交。</summary>
public sealed class EraWeaponDataProvider : IEraWeaponDataProvider
{
    private readonly IRepository<EraWeaponTemplate> _eraWeaponTemplateRepository;
    private readonly IRepository<EraWeaponStatEffectProfile> _eraWeaponStatEffectProfileRepository;
    private readonly IRepository<EraWeaponModDefinition> _eraWeaponModDefinitionRepository;
    private readonly IRepository<EraWeaponModStatModifier> _eraWeaponModStatModifierRepository;
    private readonly IRepository<EraWeaponTemplateModSlot> _eraWeaponTemplateModSlotRepository;
    private readonly IRepository<EraWeaponTemplateModPool> _eraWeaponTemplateModPoolRepository;
    private readonly IRepository<EraWeaponModCandidate> _eraWeaponModCandidateRepository;
    private readonly IRepository<EraWeaponModSelection> _eraWeaponModSelectionRepository;
    private readonly IRepository<EraEquipmentConfiguration> _eraEquipmentConfigurationRepository;
    private readonly IRepository<EraEquipmentInstance> _eraEquipmentInstanceRepository;
    private readonly IRepository<EraRewardEvent> _eraRewardEventRepository;
    private readonly IRepository<EraRewardConfiguration> _eraRewardConfigurationRepository;
    private readonly IMapper _mapper;

    public EraWeaponDataProvider(
        IRepository<EraWeaponTemplate> eraWeaponTemplateRepository,
        IRepository<EraWeaponStatEffectProfile> eraWeaponStatEffectProfileRepository,
        IRepository<EraWeaponModDefinition> eraWeaponModDefinitionRepository,
        IRepository<EraWeaponModStatModifier> eraWeaponModStatModifierRepository,
        IRepository<EraWeaponTemplateModSlot> eraWeaponTemplateModSlotRepository,
        IRepository<EraWeaponTemplateModPool> eraWeaponTemplateModPoolRepository,
        IRepository<EraWeaponModCandidate> eraWeaponModCandidateRepository,
        IRepository<EraWeaponModSelection> eraWeaponModSelectionRepository,
        IRepository<EraEquipmentConfiguration> eraEquipmentConfigurationRepository,
        IRepository<EraEquipmentInstance> eraEquipmentInstanceRepository,
        IRepository<EraRewardEvent> eraRewardEventRepository,
        IRepository<EraRewardConfiguration> eraRewardConfigurationRepository,
        IMapper mapper)
    {
        _eraWeaponTemplateRepository = eraWeaponTemplateRepository;
        _eraWeaponStatEffectProfileRepository = eraWeaponStatEffectProfileRepository;
        _eraWeaponModDefinitionRepository = eraWeaponModDefinitionRepository;
        _eraWeaponModStatModifierRepository = eraWeaponModStatModifierRepository;
        _eraWeaponTemplateModSlotRepository = eraWeaponTemplateModSlotRepository;
        _eraWeaponTemplateModPoolRepository = eraWeaponTemplateModPoolRepository;
        _eraWeaponModCandidateRepository = eraWeaponModCandidateRepository;
        _eraWeaponModSelectionRepository = eraWeaponModSelectionRepository;
        _eraEquipmentConfigurationRepository = eraEquipmentConfigurationRepository;
        _eraEquipmentInstanceRepository = eraEquipmentInstanceRepository;
        _eraRewardEventRepository = eraRewardEventRepository;
        _eraRewardConfigurationRepository = eraRewardConfigurationRepository;
        _mapper = mapper;
    }

    public async Task<EraWeaponCatalogDto> GetCatalogAsync(CancellationToken cancellationToken)
    {
        var templates = await _eraWeaponTemplateRepository.QueryNoTracking().OrderBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var profiles = await _eraWeaponStatEffectProfileRepository.QueryNoTracking().OrderBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var definitions = await _eraWeaponModDefinitionRepository.QueryNoTracking().OrderBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var modifiers = await _eraWeaponModStatModifierRepository.QueryNoTracking().OrderBy(row => row.Sequence).ToListAsync(cancellationToken).ConfigureAwait(false);
        var slots = await _eraWeaponTemplateModSlotRepository.QueryNoTracking().OrderBy(row => row.DisplayOrder).ThenBy(row => row.Slot).ToListAsync(cancellationToken).ConfigureAwait(false);
        var pools = await _eraWeaponTemplateModPoolRepository.QueryNoTracking().OrderBy(row => row.ModDefinitionId).ToListAsync(cancellationToken).ConfigureAwait(false);
        return EraWeaponDataMapping.CreateCatalog(templates, profiles, definitions, modifiers, slots, pools, _mapper);
    }

    public async Task<Dictionary<Guid, EraWeaponInstanceDto>> GetInstancesAsync(IEnumerable<Guid> equipmentIds, CancellationToken cancellationToken)
    {
        var ids = equipmentIds.Distinct().ToArray();
        if (ids.Length == 0) return new Dictionary<Guid, EraWeaponInstanceDto>();
        var candidates = await _eraWeaponModCandidateRepository.QueryNoTracking().Where(row => ids.Contains(row.EquipmentId))
            .OrderBy(row => row.DisplayOrder).ThenBy(row => row.CandidateId).ToListAsync(cancellationToken).ConfigureAwait(false);
        var selections = await _eraWeaponModSelectionRepository.QueryNoTracking().Where(row => ids.Contains(row.EquipmentId))
            .OrderBy(row => row.Slot).ToListAsync(cancellationToken).ConfigureAwait(false);
        return EraWeaponDataMapping.CreateInstances(candidates, selections, _mapper);
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

    public Task<List<EraEquipmentInstance>> GetReferencedWeaponsAsync(CancellationToken cancellationToken) =>
        _eraEquipmentInstanceRepository.QueryNoTracking().Where(row => row.EquipmentType == EraEquipmentType.Weapon)
            .ToListAsync(cancellationToken);

    public Task<List<EraWeaponModCandidate>> GetReferencedCandidatesAsync(CancellationToken cancellationToken) =>
        _eraWeaponModCandidateRepository.QueryNoTracking().ToListAsync(cancellationToken);

    public Task<List<string>> GetPendingRewardPlansAsync(CancellationToken cancellationToken) =>
        _eraRewardEventRepository.QueryNoTracking().Where(row => row.ResultPayload == null)
            .Select(row => row.PlanPayload).ToListAsync(cancellationToken);

    public Task<string?> GetRewardConfigurationPayloadAsync(CancellationToken cancellationToken) =>
        _eraRewardConfigurationRepository.QueryNoTracking()
            .Where(row => row.Id == Void.Bot.Message.Constants.ERA.EraRewardConstants.ConfigurationId)
            .Select(row => row.Payload).SingleOrDefaultAsync(cancellationToken);

    public async Task ReplaceCatalogAsync(EraWeaponCatalogDto catalog, CancellationToken cancellationToken)
    {
        _eraWeaponTemplateRepository.RequireTransaction();
        // 发布已完成全部业务校验；任何后续异常必须交给工作单元回滚这些即时写入。
        await _eraWeaponTemplateModPoolRepository.ExecuteDeleteAsync(_ => true, cancellationToken).ConfigureAwait(false);
        await _eraWeaponTemplateModSlotRepository.ExecuteDeleteAsync(_ => true, cancellationToken).ConfigureAwait(false);
        await _eraWeaponModStatModifierRepository.ExecuteDeleteAsync(_ => true, cancellationToken).ConfigureAwait(false);
        var templates = await _eraWeaponTemplateRepository.Query().ToDictionaryAsync(row => row.Id, cancellationToken).ConfigureAwait(false);
        var modifications = await _eraWeaponModDefinitionRepository.Query().ToDictionaryAsync(row => row.Id, cancellationToken).ConfigureAwait(false);
        var profiles = await _eraWeaponStatEffectProfileRepository.Query().ToDictionaryAsync(row => row.Id, cancellationToken).ConfigureAwait(false);
        foreach (var entity in profiles.Values.Where(row => !catalog.StatEffectProfiles.Any(profile => profile.Id == row.Id)))
            _eraWeaponStatEffectProfileRepository.Delete(entity);
        foreach (var profile in catalog.StatEffectProfiles)
        {
            if (!profiles.TryGetValue(profile.Id, out var entity))
            {
                entity = new EraWeaponStatEffectProfile { Id = profile.Id };
                _eraWeaponStatEffectProfileRepository.Insert(entity);
            }
            _mapper.Map(profile, entity);
        }
        foreach (var entity in templates.Values.Where(row => !catalog.Templates.Any(template => template.Id == row.Id))) _eraWeaponTemplateRepository.Delete(entity);
        foreach (var entity in modifications.Values.Where(row => !catalog.Modifications.Any(modification => modification.Id == row.Id))) _eraWeaponModDefinitionRepository.Delete(entity);
        foreach (var template in catalog.Templates)
        {
            if (!templates.TryGetValue(template.Id, out var entity))
            {
                entity = new EraWeaponTemplate { Id = template.Id };
                _eraWeaponTemplateRepository.Insert(entity);
            }
            entity.Name = template.Name; entity.Icon = template.Icon; entity.Note = template.Note;
            entity.IsGenerationEnabled = template.IsGenerationEnabled; entity.PresentationKey = template.PresentationKey;
            entity.StatEffectProfileId = template.StatEffectProfileId;
            _mapper.Map(template.BaseStats, entity);
            entity.BehaviorPayload = JsonSerializer.Serialize(template.Behavior);
            entity.CompatibleSlotsPayload = JsonSerializer.Serialize(template.CompatibleSlots);
            foreach (var slot in template.Slots)
            {
                _eraWeaponTemplateModSlotRepository.Insert(new EraWeaponTemplateModSlot { TemplateId = template.Id, Slot = slot.Slot, CandidateCount = slot.CandidateCount, DisplayOrder = slot.DisplayOrder });
                foreach (var entry in slot.Pool) _eraWeaponTemplateModPoolRepository.Insert(new EraWeaponTemplateModPool
                {
                    TemplateId = template.Id, Slot = slot.Slot, ModDefinitionId = entry.ModDefinitionId, Weight = entry.Weight
                });
            }
        }
        foreach (var modification in catalog.Modifications)
        {
            if (!modifications.TryGetValue(modification.Id, out var entity))
            {
                entity = new EraWeaponModDefinition { Id = modification.Id };
                _eraWeaponModDefinitionRepository.Insert(entity);
            }
            entity.Name = modification.Name; entity.Slot = modification.Slot; entity.Description = modification.Description; entity.Icon = modification.Icon;
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
        _eraWeaponModCandidateRepository.InsertMany(EraWeaponDataMapping.CreateCandidates(equipmentId, instance));
        _eraWeaponModSelectionRepository.InsertMany(EraWeaponDataMapping.CreateSelections(equipmentId, instance));
    }

    public async Task UpdateSelectionAsync(Guid equipmentId, EraWeaponModSlot slot, Guid previous, Guid candidate, long expectedItemVersion, CancellationToken cancellationToken)
    {
        _eraWeaponTemplateRepository.RequireTransaction();
        var changed = await _eraWeaponModSelectionRepository.ExecuteUpdateAsync(
            row => row.EquipmentId == equipmentId && row.Slot == slot && row.SelectedCandidateId == previous,
            setters => setters.SetProperty(row => row.SelectedCandidateId, candidate), cancellationToken).ConfigureAwait(false);
        if (changed != 1) throw new EraEquipmentConcurrencyException("Weapon selection changed during the operation.");
        var touched = await _eraEquipmentInstanceRepository.ExecuteUpdateAsync(
            row => row.Id == equipmentId && row.Version == expectedItemVersion,
            setters => setters.SetProperty(row => row.Version, expectedItemVersion + 1)
                .SetProperty(row => row.UpdatedAt, (DateTimeOffset?)DateTimeOffset.UtcNow), cancellationToken).ConfigureAwait(false);
        if (touched != 1) throw new EraEquipmentConcurrencyException("Weapon instance changed during the operation.");
    }
}
