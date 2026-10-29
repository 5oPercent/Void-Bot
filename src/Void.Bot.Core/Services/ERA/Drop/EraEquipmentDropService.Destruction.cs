using System.Net;
using System.Text.Json;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Drop;

public sealed partial class EraEquipmentDropService
{
    public async Task<DestroyEraEquipmentDropsResponse> DestroyAsync(DestroyEraEquipmentDropsCommand command, CancellationToken cancellationToken)
    {
        if (command.WorldInstanceId == Guid.Empty || command.CharacterId == Guid.Empty || command.RequestId == Guid.Empty ||
            command.ExpectedEquipmentVersion < 0 || command.Items == null || command.Items.Count == 0 ||
            command.Items.Any(item => item == null || item.DropId == Guid.Empty || item.ExpectedDropVersion < 1 || item.ExpectedItemVersion < 1) ||
            command.Items.Select(item => item.DropId).Distinct().Count() != command.Items.Count)
            throw Error(HttpStatusCode.BadRequest, "era.drop_invalid_operation", text => text.InvalidOperation);

        var character = await _characterContext.RequireOwnedAsync(command.CharacterId, true, cancellationToken).ConfigureAwait(false);
        var items = command.Items.OrderBy(item => item.DropId).ToList();
        var payload = JsonSerializer.Serialize(new
        {
            SchemaVersion = 1, Operation = EraEquipmentOperation.DestroyGround, command.WorldInstanceId,
            command.CharacterId, command.ExpectedEquipmentVersion, Items = items
        });
        var existing = await _equipment.GetOperationReceiptAsync(character.PlayerId, command.RequestId, cancellationToken).ConfigureAwait(false);
        if (existing != null)
        {
            if (existing.Operation != EraEquipmentOperation.DestroyGround || existing.CharacterId != character.Id || existing.RequestPayload != payload)
                throw Error(HttpStatusCode.Conflict, "era.drop_idempotency_conflict", text => text.IdempotencyConflict);
            EraEquipmentOperationReceiptDto? previous;
            try { previous = JsonSerializer.Deserialize<EraEquipmentOperationReceiptDto>(existing.ResultPayload); }
            catch (JsonException) { previous = null; }
            if (previous?.State == null || previous.RequestId != command.RequestId || previous.CharacterId != character.Id ||
                previous.Operation != EraEquipmentOperation.DestroyGround || previous.State.CharacterId != character.Id ||
                previous.PreviousEquipmentVersion != command.ExpectedEquipmentVersion || previous.PreviousEquipmentVersion == long.MaxValue ||
                previous.State.EquipmentVersion != previous.PreviousEquipmentVersion + 1 || previous.DestroyedDrops == null ||
                previous.DestroyedDrops.Count != items.Count || previous.DestroyedDrops.Any(drop => drop == null || drop.EquipmentId == Guid.Empty ||
                    drop.WorldInstanceId != command.WorldInstanceId) ||
                previous.DestroyedDrops.Select(drop => drop.DropId).Distinct().Count() != items.Count ||
                items.Any(item => item.ExpectedDropVersion == long.MaxValue ||
                    !previous.DestroyedDrops.Any(drop => drop.DropId == item.DropId && drop.DropVersion == item.ExpectedDropVersion + 1)))
                throw Error(HttpStatusCode.ServiceUnavailable, "era.drop_data_unavailable", text => text.DataUnavailable);
            return new DestroyEraEquipmentDropsResponse { Code = HttpStatusCode.OK, Data = previous, IsReplay = true };
        }
        if (character.IsDisabled)
            throw Error(HttpStatusCode.Forbidden, "era.drop_character_unavailable", text => text.CharacterUnavailable);
        if (character.EquipmentVersion != command.ExpectedEquipmentVersion)
            throw Error(HttpStatusCode.Conflict, "era.drop_version_conflict", text => text.VersionConflict);

        var world = await LockActiveWorldAsync(command.WorldInstanceId, character, cancellationToken).ConfigureAwait(false);
        var drops = new List<(EraEquipmentDrop Drop, EraEquipmentShare? Share)>();
        foreach (var selected in items)
        {
            var drop = await _drops.LockDropAsync(selected.DropId, cancellationToken).ConfigureAwait(false);
            var share = drop == null ? null : await _drops.LockShareAsync(drop.EquipmentId, cancellationToken).ConfigureAwait(false);
            if (drop == null || drop.WorldInstanceId != world.Id || !CanPickup(drop, share, character.PlayerId, _clock.GetUtcNow()))
                throw Error(HttpStatusCode.NotFound, "era.drop_not_found", text => text.NotFound);
            if (drop.Status != EraEquipmentDropStatus.Available || drop.Version != selected.ExpectedDropVersion)
                throw Error(HttpStatusCode.Conflict, "era.drop_version_conflict", text => text.VersionConflict);
            drops.Add((drop, share));
        }
        var snapshot = await _equipment.LockCharacterEquipmentAsync(character, cancellationToken).ConfigureAwait(false);
        var equipment = new List<EraEquipmentInstance>();
        for (int index = 0; index < drops.Count; index++)
        {
            var (drop, share) = drops[index];
            var entry = await _drops.LockEquipmentAsync(drop.EquipmentId, cancellationToken).ConfigureAwait(false);
            if (entry == null || !IsValidEntry(new Models.EraEquipmentDropReadEntry(drop, entry.Equipment, entry.Armor, share, entry.Weapon)))
                throw Error(HttpStatusCode.ServiceUnavailable, "era.drop_data_unavailable", text => text.DataUnavailable);
            if (entry.Equipment.Version != items[index].ExpectedItemVersion)
                throw Error(HttpStatusCode.Conflict, "era.drop_version_conflict", text => text.VersionConflict);
            if (entry.Equipment.Version == long.MaxValue || drop.Version == long.MaxValue || character.EquipmentVersion == long.MaxValue)
                throw Error(HttpStatusCode.ServiceUnavailable, "era.drop_data_unavailable", text => text.DataUnavailable);
            equipment.Add(entry.Equipment);
        }
        var manifest = await EraEquipmentService.ReadConfigurationAsync(_equipment, false, cancellationToken).ConfigureAwait(false);
        // 所有写锁取得后统一重判权限，整批任一过期或被领取都不执行销毁。
        var now = _clock.GetUtcNow();
        if (drops.Any(row => !CanPickup(row.Drop, row.Share, character.PlayerId, now)))
            throw Error(HttpStatusCode.NotFound, "era.drop_not_found", text => text.NotFound);
        snapshot.EquipmentVersion = character.EquipmentVersion + 1;
        var state = EraEquipmentService.CreateState(snapshot, manifest, null);
        if (state == null)
            throw Error(HttpStatusCode.ServiceUnavailable, "era.drop_data_unavailable", text => text.DataUnavailable);

        var destroyed = new List<EraEquipmentDropDestroyedDto>();
        for (int index = 0; index < drops.Count; index++)
        {
            var drop = drops[index].Drop;
            equipment[index].Location = EraEquipmentLocation.Destroyed;
            equipment[index].Version++;
            drop.Status = EraEquipmentDropStatus.Destroyed;
            drop.ActiveEquipmentId = null;
            drop.Version++;
            destroyed.Add(new EraEquipmentDropDestroyedDto
            {
                WorldInstanceId = world.Id, DropId = drop.Id, EquipmentId = drop.EquipmentId, DropVersion = drop.Version
            });
        }
        var receipt = new EraEquipmentOperationReceiptDto
        {
            RequestId = command.RequestId, CharacterId = character.Id, Operation = EraEquipmentOperation.DestroyGround,
            PreviousEquipmentVersion = character.EquipmentVersion, State = state, DestroyedDrops = destroyed
        };
        await _equipment.CreateOperationReceiptAsync(new EraEquipmentOperationReceipt
        {
            Id = Guid.NewGuid(), PlayerId = character.PlayerId, CharacterId = character.Id, RequestId = command.RequestId,
            Operation = EraEquipmentOperation.DestroyGround, RequestPayload = payload, ResultPayload = JsonSerializer.Serialize(receipt), CreatedAt = now
        }, cancellationToken).ConfigureAwait(false);
        character.EquipmentVersion = state.EquipmentVersion;
        if (_notifications != null)
            await _notifications.EnqueueAsync(character.PlayerId, character.Id, state.EquipmentVersion, command.RequestId, cancellationToken).ConfigureAwait(false);
        if (now > world.LastActiveAt) world.LastActiveAt = now;
        world.TimeoutObservedAt = null;
        return new DestroyEraEquipmentDropsResponse { Code = HttpStatusCode.OK, Data = receipt };
    }
}
