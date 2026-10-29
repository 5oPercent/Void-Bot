using Void.Bot.Message.Constants.ERA;
using AutoMapper;
using Void.Bot.Core.Mappings;
using Void.Bot.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Domain.Account;
using System.Text.Json;
using Void.Bot.Core.Services.ERA.Equipment.Models;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment;

public partial interface IEraEquipmentDataProvider : IScope
{
    Task<EraArmorCatalogDto> GetArmorCatalogAsync(CancellationToken cancellationToken);
    Task<EraWeaponCatalogDto> GetWeaponCatalogAsync(CancellationToken cancellationToken);
    Task<EraEquipmentConfiguration?> GetConfigurationAsync(CancellationToken cancellationToken);
    Task<EraEquipmentReadSnapshot?> GetCharacterSnapshotAsync(
        Guid characterId, Guid playerId, CancellationToken cancellationToken);
    Task<Dictionary<Guid, EraEquipmentShareDto>> ReadSharesAsync(IEnumerable<Guid> equipmentIds, CancellationToken cancellationToken);
}

/// <summary>装备查询与位置变更的数据访问；Request 不产生写入，事务由工作单元提交。</summary>
public sealed partial class EraEquipmentDataProvider : IEraEquipmentDataProvider
{
    private static readonly Guid ConfigurationId = EraEquipmentConstants.ConfigurationId;
    private readonly IRepository<EraEquipmentConfiguration> _eraEquipmentConfigurationRepository;
    private readonly IRepository<EraCharacter> _eraCharacterRepository;
    private readonly IRepository<EraEquipmentInstance> _eraEquipmentInstanceRepository;
    private readonly IRepository<EraArmorInstanceData> _eraArmorInstanceDataRepository;
    private readonly IRepository<EraWorldMember> _eraWorldMemberRepository;
    private readonly IRepository<EraWorld> _eraWorldRepository;
    private readonly IRepository<EraPlayerAccountLink> _eraPlayerAccountLinkRepository;
    private readonly IRepository<EraEquipmentOperationReceipt> _eraEquipmentOperationReceiptRepository;
    private readonly IRepository<EraWeaponTemplate> _eraWeaponTemplateRepository;
    private readonly IRepository<EraWeaponStatMapping> _eraWeaponStatMappingRepository;
    private readonly IRepository<EraWeaponModDefinition> _eraWeaponModDefinitionRepository;
    private readonly IRepository<EraWeaponModStatModifier> _eraWeaponModStatModifierRepository;
    private readonly IRepository<EraWeaponConfigurationSlot> _eraWeaponConfigurationSlotRepository;
    private readonly IRepository<EraWeaponConfigurationModPool> _eraWeaponConfigurationModPoolRepository;
    private readonly IRepository<EraWeaponModCandidate> _eraWeaponModCandidateRepository;
    private readonly IRepository<EraWeaponModSelection> _eraWeaponModSelectionRepository;
    private readonly IRepository<EraWeaponSlotType> _eraWeaponSlotTypeRepository;
    private readonly IRepository<EraWeaponSlotConfiguration> _eraWeaponSlotConfigurationRepository;
    private readonly IRepository<EraArmorTemplate> _eraArmorTemplateRepository;
    private readonly IRepository<EraArmorModDefinition> _eraArmorModDefinitionRepository;
    private readonly IRepository<EraArmorModStatModifier> _eraArmorModStatModifierRepository;
    private readonly IRepository<EraArmorConfigurationSlot> _eraArmorConfigurationSlotRepository;
    private readonly IRepository<EraArmorConfigurationModPool> _eraArmorConfigurationModPoolRepository;
    private readonly IRepository<EraArmorSlotType> _eraArmorSlotTypeRepository;
    private readonly IRepository<EraArmorSlotConfiguration> _eraArmorSlotConfigurationRepository;
    private readonly IRepository<EraArmorModCandidate> _eraArmorModCandidateRepository;
    private readonly IRepository<EraArmorModSelection> _eraArmorModSelectionRepository;
    private readonly IMapper _mapper;
    private readonly IRepository<EraEquipmentShare> _eraEquipmentShareRepository;
    private readonly IRepository<UserAccount> _userAccountRepository;

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
        IRepository<EraWeaponStatMapping> eraWeaponStatMappingRepository,
        IRepository<EraWeaponModDefinition> eraWeaponModDefinitionRepository,
        IRepository<EraWeaponModStatModifier> eraWeaponModStatModifierRepository,
        IRepository<EraWeaponConfigurationSlot> eraWeaponConfigurationSlotRepository,
        IRepository<EraWeaponConfigurationModPool> eraWeaponConfigurationModPoolRepository,
        IRepository<EraWeaponModCandidate> eraWeaponModCandidateRepository,
        IRepository<EraWeaponModSelection> eraWeaponModSelectionRepository,
        IRepository<EraWeaponSlotType> eraWeaponSlotTypeRepository,
        IRepository<EraWeaponSlotConfiguration> eraWeaponSlotConfigurationRepository,
        IRepository<EraArmorTemplate> eraArmorTemplateRepository,
        IRepository<EraArmorModDefinition> eraArmorModDefinitionRepository,
        IRepository<EraArmorModStatModifier> eraArmorModStatModifierRepository,
        IRepository<EraArmorConfigurationSlot> eraArmorConfigurationSlotRepository,
        IRepository<EraArmorConfigurationModPool> eraArmorConfigurationModPoolRepository,
        IRepository<EraArmorSlotType> eraArmorSlotTypeRepository,
        IRepository<EraArmorSlotConfiguration> eraArmorSlotConfigurationRepository,
        IRepository<EraArmorModCandidate> eraArmorModCandidateRepository,
        IRepository<EraArmorModSelection> eraArmorModSelectionRepository,
        IMapper mapper,
        IRepository<EraEquipmentShare> eraEquipmentShareRepository,
        IRepository<UserAccount> userAccountRepository)
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
        _eraWeaponStatMappingRepository = eraWeaponStatMappingRepository;
        _eraWeaponModDefinitionRepository = eraWeaponModDefinitionRepository;
        _eraWeaponModStatModifierRepository = eraWeaponModStatModifierRepository;
        _eraWeaponConfigurationSlotRepository = eraWeaponConfigurationSlotRepository;
        _eraWeaponConfigurationModPoolRepository = eraWeaponConfigurationModPoolRepository;
        _eraWeaponModCandidateRepository = eraWeaponModCandidateRepository;
        _eraWeaponModSelectionRepository = eraWeaponModSelectionRepository;
        _eraWeaponSlotTypeRepository = eraWeaponSlotTypeRepository;
        _eraWeaponSlotConfigurationRepository = eraWeaponSlotConfigurationRepository;
        _mapper = mapper;
        _eraEquipmentShareRepository = eraEquipmentShareRepository;
        _userAccountRepository = userAccountRepository;
        _eraArmorTemplateRepository = eraArmorTemplateRepository;
        _eraArmorModDefinitionRepository = eraArmorModDefinitionRepository;
        _eraArmorModStatModifierRepository = eraArmorModStatModifierRepository;
        _eraArmorConfigurationSlotRepository = eraArmorConfigurationSlotRepository;
        _eraArmorConfigurationModPoolRepository = eraArmorConfigurationModPoolRepository;
        _eraArmorSlotTypeRepository = eraArmorSlotTypeRepository;
        _eraArmorSlotConfigurationRepository = eraArmorSlotConfigurationRepository;
        _eraArmorModCandidateRepository = eraArmorModCandidateRepository;
        _eraArmorModSelectionRepository = eraArmorModSelectionRepository;
    }

    public async Task<EraArmorCatalogDto> GetArmorCatalogAsync(CancellationToken cancellationToken)
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

    public async Task<EraWeaponCatalogDto> GetWeaponCatalogAsync(CancellationToken cancellationToken)
    {
        var templates = await _eraWeaponTemplateRepository.QueryNoTracking().OrderBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var statMappings = await _eraWeaponStatMappingRepository.QueryNoTracking().OrderBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var definitions = await _eraWeaponModDefinitionRepository.QueryNoTracking().OrderBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var modifiers = await _eraWeaponModStatModifierRepository.QueryNoTracking().OrderBy(row => row.Sequence).ToListAsync(cancellationToken).ConfigureAwait(false);
        var slots = await _eraWeaponConfigurationSlotRepository.QueryNoTracking().OrderBy(row => row.DisplayOrder).ThenBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var pools = await _eraWeaponConfigurationModPoolRepository.QueryNoTracking().OrderBy(row => row.ModDefinitionId).ToListAsync(cancellationToken).ConfigureAwait(false);
        var slotTypes = await _eraWeaponSlotTypeRepository.QueryNoTracking().OrderBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        var configurations = await _eraWeaponSlotConfigurationRepository.QueryNoTracking().OrderBy(row => row.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
        return EraWeaponDataMapping.CreateCatalog(slotTypes, configurations, templates, statMappings, definitions, modifiers, slots, pools, _mapper);
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
        var shares = await ReadSharesAsync(rows.Where(row => row.Equipment != null).Select(row => row.Equipment!.Id), cancellationToken).ConfigureAwait(false);
        var armorInstances = await ReadArmorInstancesAsync(rows.Where(row => row.Armor != null).Select(row => row.Armor!.EquipmentId), cancellationToken).ConfigureAwait(false);
        foreach (var row in rows.Where(row => row.Armor != null)) row.Armor!.Modifications = armorInstances[row.Armor.EquipmentId];
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
                .Select(row => new EraEquipmentReadEntry(row.Equipment!, row.Armor, weaponInstances.GetValueOrDefault(row.Equipment!.Id), shares.GetValueOrDefault(row.Equipment.Id))).ToList()
        };
    }

    public async Task<Dictionary<Guid, EraEquipmentShareDto>> ReadSharesAsync(IEnumerable<Guid> equipmentIds, CancellationToken cancellationToken)
    {
        var ids = equipmentIds.Distinct().ToArray();
        var result = new Dictionary<Guid, EraEquipmentShareDto>();
        if (ids.Length == 0) return result;
        var shares = await _eraEquipmentShareRepository.QueryNoTracking().Where(row => ids.Contains(row.EquipmentId))
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        foreach (var share in shares)
        {
            try
            {
                var players = JsonSerializer.Deserialize<List<Guid>>(share.AllowedPlayerIdsPayload);
                if (share.ShareUntil == default || players == null || players.Count == 0 || players.Contains(Guid.Empty) || players.Distinct().Count() != players.Count) continue;
                result.Add(share.EquipmentId, new EraEquipmentShareDto
                {
                    ShareUntil = share.ShareUntil,
                    Members = players.Select(id => new EraEquipmentShareMemberDto { PlayerId = id }).ToList()
                });
            }
            catch (JsonException) { }
        }
        // 资格来自不可变原名单；账号改名或离队只影响展示，不扩大名单。
        var playerIds = result.Values.SelectMany(share => share.Members).Select(member => member.PlayerId).Distinct().ToArray();
        if (playerIds.Length == 0) return result;
        var names = await (from link in _eraPlayerAccountLinkRepository.QueryNoTracking()
            join account in _userAccountRepository.QueryNoTracking() on link.AccountId equals account.Id
            where playerIds.Contains(link.PlayerId)
            select new { link.PlayerId, account.Name, account.Suffix }).ToDictionaryAsync(row => row.PlayerId, cancellationToken).ConfigureAwait(false);
        foreach (var member in result.Values.SelectMany(share => share.Members))
            if (names.TryGetValue(member.PlayerId, out var name))
            {
                member.UserName = name.Name;
                member.Suffix = name.Suffix;
            }
        return result;
    }

    private async Task<Dictionary<Guid, EraWeaponInstanceDto>> ReadWeaponInstancesAsync(IEnumerable<Guid> equipmentIds, CancellationToken cancellationToken)
    {
        var ids = equipmentIds.Distinct().ToArray();
        if (ids.Length == 0) return new Dictionary<Guid, EraWeaponInstanceDto>();
        var candidates = await _eraWeaponModCandidateRepository.QueryNoTracking().Where(row => ids.Contains(row.EquipmentId))
            .OrderBy(row => row.DisplayOrder).ThenBy(row => row.CandidateId).ToListAsync(cancellationToken).ConfigureAwait(false);
        var selections = await _eraWeaponModSelectionRepository.QueryNoTracking().Where(row => ids.Contains(row.EquipmentId))
            .OrderBy(row => row.DisplayOrder).ThenBy(row => row.SlotId).ToListAsync(cancellationToken).ConfigureAwait(false);
        return EraWeaponDataMapping.CreateInstances(candidates, selections, _mapper, ids);
    }

    private async Task<Dictionary<Guid, EraArmorInstanceDto>> ReadArmorInstancesAsync(IEnumerable<Guid> equipmentIds, CancellationToken cancellationToken)
    {
        var ids = equipmentIds.Distinct().ToArray();
        if (ids.Length == 0) return new Dictionary<Guid, EraArmorInstanceDto>();
        var candidates = await _eraArmorModCandidateRepository.QueryNoTracking().Where(row => ids.Contains(row.EquipmentId))
            .OrderBy(row => row.DisplayOrder).ThenBy(row => row.CandidateId).ToListAsync(cancellationToken).ConfigureAwait(false);
        var selections = await _eraArmorModSelectionRepository.QueryNoTracking().Where(row => ids.Contains(row.EquipmentId))
            .OrderBy(row => row.DisplayOrder).ThenBy(row => row.SlotId).ToListAsync(cancellationToken).ConfigureAwait(false);
        return EraArmorDataMapping.CreateInstances(candidates, selections, _mapper, ids);
    }
}
