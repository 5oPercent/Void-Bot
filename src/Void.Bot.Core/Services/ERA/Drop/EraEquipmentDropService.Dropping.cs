using System.Net;
using System.Text.Json;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Drop.Models;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Drop;

public sealed partial class EraEquipmentDropService
{
    public async Task<DropEraInventoryEquipmentResponse> DropAsync(DropEraInventoryEquipmentCommand command, CancellationToken cancellationToken)
    {
        if (command.WorldInstanceId == Guid.Empty || command.CharacterId == Guid.Empty || command.EquipmentId == Guid.Empty ||
            command.RequestId == Guid.Empty || command.ExpectedEquipmentVersion < 0 || command.ExpectedItemVersion < 1 ||
            command.Placement == null || !double.IsFinite(command.Placement.X) || !double.IsFinite(command.Placement.Y) ||
            !double.IsFinite(command.Placement.Z) || !double.IsFinite(command.Placement.YawRadians))
        {
            throw Error(HttpStatusCode.BadRequest, "era.drop_invalid_operation", text => text.InvalidOperation);
        }
        var character = await _characterContext.RequireOwnedAsync(command.CharacterId, true, cancellationToken).ConfigureAwait(false);
        var payload = JsonSerializer.Serialize(new
        {
            SchemaVersion = 1, Operation = EraEquipmentOperation.Drop, command.WorldInstanceId, command.CharacterId, command.EquipmentId,
            command.ExpectedEquipmentVersion, command.ExpectedItemVersion, command.Placement
        });
        var existing = await _equipment.GetOperationReceiptAsync(character.PlayerId, command.RequestId, cancellationToken).ConfigureAwait(false);
        if (existing != null)
        {
            if (existing.Operation != EraEquipmentOperation.Drop || existing.CharacterId != character.Id || existing.RequestPayload != payload)
            {
                throw Error(HttpStatusCode.Conflict, "era.drop_idempotency_conflict", text => text.IdempotencyConflict);
            }
            EraEquipmentOperationReceiptDto? previous;
            try
            {
                previous = JsonSerializer.Deserialize<EraEquipmentOperationReceiptDto>(existing.ResultPayload);
            }
            catch (JsonException)
            {
                previous = null;
            }
            if (previous == null || previous.RequestId != command.RequestId || previous.CharacterId != character.Id ||
                previous.Operation != EraEquipmentOperation.Drop || previous.State == null || previous.State.CharacterId != character.Id ||
                previous.PreviousEquipmentVersion != command.ExpectedEquipmentVersion || previous.PreviousEquipmentVersion == long.MaxValue ||
                previous.State.EquipmentVersion != previous.PreviousEquipmentVersion + 1 || previous.Dropped == null ||
                previous.Dropped.WorldInstanceId != command.WorldInstanceId || previous.Dropped.EquipmentId != command.EquipmentId ||
                previous.Dropped.DropId == Guid.Empty || previous.Dropped.DropVersion != 1 || command.ExpectedItemVersion == long.MaxValue ||
                previous.Dropped.EquipmentVersion != command.ExpectedItemVersion + 1 || previous.State.InventoryItems == null ||
                previous.State.InventoryItems.Any(item => item.Id == command.EquipmentId))
            {
                throw Error(HttpStatusCode.ServiceUnavailable, "era.drop_data_unavailable", text => text.DataUnavailable);
            }
            return new DropEraInventoryEquipmentResponse { Code = HttpStatusCode.OK, Data = previous, IsReplay = true };
        }
        if (character.IsDisabled)
        {
            throw Error(HttpStatusCode.Forbidden, "era.drop_character_unavailable", text => text.CharacterUnavailable);
        }
        if (character.EquipmentVersion != command.ExpectedEquipmentVersion)
        {
            throw Error(HttpStatusCode.Conflict, "era.drop_version_conflict", text => text.VersionConflict);
        }
        var world = await LockActiveWorldAsync(command.WorldInstanceId, character, cancellationToken).ConfigureAwait(false);
        var snapshot = await _equipment.LockCharacterEquipmentAsync(character, cancellationToken).ConfigureAwait(false);
        // 先在本人快照中确定候选，不能持有世界锁后尝试锁其他玩家的物品。
        if (!snapshot.Entries.Any(entry => entry.Equipment.Id == command.EquipmentId && entry.Equipment.Location == EraEquipmentLocation.Inventory))
        {
            throw Error(HttpStatusCode.NotFound, "era.drop_not_found", text => text.NotFound);
        }
        var item = await _drops.LockEquipmentAsync(command.EquipmentId, cancellationToken).ConfigureAwait(false);
        if (item == null || item.Equipment.Location != EraEquipmentLocation.Inventory || item.Equipment.OwnerPlayerId != character.PlayerId ||
            item.Equipment.CharacterId != character.Id || item.Equipment.EquippedSlot.HasValue)
        {
            throw Error(HttpStatusCode.NotFound, "era.drop_not_found", text => text.NotFound);
        }
        var equipment = item.Equipment;
        if (equipment.Version != command.ExpectedItemVersion)
        {
            throw Error(HttpStatusCode.Conflict, "era.drop_version_conflict", text => text.VersionConflict);
        }
        var share = await _drops.LockShareAsync(equipment.Id, cancellationToken).ConfigureAwait(false);
        var players = ReadSharePlayers(share);
        if (players == null || !players.Contains(character.PlayerId))
        {
            throw Error(HttpStatusCode.Forbidden, "era.drop_share_unavailable", text => text.ShareUnavailable);
        }
        if (share!.ShareUntil - equipment.CreatedAt != TimeSpan.FromHours(1))
        {
            throw Error(HttpStatusCode.ServiceUnavailable, "era.drop_data_unavailable", text => text.DataUnavailable);
        }
        var clockNow = _clock.GetUtcNow();
        if (clockNow >= share.ShareUntil)
        {
            throw Error(HttpStatusCode.Forbidden, "era.drop_share_expired", text => text.ShareExpired);
        }
        if (equipment.Version == long.MaxValue || character.EquipmentVersion == long.MaxValue)
        {
            throw Error(HttpStatusCode.ServiceUnavailable, "era.drop_data_unavailable", text => text.DataUnavailable);
        }
        var now = new DateTimeOffset(clockNow.UtcTicks - clockNow.UtcTicks % 10, TimeSpan.Zero);
        var drop = new EraEquipmentDrop
        {
            Id = Guid.NewGuid(), EquipmentId = equipment.Id, ActiveEquipmentId = equipment.Id, WorldInstanceId = world.Id,
            DroppedByPlayerId = character.PlayerId, Status = EraEquipmentDropStatus.Available, Version = 1, CreatedAt = now,
            X = command.Placement.X, Y = command.Placement.Y, Z = command.Placement.Z, YawRadians = command.Placement.YawRadians
        };
        equipment.Location = EraEquipmentLocation.Ground;
        equipment.OwnerPlayerId = null;
        equipment.CharacterId = null;
        equipment.EquippedSlot = null;
        equipment.Version++;
        if (!IsValidEntry(new EraEquipmentDropReadEntry(drop, equipment, item.Armor, share, item.Weapon)))
        {
            throw Error(HttpStatusCode.ServiceUnavailable, "era.drop_data_unavailable", text => text.DataUnavailable);
        }
        snapshot.Entries.RemoveAll(entry => entry.Equipment.Id == equipment.Id);
        snapshot.EquipmentVersion = character.EquipmentVersion + 1;
        var manifest = await EraEquipmentService.ReadConfigurationAsync(_equipment, false, cancellationToken).ConfigureAwait(false);
        var state = EraEquipmentService.CreateState(snapshot, manifest, null);
        if (state == null)
        {
            throw Error(HttpStatusCode.ServiceUnavailable, "era.drop_data_unavailable", text => text.DataUnavailable);
        }
        var receipt = new EraEquipmentOperationReceiptDto
        {
            RequestId = command.RequestId, CharacterId = character.Id, Operation = EraEquipmentOperation.Drop,
            PreviousEquipmentVersion = character.EquipmentVersion, State = state,
            Dropped = new EraEquipmentDropCreatedDto
            {
                WorldInstanceId = world.Id, EquipmentId = equipment.Id, EquipmentVersion = equipment.Version, DropId = drop.Id,
                DropVersion = drop.Version, CreatedAt = now, ShareUntil = share.ShareUntil,
                Placement = new EraEquipmentDropPlacementDto { X = drop.X, Y = drop.Y, Z = drop.Z, YawRadians = drop.YawRadians }
            }
        };
        _drops.AddDrop(drop);
        await _equipment.CreateOperationReceiptAsync(new EraEquipmentOperationReceipt
        {
            Id = Guid.NewGuid(), PlayerId = character.PlayerId, CharacterId = character.Id, RequestId = command.RequestId,
            Operation = EraEquipmentOperation.Drop, RequestPayload = payload, ResultPayload = JsonSerializer.Serialize(receipt), CreatedAt = now
        }, cancellationToken).ConfigureAwait(false);
        character.EquipmentVersion = state.EquipmentVersion;
        if (_notifications != null)
            await _notifications.EnqueueAsync(character.PlayerId, character.Id, state.EquipmentVersion, command.RequestId, cancellationToken).ConfigureAwait(false);
        if (now > world.LastActiveAt)
        {
            world.LastActiveAt = now;
        }
        world.TimeoutObservedAt = null;
        return new DropEraInventoryEquipmentResponse { Code = HttpStatusCode.OK, Data = receipt };
    }
}
