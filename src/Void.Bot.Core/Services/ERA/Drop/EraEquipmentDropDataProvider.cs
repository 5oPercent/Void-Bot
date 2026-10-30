using Void.Bot.Message.Dto.ERA;
using AutoMapper;
using Void.Bot.Core.Mappings;
using Void.Bot.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Drop.Models;
using Void.Bot.Core.Services.ERA.Equipment.Models;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Drop;

public interface IEraEquipmentDropDataProvider : IScope
{
    Task<EraEquipmentDropReadSnapshot?> ReadVisibleAsync(Guid worldId, Guid playerId, CancellationToken cancellationToken);
    Task<EraEquipmentDrop?> LockDropAsync(Guid dropId, CancellationToken cancellationToken);
    Task<EraEquipmentReadEntry?> LockEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken);
    Task<EraEquipmentShare?> LockShareAsync(Guid equipmentId, CancellationToken cancellationToken);
    void Add(EraEquipmentDrop drop, EraEquipmentShare share);
    void AddDrop(EraEquipmentDrop drop);
    Task InvalidateWorldAsync(Guid worldId, CancellationToken cancellationToken);
}

public sealed class EraEquipmentDropDataProvider : IEraEquipmentDropDataProvider
{
    private readonly IRepository<EraWorld> _eraWorldRepository;
    private readonly IRepository<EraWorldMember> _eraWorldMemberRepository;
    private readonly IRepository<EraCharacter> _eraCharacterRepository;
    private readonly IRepository<EraWorldTeamState> _eraWorldTeamStateRepository;
    private readonly IRepository<EraEquipmentDrop> _eraEquipmentDropRepository;
    private readonly IRepository<EraEquipmentInstance> _eraEquipmentInstanceRepository;
    private readonly IRepository<EraArmorInstance> _eraArmorInstanceRepository;
    private readonly IRepository<EraEquipmentShare> _eraEquipmentShareRepository;
    private readonly IRepository<EraWeaponInstanceModCandidate> _eraWeaponInstanceModCandidateRepository;
    private readonly IRepository<EraWeaponInstanceModSelection> _eraWeaponInstanceModSelectionRepository;
    private readonly IRepository<EraArmorInstanceModCandidate> _eraArmorInstanceModCandidateRepository;
    private readonly IRepository<EraArmorInstanceModSelection> _eraArmorInstanceModSelectionRepository;
    private readonly IMapper _mapper;

    public EraEquipmentDropDataProvider(
        IRepository<EraWorld> eraWorldRepository,
        IRepository<EraWorldMember> eraWorldMemberRepository,
        IRepository<EraCharacter> eraCharacterRepository,
        IRepository<EraWorldTeamState> eraWorldTeamStateRepository,
        IRepository<EraEquipmentDrop> eraEquipmentDropRepository,
        IRepository<EraEquipmentInstance> eraEquipmentInstanceRepository,
        IRepository<EraArmorInstance> eraArmorInstanceRepository,
        IRepository<EraEquipmentShare> eraEquipmentShareRepository,
        IRepository<EraWeaponInstanceModCandidate> eraWeaponInstanceModCandidateRepository,
        IRepository<EraWeaponInstanceModSelection> eraWeaponInstanceModSelectionRepository,
        IRepository<EraArmorInstanceModCandidate> eraArmorInstanceModCandidateRepository,
        IRepository<EraArmorInstanceModSelection> eraArmorInstanceModSelectionRepository,
        IMapper mapper)
    {
        _eraWorldRepository = eraWorldRepository;
        _eraWorldMemberRepository = eraWorldMemberRepository;
        _eraCharacterRepository = eraCharacterRepository;
        _eraWorldTeamStateRepository = eraWorldTeamStateRepository;
        _eraEquipmentDropRepository = eraEquipmentDropRepository;
        _eraEquipmentInstanceRepository = eraEquipmentInstanceRepository;
        _eraArmorInstanceRepository = eraArmorInstanceRepository;
        _eraEquipmentShareRepository = eraEquipmentShareRepository;
        _eraWeaponInstanceModCandidateRepository = eraWeaponInstanceModCandidateRepository;
        _eraWeaponInstanceModSelectionRepository = eraWeaponInstanceModSelectionRepository;
        _mapper = mapper;
        _eraArmorInstanceModCandidateRepository = eraArmorInstanceModCandidateRepository;
        _eraArmorInstanceModSelectionRepository = eraArmorInstanceModSelectionRepository;
    }

    public async Task<EraEquipmentDropReadSnapshot?> ReadVisibleAsync(Guid worldId, Guid playerId, CancellationToken cancellationToken)
    {
        // 一条 SQL 固定角色及候选条目；分享名单与时间资格由 Service 统一检查后再返回客户端。
        var rows = await (from world in _eraWorldRepository.Query()
            where world.Id == worldId
            join member in _eraWorldMemberRepository.Query().Where(row => row.PlayerId == playerId) on world.Id equals member.WorldId
            join character in _eraCharacterRepository.Query() on new { member.CharacterId, member.PlayerId }
                equals new { CharacterId = character.Id, character.PlayerId }
            join team in _eraWorldTeamStateRepository.Query() on world.TeamId equals team.TeamId
            join drop in _eraEquipmentDropRepository.Query().Where(row => row.Status == EraEquipmentDropStatus.Available &&
                (row.PrivateOwnerPlayerId == playerId || row.PrivateOwnerPlayerId == null && row.DroppedByPlayerId != null))
                on world.Id equals drop.WorldInstanceId into dropGroup
            from drop in dropGroup.DefaultIfEmpty()
            join equipment in _eraEquipmentInstanceRepository.Query() on drop!.EquipmentInstanceId equals equipment.Id into equipmentGroup
            from equipment in equipmentGroup.DefaultIfEmpty()
            join armor in _eraArmorInstanceRepository.Query() on equipment!.Id equals armor.EquipmentInstanceId into armorGroup
            from armor in armorGroup.DefaultIfEmpty()
            join share in _eraEquipmentShareRepository.Query() on equipment!.Id equals share.EquipmentInstanceId into shareGroup
            from share in shareGroup.DefaultIfEmpty()
            orderby drop!.CreatedAt, drop.Id
            select new { World = world, Member = member, Character = character, team.ActiveWorldId, Drop = drop, Equipment = equipment, Armor = armor, Share = share })
            .AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        if (rows.Count == 0)
        {
            return null;
        }
        var first = rows[0];
        var armorInstances = await ReadArmorInstancesAsync(rows.Where(row => row.Armor != null).Select(row => row.Armor!.EquipmentInstanceId), cancellationToken).ConfigureAwait(false);
        foreach (var row in rows.Where(row => row.Armor != null)) row.Armor!.Modifications = armorInstances[row.Armor.EquipmentInstanceId];
        var weapons = await ReadWeaponInstancesAsync(rows.Where(row => row.Equipment?.EquipmentType == EraEquipmentType.Weapon)
                .Select(row => row.Equipment!.Id), cancellationToken).ConfigureAwait(false);
        return new EraEquipmentDropReadSnapshot(first.World, first.Member, first.Character, first.ActiveWorldId,
            rows.Where(row => row.Drop != null).Select(row => new EraEquipmentDropReadEntry(row.Drop!, row.Equipment, row.Armor, row.Share,
                row.Equipment == null ? null : weapons.GetValueOrDefault(row.Equipment.Id))).ToList());
    }

    public async Task<EraEquipmentDrop?> LockDropAsync(Guid dropId, CancellationToken cancellationToken)
    {
        _eraWorldRepository.RequireTransaction();
        var rows = await _eraEquipmentDropRepository.QuerySql(
            $"SELECT * FROM era_equipment_drop WHERE id = {dropId.ToString()} FOR UPDATE").ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public async Task<EraEquipmentReadEntry?> LockEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken)
    {
        _eraWorldRepository.RequireTransaction();
        var equipment = await _eraEquipmentInstanceRepository.QuerySql(
            $"SELECT * FROM era_equipment_instance WHERE id = {equipmentId.ToString()} FOR UPDATE").ToListAsync(cancellationToken).ConfigureAwait(false);
        var armor = await _eraArmorInstanceRepository.QuerySql(
            $"SELECT * FROM era_armor_instance WHERE equipment_instance_id = {equipmentId.ToString()} FOR UPDATE")
            .AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        var armorInstances = await ReadArmorInstancesAsync(armor.Select(row => row.EquipmentInstanceId), cancellationToken).ConfigureAwait(false);
        foreach (var row in armor) row.Modifications = armorInstances[row.EquipmentInstanceId];
        var weapons = await ReadWeaponInstancesAsync(equipment.Where(row => row.EquipmentType == EraEquipmentType.Weapon).Select(row => row.Id), cancellationToken).ConfigureAwait(false);
        return equipment.Count == 0 ? null : new EraEquipmentReadEntry(equipment.Single(), armor.SingleOrDefault(), weapons.GetValueOrDefault(equipmentId));
    }

    public async Task<EraEquipmentShare?> LockShareAsync(Guid equipmentId, CancellationToken cancellationToken)
    {
        _eraWorldRepository.RequireTransaction();
        var rows = await _eraEquipmentShareRepository.QuerySql(
                $"SELECT * FROM era_equipment_share WHERE equipment_instance_id = {equipmentId.ToString()} FOR SHARE")
            .AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public void AddDrop(EraEquipmentDrop drop)
    {
        _eraEquipmentDropRepository.Insert(drop);
    }

    public void Add(EraEquipmentDrop drop, EraEquipmentShare share)
    {
        _eraEquipmentDropRepository.Insert(drop);
        _eraEquipmentShareRepository.Insert(share);
    }

    public async Task InvalidateWorldAsync(Guid worldId, CancellationToken cancellationToken)
    {
        _eraWorldRepository.RequireTransaction();
        // 调用方已持有队伍和世界排他锁；拾取及奖励生成也取得该锁。
        var drops = await _eraEquipmentDropRepository.QuerySql(
            $"SELECT * FROM era_equipment_drop WHERE world_instance_id = {worldId.ToString()} AND status = {(int)EraEquipmentDropStatus.Available} ORDER BY id FOR UPDATE")
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var equipment = await _eraEquipmentInstanceRepository.QuerySql(
            $"SELECT equipment.* FROM era_equipment_instance AS equipment INNER JOIN era_equipment_drop AS drop_row ON drop_row.equipment_instance_id = equipment.id WHERE drop_row.world_instance_id = {worldId.ToString()} AND drop_row.status = {(int)EraEquipmentDropStatus.Available} ORDER BY equipment.id FOR UPDATE")
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var byId = equipment.ToDictionary(item => item.Id);
        foreach (var drop in drops)
        {
            if (!byId.TryGetValue(drop.EquipmentInstanceId, out var item) || item.Location != EraEquipmentLocation.Ground ||
                item.OwnerPlayerId.HasValue || item.CharacterId.HasValue || item.EquippedSlot.HasValue ||
                drop.ActiveEquipmentInstanceId != item.Id || drop.Version < 1 || drop.Version == long.MaxValue || item.Version < 1 || item.Version == long.MaxValue)
            {
                throw new EraEquipmentConcurrencyException("Unpicked drop and equipment state are inconsistent.");
            }
        }
        foreach (var drop in drops)
        {
            var item = byId[drop.EquipmentInstanceId];
            item.Location = EraEquipmentLocation.Destroyed;
            item.Version++;
            drop.Status = EraEquipmentDropStatus.Invalidated;
            drop.ActiveEquipmentInstanceId = null;
            drop.Version++;
        }
    }

    private async Task<Dictionary<Guid, EraWeaponInstanceDto>> ReadWeaponInstancesAsync(IEnumerable<Guid> equipmentIds, CancellationToken cancellationToken)
    {
        var ids = equipmentIds.Distinct().ToArray();
        if (ids.Length == 0) return new Dictionary<Guid, EraWeaponInstanceDto>();
        var candidates = await _eraWeaponInstanceModCandidateRepository.QueryNoTracking().Where(row => ids.Contains(row.EquipmentInstanceId))
            .OrderBy(row => row.DisplayOrder).ThenBy(row => row.CandidateId).ToListAsync(cancellationToken).ConfigureAwait(false);
        var selections = await _eraWeaponInstanceModSelectionRepository.QueryNoTracking().Where(row => ids.Contains(row.EquipmentInstanceId))
            .OrderBy(row => row.DisplayOrder).ThenBy(row => row.SlotId).ToListAsync(cancellationToken).ConfigureAwait(false);
        return EraWeaponDataMapping.CreateInstances(candidates, selections, _mapper, ids);
    }

    private async Task<Dictionary<Guid, EraArmorInstanceDto>> ReadArmorInstancesAsync(IEnumerable<Guid> equipmentIds, CancellationToken cancellationToken)
    {
        var ids = equipmentIds.Distinct().ToArray();
        if (ids.Length == 0) return new Dictionary<Guid, EraArmorInstanceDto>();
        var candidates = await _eraArmorInstanceModCandidateRepository.QueryNoTracking().Where(row => ids.Contains(row.EquipmentInstanceId))
            .OrderBy(row => row.DisplayOrder).ThenBy(row => row.CandidateId).ToListAsync(cancellationToken).ConfigureAwait(false);
        var selections = await _eraArmorInstanceModSelectionRepository.QueryNoTracking().Where(row => ids.Contains(row.EquipmentInstanceId))
            .OrderBy(row => row.DisplayOrder).ThenBy(row => row.SlotId).ToListAsync(cancellationToken).ConfigureAwait(false);
        return EraArmorDataMapping.CreateInstances(candidates, selections, _mapper, ids);
    }
}
