using Void.Bot.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Equipment.Models;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment;

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
        _eraEquipmentConfigurationRepository.RequireTransaction();

        // 玩家行锁串行化同玩家的装备请求；锁定读取避免复用旧的事务快照。
        var rows = await _eraEquipmentOperationReceiptRepository.QuerySql(
                $"SELECT * FROM era_equipment_operation_receipt WHERE player_id = {playerId.ToString()} AND request_id = {requestId.ToString()} FOR UPDATE")
            .AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public async Task<EraEquipmentConfiguration?> LockConfigurationAsync(CancellationToken cancellationToken)
    {
        _eraEquipmentConfigurationRepository.RequireTransaction();

        // 共享锁固定本次兼容关系，目录更新必须等待装备操作完成。
        var rows = await _eraEquipmentConfigurationRepository.QuerySql(
                $"SELECT * FROM era_equipment_configuration WHERE id = {ConfigurationId.ToString()} FOR SHARE")
            .AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public async Task<EraEquipmentReadSnapshot> LockCharacterEquipmentAsync(
        EraCharacter character, CancellationToken cancellationToken)
    {
        _eraEquipmentConfigurationRepository.RequireTransaction();

        // 只锁世界行：JOIN 成员若也加共享锁，会与结束事务“先锁世界、再更新成员”的顺序形成死锁。
        // 世界限制共享锁先于实例锁，限制声明及结束须等待本次装备事务完成。
        var worlds = await _eraWorldRepository.QuerySql(
                $"SELECT world.* FROM era_world AS world INNER JOIN era_world_member AS member ON member.world_id = world.id WHERE member.active_character_id = {character.Id.ToString()} AND member.is_continuing = 1 AND world.state IN (1, 2, 3) FOR SHARE OF world")
            .AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        var world = worlds.SingleOrDefault();
        // 调用方已先锁玩家、角色；按 ID 顺序锁本角色物品，再读取其不可变护甲数据。
        var equipment = await _eraEquipmentInstanceRepository.QuerySql(
                $"SELECT * FROM era_equipment_instance WHERE character_id = {character.Id.ToString()} AND (location = {(int)EraEquipmentLocation.Inventory} OR location = {(int)EraEquipmentLocation.Equipped}) ORDER BY id FOR UPDATE")
            .AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        var armor = await _eraArmorInstanceDataRepository.QuerySql(
                $"SELECT armor.* FROM era_armor_instance_data AS armor INNER JOIN era_equipment_instance AS equipment ON armor.equipment_id = equipment.id WHERE equipment.character_id = {character.Id.ToString()} AND (equipment.location = {(int)EraEquipmentLocation.Inventory} OR equipment.location = {(int)EraEquipmentLocation.Equipped}) ORDER BY armor.equipment_id FOR UPDATE")
            .AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        var armorInstances = await ReadArmorInstancesAsync(armor.Select(row => row.EquipmentId), cancellationToken).ConfigureAwait(false);
        foreach (var row in armor) row.Modifications = armorInstances[row.EquipmentId];
        var armorById = armor.ToDictionary(item => item.EquipmentId);
        var weapons = await ReadWeaponInstancesAsync(equipment.Where(row => row.EquipmentType == EraEquipmentType.Weapon)
                .Select(row => row.Id), cancellationToken).ConfigureAwait(false);

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
                armorById.GetValueOrDefault(item.Id), weapons.GetValueOrDefault(item.Id))).ToList()
        };
    }

    public async Task ApplyPositionChangesAsync(Guid characterId, Guid playerId,
        IReadOnlyList<EraEquipmentPositionChange> changes, CancellationToken cancellationToken)
    {
        _eraEquipmentConfigurationRepository.RequireTransaction();

        var updatedAt = DateTimeOffset.UtcNow;
        // 同一事务内先释放所有移动武器的槽位，避免互换／轮换触发同槽唯一索引。
        // 中间位置不提交、不生成回执、不增加版本；后续任一失败由外层工作单元整体回滚。
        var moving = changes.Where(change => change.PreviousLocation == EraEquipmentLocation.Equipped &&
            change.Location == EraEquipmentLocation.Equipped && change.PreviousEquippedSlot != change.EquippedSlot)
            .Select(change => change.EquipmentId).ToHashSet();
        foreach (var change in changes.Where(change => moving.Contains(change.EquipmentId)))
        {
            var affected = await _eraEquipmentInstanceRepository.ExecuteUpdateAsync(
                equipment => equipment.Id == change.EquipmentId && equipment.CharacterId == characterId &&
                    equipment.OwnerPlayerId == playerId && equipment.Version == change.ExpectedVersion &&
                    equipment.Location == change.PreviousLocation && equipment.EquippedSlot == change.PreviousEquippedSlot,
                setters => setters.SetProperty(equipment => equipment.Location, EraEquipmentLocation.Inventory)
                    .SetProperty(equipment => equipment.EquippedSlot, (EraEquipmentSlot?)null), cancellationToken)
                .ConfigureAwait(false);
            if (affected != 1)
                throw new EraEquipmentConcurrencyException("Equipment position changed during the operation.");
        }
        // 背包替换仍先释放旧装备；已穿移动使用上面已校验的事务内暂存位置。
        foreach (var change in changes)
        {
            var previousLocation = moving.Contains(change.EquipmentId) ? EraEquipmentLocation.Inventory : change.PreviousLocation;
            var previousSlot = moving.Contains(change.EquipmentId) ? null : change.PreviousEquippedSlot;
            var affected = await _eraEquipmentInstanceRepository.ExecuteUpdateAsync(
                equipment => equipment.Id == change.EquipmentId &&
                    equipment.CharacterId == characterId && equipment.OwnerPlayerId == playerId &&
                    equipment.Version == change.ExpectedVersion &&
                    equipment.Location == previousLocation && equipment.EquippedSlot == previousSlot,
                setters => setters
                    .SetProperty(equipment => equipment.Location, change.Location)
                    .SetProperty(equipment => equipment.EquippedSlot, change.EquippedSlot)
                    .SetProperty(equipment => equipment.CharacterId, change.Location == EraEquipmentLocation.Destroyed ? (Guid?)null : characterId)
                    .SetProperty(equipment => equipment.OwnerPlayerId, change.Location == EraEquipmentLocation.Destroyed ? (Guid?)null : playerId)
                    .SetProperty(equipment => equipment.Version, change.ExpectedVersion + 1)
                    .SetProperty(equipment => equipment.UpdatedAt, (DateTimeOffset?)updatedAt), cancellationToken)
                .ConfigureAwait(false);
            if (affected != 1)
            {
                throw new EraEquipmentConcurrencyException("Equipment position changed during the operation.");
            }
        }
    }

    public async Task CreateOperationReceiptAsync(EraEquipmentOperationReceipt receipt, CancellationToken cancellationToken)
    {
        await _eraEquipmentOperationReceiptRepository.InsertAsync(receipt, cancellationToken).ConfigureAwait(false);
    }
}
