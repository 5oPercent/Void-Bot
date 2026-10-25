using System.Net;
using System.Text.Json;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Resources.i18n.ERA;
using Void.Bot.Core.Services.ERA.Equipment.Models;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment;

public partial interface IEraEquipmentService
{
    Task<DestroyEraInventoryEquipmentResponse> DestroyInventoryAsync(DestroyEraInventoryEquipmentCommand command, CancellationToken cancellationToken);
}

public sealed partial class EraEquipmentService
{
    public async Task<DestroyEraInventoryEquipmentResponse> DestroyInventoryAsync(
        DestroyEraInventoryEquipmentCommand command, CancellationToken cancellationToken)
    {
        if (command.RequestId == Guid.Empty || command.CharacterId == Guid.Empty || command.ExpectedEquipmentVersion < 0 ||
            command.Items == null || command.Items.Count is < 1 or > EraEquipmentRules.InventoryCapacity ||
            command.Items.Any(item => item == null || item.EquipmentId == Guid.Empty || item.ExpectedItemVersion < 1) ||
            command.Items.Select(item => item.EquipmentId).Distinct().Count() != command.Items.Count)
        {
            return Error<DestroyEraInventoryEquipmentResponse>(HttpStatusCode.BadRequest, "era.equipment_invalid_operation", text => text.InvalidOperation);
        }

        var character = await _characterContext.RequireOwnedAsync(command.CharacterId, true, cancellationToken).ConfigureAwait(false);
        // 选择顺序不影响请求身份，重试仍必须保留同一批实例与版本。
        var items = command.Items.OrderBy(item => item.EquipmentId).ToList();
        var requestPayload = JsonSerializer.Serialize(new
        {
            SchemaVersion = 1, Operation = EraEquipmentOperation.Destroy,
            command.CharacterId, command.ExpectedEquipmentVersion, Items = items
        });
        var receipt = await _dataProvider.GetOperationReceiptAsync(character.PlayerId, command.RequestId, cancellationToken).ConfigureAwait(false);
        if (receipt != null)
        {
            if (receipt.PlayerId != character.PlayerId || receipt.CharacterId != character.Id || receipt.Operation != EraEquipmentOperation.Destroy ||
                !string.Equals(receipt.RequestPayload, requestPayload, StringComparison.Ordinal))
                return Error<DestroyEraInventoryEquipmentResponse>(HttpStatusCode.Conflict, "era.equipment_idempotency_conflict", text => text.IdempotencyConflict);

            EraEquipmentOperationReceiptDto? previous;
            try { previous = JsonSerializer.Deserialize<EraEquipmentOperationReceiptDto>(receipt.ResultPayload); }
            catch (JsonException) { previous = null; }
            if (previous?.State == null || previous.RequestId != command.RequestId || previous.CharacterId != character.Id ||
                previous.Operation != EraEquipmentOperation.Destroy || previous.State.CharacterId != character.Id ||
                previous.PreviousEquipmentVersion != command.ExpectedEquipmentVersion || previous.PreviousEquipmentVersion == long.MaxValue ||
                previous.State.EquipmentVersion != previous.PreviousEquipmentVersion + 1)
                return Error<DestroyEraInventoryEquipmentResponse>(HttpStatusCode.ServiceUnavailable, "era.equipment_data_unavailable", text => text.DataUnavailable);

            return new DestroyEraInventoryEquipmentResponse { Code = HttpStatusCode.OK, Data = previous, IsReplay = true };
        }

        if (character.IsDisabled)
            return Error<DestroyEraInventoryEquipmentResponse>(HttpStatusCode.Forbidden, "era.equipment_character_disabled", text => text.CharacterDisabled);
        if (character.EquipmentVersion != command.ExpectedEquipmentVersion)
            return Error<DestroyEraInventoryEquipmentResponse>(HttpStatusCode.Conflict, "era.equipment_version_conflict", text => text.VersionConflict);

        // 销毁不依赖当前定义或世界已穿装备限制；沿用玩家、角色、回执、世界、实例的锁顺序。
        var manifest = await ReadConfigurationAsync(_dataProvider, false, cancellationToken).ConfigureAwait(false);
        var snapshot = await _dataProvider.LockCharacterEquipmentAsync(character, cancellationToken).ConfigureAwait(false);
        if (snapshot.CharacterId != character.Id || snapshot.PlayerId != character.PlayerId ||
            snapshot.EquipmentVersion != character.EquipmentVersion || CreateState(snapshot, manifest, null) == null)
            return Error<DestroyEraInventoryEquipmentResponse>(HttpStatusCode.ServiceUnavailable, "era.equipment_data_unavailable", text => text.DataUnavailable);

        var changes = new List<EraEquipmentPositionChange>();
        foreach (var selected in items)
        {
            var item = snapshot.Entries.SingleOrDefault(entry => entry.Equipment.Id == selected.EquipmentId)?.Equipment;
            if (item == null)
                return Error<DestroyEraInventoryEquipmentResponse>(HttpStatusCode.NotFound, "era.equipment_not_found", text => text.EquipmentNotFound);
            if (item.Location != EraEquipmentLocation.Inventory || item.Version != selected.ExpectedItemVersion)
                return Error<DestroyEraInventoryEquipmentResponse>(HttpStatusCode.Conflict, "era.equipment_version_conflict", text => text.VersionConflict);
            if (item.Version == long.MaxValue || character.EquipmentVersion == long.MaxValue)
                return Error<DestroyEraInventoryEquipmentResponse>(HttpStatusCode.ServiceUnavailable, "era.equipment_data_unavailable", text => text.DataUnavailable);
            changes.Add(new EraEquipmentPositionChange
            {
                EquipmentId = item.Id, ExpectedVersion = item.Version,
                PreviousLocation = item.Location, PreviousEquippedSlot = item.EquippedSlot,
                Location = EraEquipmentLocation.Destroyed, EquippedSlot = null
            });
        }

        var destroyedIds = items.Select(item => item.EquipmentId).ToHashSet();
        snapshot.Entries.RemoveAll(entry => destroyedIds.Contains(entry.Equipment.Id));
        snapshot.EquipmentVersion = character.EquipmentVersion + 1;
        var state = CreateState(snapshot, manifest, null);
        if (state == null)
            return Error<DestroyEraInventoryEquipmentResponse>(HttpStatusCode.ServiceUnavailable, "era.equipment_data_unavailable", text => text.DataUnavailable);
        var result = new EraEquipmentOperationReceiptDto
        {
            RequestId = command.RequestId, CharacterId = character.Id, Operation = EraEquipmentOperation.Destroy,
            PreviousEquipmentVersion = character.EquipmentVersion, State = state
        };
        var newReceipt = new EraEquipmentOperationReceipt
        {
            Id = Guid.NewGuid(), PlayerId = character.PlayerId, CharacterId = character.Id,
            RequestId = command.RequestId, Operation = EraEquipmentOperation.Destroy, RequestPayload = requestPayload,
            ResultPayload = JsonSerializer.Serialize(result), CreatedAt = DateTimeOffset.UtcNow
        };
        try
        {
            await _dataProvider.ApplyPositionChangesAsync(character.Id, character.PlayerId, changes, cancellationToken).ConfigureAwait(false);
        }
        catch (EraEquipmentConcurrencyException)
        {
            // 抛出异常使工作单元回滚整批，不能提交前面已经成功的 UPDATE。
            throw new EraEquipmentWriteConflictException(_localizationService.GetValue<EraEquipmentContextTemplate>(text => text.VersionConflict));
        }
        await _dataProvider.CreateOperationReceiptAsync(newReceipt, cancellationToken).ConfigureAwait(false);
        character.EquipmentVersion = state.EquipmentVersion;
        if (_notifications != null)
            await _notifications.EnqueueAsync(character.PlayerId, character.Id, state.EquipmentVersion, command.RequestId, cancellationToken).ConfigureAwait(false);
        return new DestroyEraInventoryEquipmentResponse { Code = HttpStatusCode.OK, Data = result };
    }
}
