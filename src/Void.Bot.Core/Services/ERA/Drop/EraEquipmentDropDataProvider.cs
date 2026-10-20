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
    void Add(EraEquipmentDrop drop, EraEquipmentShare share);
    Task InvalidateWorldAsync(Guid worldId, CancellationToken cancellationToken);
}

public sealed class EraEquipmentDropDataProvider : IEraEquipmentDropDataProvider
{
    private readonly VoidBotDbContext _context;

    public EraEquipmentDropDataProvider(VoidBotDbContext context)
    {
        _context = context;
    }

    public async Task<EraEquipmentDropReadSnapshot?> ReadVisibleAsync(Guid worldId, Guid playerId, CancellationToken cancellationToken)
    {
        // 一条 SQL 固定世界、本人继续状态、角色版本和私人可见条目；空掉落仍返回根记录。
        var rows = await (from world in _context.Set<EraWorld>()
            where world.Id == worldId
            join member in _context.Set<EraWorldMember>().Where(row => row.PlayerId == playerId) on world.Id equals member.WorldId
            join character in _context.Set<EraCharacter>() on new { member.CharacterId, member.PlayerId }
                equals new { CharacterId = character.Id, character.PlayerId }
            join team in _context.Set<EraWorldTeamState>() on world.TeamId equals team.TeamId
            join drop in _context.Set<EraEquipmentDrop>().Where(row => row.Status == EraEquipmentDropStatus.Available && row.PrivateOwnerPlayerId == playerId)
                on world.Id equals drop.WorldInstanceId into dropGroup
            from drop in dropGroup.DefaultIfEmpty()
            join equipment in _context.Set<EraEquipmentInstance>() on drop!.EquipmentId equals equipment.Id into equipmentGroup
            from equipment in equipmentGroup.DefaultIfEmpty()
            join armor in _context.Set<EraArmorInstanceData>() on equipment!.Id equals armor.EquipmentId into armorGroup
            from armor in armorGroup.DefaultIfEmpty()
            orderby drop!.CreatedAt, drop.Id
            select new { World = world, Member = member, Character = character, team.ActiveWorldId, Drop = drop, Equipment = equipment, Armor = armor })
            .AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        if (rows.Count == 0)
        {
            return null;
        }
        var first = rows[0];
        return new EraEquipmentDropReadSnapshot(first.World, first.Member, first.Character, first.ActiveWorldId,
            rows.Where(row => row.Drop != null).Select(row => new EraEquipmentDropReadEntry(row.Drop!, row.Equipment, row.Armor)).ToList());
    }

    public async Task<EraEquipmentDrop?> LockDropAsync(Guid dropId, CancellationToken cancellationToken)
    {
        if (_context.Database.CurrentTransaction == null)
        {
            throw new InvalidOperationException("Drop writes require the command transaction.");
        }
        var rows = await _context.Set<EraEquipmentDrop>().FromSqlInterpolated(
            $"SELECT * FROM era_equipment_drop WHERE id = {dropId.ToString()} FOR UPDATE").ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public async Task<EraEquipmentReadEntry?> LockEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken)
    {
        if (_context.Database.CurrentTransaction == null)
        {
            throw new InvalidOperationException("Drop equipment locks require the command transaction.");
        }
        var equipment = await _context.Set<EraEquipmentInstance>().FromSqlInterpolated(
            $"SELECT * FROM era_equipment_instance WHERE id = {equipmentId.ToString()} FOR UPDATE").ToListAsync(cancellationToken).ConfigureAwait(false);
        var armor = await _context.Set<EraArmorInstanceData>().FromSqlInterpolated(
            $"SELECT * FROM era_armor_instance_data WHERE equipment_id = {equipmentId.ToString()} FOR UPDATE")
            .AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        return equipment.Count == 0 ? null : new EraEquipmentReadEntry(equipment.Single(), armor.SingleOrDefault());
    }

    public void Add(EraEquipmentDrop drop, EraEquipmentShare share)
    {
        _context.Set<EraEquipmentDrop>().Add(drop);
        _context.Set<EraEquipmentShare>().Add(share);
    }

    public async Task InvalidateWorldAsync(Guid worldId, CancellationToken cancellationToken)
    {
        if (_context.Database.CurrentTransaction == null)
        {
            throw new InvalidOperationException("Drop cleanup requires the world command transaction.");
        }
        // 调用方已持有队伍和世界排他锁；拾取及奖励生成也取得该锁。
        var drops = await _context.Set<EraEquipmentDrop>().FromSqlInterpolated(
            $"SELECT * FROM era_equipment_drop WHERE world_instance_id = {worldId.ToString()} AND status = {(int)EraEquipmentDropStatus.Available} ORDER BY id FOR UPDATE")
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var equipment = await _context.Set<EraEquipmentInstance>().FromSqlInterpolated(
            $"SELECT equipment.* FROM era_equipment_instance AS equipment INNER JOIN era_equipment_drop AS drop_row ON drop_row.equipment_id = equipment.id WHERE drop_row.world_instance_id = {worldId.ToString()} AND drop_row.status = {(int)EraEquipmentDropStatus.Available} ORDER BY equipment.id FOR UPDATE")
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var byId = equipment.ToDictionary(item => item.Id);
        foreach (var drop in drops)
        {
            if (!byId.TryGetValue(drop.EquipmentId, out var item) || item.Location != EraEquipmentLocation.Ground ||
                item.OwnerPlayerId.HasValue || item.CharacterId.HasValue || item.EquippedSlot.HasValue ||
                drop.ActiveEquipmentId != item.Id || drop.Version < 1 || drop.Version == long.MaxValue || item.Version < 1 || item.Version == long.MaxValue)
            {
                throw new DbUpdateConcurrencyException("Unpicked drop and equipment state are inconsistent.");
            }
        }
        foreach (var drop in drops)
        {
            var item = byId[drop.EquipmentId];
            item.Location = EraEquipmentLocation.Destroyed;
            item.Version++;
            drop.Status = EraEquipmentDropStatus.Invalidated;
            drop.ActiveEquipmentId = null;
            drop.Version++;
        }
    }
}
