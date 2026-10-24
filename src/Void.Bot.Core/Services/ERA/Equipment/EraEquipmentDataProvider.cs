using AutoMapper;
using Void.Bot.Core.Mappings;
using Void.Bot.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Equipment.Models;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment;

public partial interface IEraEquipmentDataProvider : IScope
{
    Task<EraWeaponCatalogDto> GetWeaponCatalogAsync(CancellationToken cancellationToken);
    Task<EraEquipmentConfiguration?> GetConfigurationAsync(CancellationToken cancellationToken);
    Task<EraEquipmentReadSnapshot?> GetCharacterSnapshotAsync(
        Guid characterId, Guid playerId, CancellationToken cancellationToken);
}

/// <summary>装备查询与位置变更的数据访问；Request 不产生写入，事务由工作单元提交。</summary>
public sealed partial class EraEquipmentDataProvider : IEraEquipmentDataProvider
{
    private static readonly Guid ConfigurationId = Void.Bot.Message.Constants.ERA.EraEquipmentConstants.ConfigurationId;
    private readonly IRepository<EraEquipmentConfiguration> _eraEquipmentConfigurationRepository;
    private readonly IRepository<EraCharacter> _eraCharacterRepository;
    private readonly IRepository<EraEquipmentInstance> _eraEquipmentInstanceRepository;
    private readonly IRepository<EraArmorInstanceData> _eraArmorInstanceDataRepository;
    private readonly IRepository<EraWorldMember> _eraWorldMemberRepository;
    private readonly IRepository<EraWorld> _eraWorldRepository;
    private readonly IRepository<EraPlayerAccountLink> _eraPlayerAccountLinkRepository;
    private readonly IRepository<EraEquipmentOperationReceipt> _eraEquipmentOperationReceiptRepository;
    private readonly IRepository<EraWeaponTemplate> _eraWeaponTemplateRepository;
    private readonly IRepository<EraWeaponStatEffectProfile> _eraWeaponStatEffectProfileRepository;
    private readonly IRepository<EraWeaponModDefinition> _eraWeaponModDefinitionRepository;
    private readonly IRepository<EraWeaponModStatModifier> _eraWeaponModStatModifierRepository;
    private readonly IRepository<EraWeaponTemplateModSlot> _eraWeaponTemplateModSlotRepository;
    private readonly IRepository<EraWeaponTemplateModPool> _eraWeaponTemplateModPoolRepository;
    private readonly IRepository<EraWeaponModCandidate> _eraWeaponModCandidateRepository;
    private readonly IRepository<EraWeaponModSelection> _eraWeaponModSelectionRepository;
    private readonly IMapper _mapper;

    public EraEquipmentDataProvider(
        IRepository<EraEquipmentConfiguration> eraEquipmentConfigurationRepository,
        IRepository<EraCharacter> eraCharacterRepository,
        IRepository<EraEquipmentInstance> eraEquipmentInstanceRepository,
        IRepository<EraArmorInstanceData> eraArmorInstanceDataRepository,
        IRepository<EraWorldMember> eraWorldMemberRepository,
        IRepository<EraWorld> eraWorldRepository,
        IRepository<EraPlayerAccountLink> eraPlayerAccountLinkRepository,
        IRepository<EraEquipmentOperationReceipt> eraEquipmentOperationReceiptRepository,
        IRepository<EraWeaponTemplate> eraWeaponTemplateRepository,
        IRepository<EraWeaponStatEffectProfile> eraWeaponStatEffectProfileRepository,
        IRepository<EraWeaponModDefinition> eraWeaponModDefinitionRepository,
        IRepository<EraWeaponModStatModifier> eraWeaponModStatModifierRepository,
        IRepository<EraWeaponTemplateModSlot> eraWeaponTemplateModSlotRepository,
        IRepository<EraWeaponTemplateModPool> eraWeaponTemplateModPoolRepository,
        IRepository<EraWeaponModCandidate> eraWeaponModCandidateRepository,
        IRepository<EraWeaponModSelection> eraWeaponModSelectionRepository,
        IMapper mapper)
    {
        _eraEquipmentConfigurationRepository = eraEquipmentConfigurationRepository;
        _eraCharacterRepository = eraCharacterRepository;
        _eraEquipmentInstanceRepository = eraEquipmentInstanceRepository;
        _eraArmorInstanceDataRepository = eraArmorInstanceDataRepository;
        _eraWorldMemberRepository = eraWorldMemberRepository;
        _eraWorldRepository = eraWorldRepository;
        _eraPlayerAccountLinkRepository = eraPlayerAccountLinkRepository;
        _eraEquipmentOperationReceiptRepository = eraEquipmentOperationReceiptRepository;
        _eraWeaponTemplateRepository = eraWeaponTemplateRepository;
        _eraWeaponStatEffectProfileRepository = eraWeaponStatEffectProfileRepository;
        _eraWeaponModDefinitionRepository = eraWeaponModDefinitionRepository;
        _eraWeaponModStatModifierRepository = eraWeaponModStatModifierRepository;
        _eraWeaponTemplateModSlotRepository = eraWeaponTemplateModSlotRepository;
        _eraWeaponTemplateModPoolRepository = eraWeaponTemplateModPoolRepository;
        _eraWeaponModCandidateRepository = eraWeaponModCandidateRepository;
        _eraWeaponModSelectionRepository = eraWeaponModSelectionRepository;
        _mapper = mapper;
    }

    public async Task<EraWeaponCatalogDto> GetWeaponCatalogAsync(CancellationToken cancellationToken)
    {
        var templates = await _eraWeaponTemplateRepository.QueryNoTracking().OrderBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var profiles = await _eraWeaponStatEffectProfileRepository.QueryNoTracking().OrderBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var definitions = await _eraWeaponModDefinitionRepository.QueryNoTracking().OrderBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var modifiers = await _eraWeaponModStatModifierRepository.QueryNoTracking().OrderBy(row => row.Sequence).ToListAsync(cancellationToken).ConfigureAwait(false);
        var slots = await _eraWeaponTemplateModSlotRepository.QueryNoTracking().OrderBy(row => row.DisplayOrder).ThenBy(row => row.Slot).ToListAsync(cancellationToken).ConfigureAwait(false);
        var pools = await _eraWeaponTemplateModPoolRepository.QueryNoTracking().OrderBy(row => row.ModDefinitionId).ToListAsync(cancellationToken).ConfigureAwait(false);
        return EraWeaponDataMapping.CreateCatalog(templates, profiles, definitions, modifiers, slots, pools, _mapper);
    }

    public async Task<EraEquipmentConfiguration?> GetConfigurationAsync(CancellationToken cancellationToken)
    {
        return await _eraEquipmentConfigurationRepository.QueryNoTracking()
            .SingleOrDefaultAsync(configuration => configuration.Id == ConfigurationId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<EraEquipmentReadSnapshot?> GetCharacterSnapshotAsync(
        Guid characterId, Guid playerId, CancellationToken cancellationToken)
    {
        // 一条 SQL 同时读取状态版本、两种位置与保存属性；LEFT JOIN 保留真正的空背包。
        var rows = await (
            from character in _eraCharacterRepository.Query()
            where character.Id == characterId && character.PlayerId == playerId
            join equipment in _eraEquipmentInstanceRepository.Query().Where(equipment =>
                equipment.Location == EraEquipmentLocation.Inventory || equipment.Location == EraEquipmentLocation.Equipped)
                on (Guid?)character.Id equals equipment.CharacterId into equipmentGroup
            from equipment in equipmentGroup.DefaultIfEmpty()
            join armor in _eraArmorInstanceDataRepository.Query()
                on equipment!.Id equals armor.EquipmentId into armorGroup
            from armor in armorGroup.DefaultIfEmpty()
            join member in _eraWorldMemberRepository.Query().Where(member => member.IsContinuing)
                on (Guid?)character.Id equals member.ActiveCharacterId into memberGroup
            from member in memberGroup.DefaultIfEmpty()
            join world in _eraWorldRepository.Query().Where(world => world.State == EraWorldState.Building ||
                    world.State == EraWorldState.Ready || world.State == EraWorldState.Ending)
                on member!.WorldId equals world.Id into worldGroup
            from world in worldGroup.DefaultIfEmpty()
            select new
            {
                character.Id,
                character.PlayerId,
                character.IsDisabled,
                character.EquipmentVersion,
                Equipment = equipment,
                Armor = armor,
                World = world
            }).AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);

        if (rows.Count == 0)
        {
            return null;
        }

        var first = rows[0];
        var weaponInstances = await ReadWeaponInstancesAsync(rows.Where(row => row.Equipment?.EquipmentType == EraEquipmentType.Weapon)
                .Select(row => row.Equipment!.Id), cancellationToken).ConfigureAwait(false);
        var currentVersion = await _eraCharacterRepository.QueryNoTracking().Where(row => row.Id == characterId)
            .Select(row => row.EquipmentVersion).SingleAsync(cancellationToken).ConfigureAwait(false);
        if (currentVersion != first.EquipmentVersion) throw new EraEquipmentConcurrencyException("Equipment changed while reading its weapon selections.");
        return new EraEquipmentReadSnapshot
        {
            CharacterId = first.Id,
            PlayerId = first.PlayerId,
            IsCharacterDisabled = first.IsDisabled,
            EquipmentVersion = first.EquipmentVersion,
            EquippedOperationRestriction = first.World == null ? null : new EraEquippedOperationRestrictionDto
            {
                WorldInstanceId = first.World.Id, WorldVersion = first.World.Version,
                RestrictionVersion = first.World.RestrictionVersion, IsRestricted = first.World.IsEquippedOperationsRestricted
            },
            Entries = rows.Where(row => row.Equipment != null)
                .Select(row => new EraEquipmentReadEntry(row.Equipment!, row.Armor, weaponInstances.GetValueOrDefault(row.Equipment!.Id))).ToList()
        };
    }

    private async Task<Dictionary<Guid, EraWeaponInstanceDto>> ReadWeaponInstancesAsync(IEnumerable<Guid> equipmentIds, CancellationToken cancellationToken)
    {
        var ids = equipmentIds.Distinct().ToArray();
        if (ids.Length == 0) return new Dictionary<Guid, EraWeaponInstanceDto>();
        var candidates = await _eraWeaponModCandidateRepository.QueryNoTracking().Where(row => ids.Contains(row.EquipmentId))
            .OrderBy(row => row.DisplayOrder).ThenBy(row => row.CandidateId).ToListAsync(cancellationToken).ConfigureAwait(false);
        var selections = await _eraWeaponModSelectionRepository.QueryNoTracking().Where(row => ids.Contains(row.EquipmentId))
            .OrderBy(row => row.Slot).ToListAsync(cancellationToken).ConfigureAwait(false);
        return EraWeaponDataMapping.CreateInstances(candidates, selections, _mapper);
    }
}
