using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Models;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA;

public partial interface IEraEquipmentDataProvider
{
    Task<EraEquipmentOperationReceipt?> GetOperationReceiptAsync(Guid playerId, Guid requestId, CancellationToken cancellationToken);
    Task<EraEquipmentConfiguration?> LockConfigurationAsync(CancellationToken cancellationToken);
    Task<EraEquipmentReadSnapshot> LockCharacterEquipmentAsync(EraCharacter character, CancellationToken cancellationToken);
    Task ApplyPositionChangesAsync(Guid characterId, Guid playerId,
        IReadOnlyList<EraEquipmentPositionChange> changes, CancellationToken cancellationToken);
    Task CreateOperationReceiptAsync(EraEquipmentOperationReceipt receipt, CancellationToken cancellationToken);
}

public sealed partial class EraEquipmentDataProvider
{
    public async Task<EraEquipmentOperationReceipt?> GetOperationReceiptAsync(
        Guid playerId, Guid requestId, CancellationToken cancellationToken)
    {
        if (_dbContext.Database.CurrentTransaction == null)
        {
            throw new InvalidOperationException("Equipment operations require the command transaction.");
        }

        // 玩家行锁串行化同玩家的装备请求；锁定读取避免复用旧的事务快照。
        var rows = await _dbContext.Set<EraEquipmentOperationReceipt>().FromSqlInterpolated(
                $"SELECT * FROM era_equipment_operation_receipt WHERE player_id = {playerId.ToString()} AND request_id = {requestId.ToString()} FOR UPDATE")
            .AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public async Task<EraEquipmentConfiguration?> LockConfigurationAsync(CancellationToken cancellationToken)
    {
        if (_dbContext.Database.CurrentTransaction == null)
        {
            throw new InvalidOperationException("Equipment configuration locks require the command transaction.");
        }

        // 共享锁固定本次兼容关系，目录更新必须等待装备操作完成。
        var rows = await _dbContext.Set<EraEquipmentConfiguration>().FromSqlInterpolated(
                $"SELECT * FROM era_equipment_configuration WHERE id = {ConfigurationId.ToString()} FOR SHARE")
            .AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public async Task<EraEquipmentReadSnapshot> LockCharacterEquipmentAsync(
        EraCharacter character, CancellationToken cancellationToken)
    {
        if (_dbContext.Database.CurrentTransaction == null)
        {
            throw new InvalidOperationException("Equipment locks require the command transaction.");
        }

        // 世界限制共享锁先于实例锁，限制声明及结束须等待本次装备事务完成。
        var worlds = await _dbContext.Set<EraWorld>().FromSqlInterpolated(
                $"SELECT world.* FROM era_world AS world INNER JOIN era_world_member AS member ON member.world_id = world.id WHERE member.active_character_id = {character.Id.ToString()} AND member.is_continuing = 1 AND world.state IN (1, 2, 3) FOR SHARE")
            .AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        var world = worlds.SingleOrDefault();
        // 调用方已先锁玩家、角色；按 ID 顺序锁本角色物品，再读取其不可变护甲数据。
        var equipment = await _dbContext.Set<EraEquipmentInstance>().FromSqlInterpolated(
                $"SELECT * FROM era_equipment_instance WHERE character_id = {character.Id.ToString()} AND (location = {(int)EraEquipmentLocation.Inventory} OR location = {(int)EraEquipmentLocation.Equipped}) ORDER BY id FOR UPDATE")
            .AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        var armor = await _dbContext.Set<EraArmorInstanceData>().FromSqlInterpolated(
                $"SELECT armor.* FROM era_armor_instance_data AS armor INNER JOIN era_equipment_instance AS equipment ON armor.equipment_id = equipment.id WHERE equipment.character_id = {character.Id.ToString()} AND (equipment.location = {(int)EraEquipmentLocation.Inventory} OR equipment.location = {(int)EraEquipmentLocation.Equipped}) ORDER BY armor.equipment_id FOR UPDATE")
            .AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        var armorById = armor.ToDictionary(item => item.EquipmentId);

        return new EraEquipmentReadSnapshot
        {
            CharacterId = character.Id,
            PlayerId = character.PlayerId,
            IsCharacterDisabled = character.IsDisabled,
            EquipmentVersion = character.EquipmentVersion,
            EquippedOperationRestriction = world == null ? null : new EraEquippedOperationRestrictionDto
            {
                WorldInstanceId = world.Id, WorldVersion = world.Version,
                RestrictionVersion = world.RestrictionVersion, IsRestricted = world.IsEquippedOperationsRestricted
            },
            Entries = equipment.Select(item => new EraEquipmentReadEntry(item,
                armorById.GetValueOrDefault(item.Id))).ToList()
        };
    }

    public async Task ApplyPositionChangesAsync(Guid characterId, Guid playerId,
        IReadOnlyList<EraEquipmentPositionChange> changes, CancellationToken cancellationToken)
    {
        if (_dbContext.Database.CurrentTransaction == null)
        {
            throw new InvalidOperationException("Equipment updates require the command transaction.");
        }

        var updatedAt = DateTimeOffset.UtcNow;
        // 替换时调用方先传旧装备回包，再传候选穿上，避免同槽唯一索引的瞬时冲突。
        foreach (var change in changes)
        {
            var affected = await _dbContext.Set<EraEquipmentInstance>()
                .Where(equipment => equipment.Id == change.EquipmentId &&
                    equipment.CharacterId == characterId && equipment.OwnerPlayerId == playerId &&
                    equipment.InventorySlot == change.InventorySlot && equipment.Version == change.ExpectedVersion &&
                    equipment.Location == change.PreviousLocation && equipment.EquippedSlot == change.PreviousEquippedSlot)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(equipment => equipment.Location, change.Location)
                    .SetProperty(equipment => equipment.EquippedSlot, change.EquippedSlot)
                    .SetProperty(equipment => equipment.Version, change.ExpectedVersion + 1)
                    .SetProperty(equipment => equipment.UpdatedAt, (DateTimeOffset?)updatedAt), cancellationToken)
                .ConfigureAwait(false);
            if (affected != 1)
            {
                throw new DbUpdateConcurrencyException("Equipment position changed during the operation.");
            }
        }
    }

    public async Task CreateOperationReceiptAsync(EraEquipmentOperationReceipt receipt, CancellationToken cancellationToken)
    {
        await _dbContext.Set<EraEquipmentOperationReceipt>().AddAsync(receipt, cancellationToken).ConfigureAwait(false);
    }
}
