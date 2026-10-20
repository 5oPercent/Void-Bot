using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Resources.i18n.ERA;
using Void.Bot.Core.Services.ERA.Models;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA;

public partial interface IEraEquipmentService
{
    Task<EquipEraEquipmentResponse> EquipAsync(EquipEraEquipmentCommand command, CancellationToken cancellationToken);
    Task<UnequipEraEquipmentResponse> UnequipAsync(UnequipEraEquipmentCommand command, CancellationToken cancellationToken);
}

public sealed partial class EraEquipmentService
{
    public async Task<EquipEraEquipmentResponse> EquipAsync(EquipEraEquipmentCommand command, CancellationToken cancellationToken)
    {
        return await ExecuteOperationAsync<EquipEraEquipmentResponse>(command, EraEquipmentOperation.Equip, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<UnequipEraEquipmentResponse> UnequipAsync(UnequipEraEquipmentCommand command, CancellationToken cancellationToken)
    {
        return await ExecuteOperationAsync<UnequipEraEquipmentResponse>(command, EraEquipmentOperation.Unequip, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<T> ExecuteOperationAsync<T>(
        IEraEquipmentOperation command, EraEquipmentOperation operation, CancellationToken cancellationToken)
        where T : EraEquipmentOperationResponse, new()
    {
        var equip = command as EquipEraEquipmentCommand;
        if (command.RequestId == Guid.Empty || command.CharacterId == Guid.Empty || command.EquipmentId == Guid.Empty ||
            command.ExpectedEquipmentVersion < 0 || command.ExpectedItemVersion < 1 || !System.Enum.IsDefined(command.Slot) ||
            (equip != null && (equip.ExpectedManifestVersion < 1 ||
                equip.ExpectedEquippedEquipmentId.HasValue != equip.ExpectedEquippedVersion.HasValue ||
                equip.ExpectedEquippedEquipmentId == Guid.Empty || equip.ExpectedEquippedVersion is <= 0)))
        {
            return Error<T>(HttpStatusCode.BadRequest, "era.equipment_invalid_operation", text => text.InvalidOperation);
        }

        // 与角色禁用保持一致：玩家 -> 角色 -> 回执 -> 目录 -> 世界限制共享锁 -> 实例 -> 属性。
        var character = await _characterContext.RequireOwnedAsync(command.CharacterId, true, cancellationToken).ConfigureAwait(false);
        var requestPayload = JsonSerializer.Serialize(new
        {
            SchemaVersion = 1,
            Operation = operation,
            command.CharacterId,
            command.EquipmentId,
            command.Slot,
            command.ExpectedEquipmentVersion,
            command.ExpectedItemVersion,
            ExpectedEquippedEquipmentId = equip?.ExpectedEquippedEquipmentId,
            ExpectedEquippedVersion = equip?.ExpectedEquippedVersion,
            ExpectedManifestVersion = equip?.ExpectedManifestVersion
        });
        var receipt = await _dataProvider.GetOperationReceiptAsync(character.PlayerId, command.RequestId, cancellationToken)
            .ConfigureAwait(false);
        if (receipt != null)
        {
            if (receipt.PlayerId != character.PlayerId || receipt.CharacterId != character.Id || receipt.Operation != operation ||
                !string.Equals(receipt.RequestPayload, requestPayload, StringComparison.Ordinal))
            {
                return Error<T>(HttpStatusCode.Conflict, "era.equipment_idempotency_conflict", text => text.IdempotencyConflict);
            }

            EraEquipmentOperationReceiptDto? previous;
            try
            {
                previous = JsonSerializer.Deserialize<EraEquipmentOperationReceiptDto>(receipt.ResultPayload);
            }
            catch (JsonException)
            {
                previous = null;
            }

            if (previous == null || previous.State == null || previous.RequestId != command.RequestId ||
                previous.CharacterId != character.Id || previous.Operation != operation ||
                previous.State.CharacterId != character.Id || previous.PreviousEquipmentVersion < 0 ||
                previous.PreviousEquipmentVersion == long.MaxValue ||
                previous.State.EquipmentVersion != previous.PreviousEquipmentVersion + 1)
            {
                return Error<T>(HttpStatusCode.ServiceUnavailable, "era.equipment_data_unavailable", text => text.DataUnavailable);
            }

            // 历史成功只返回原回执，不要求旧版本仍是当前状态，也不再次变更资产。
            return new T { Code = HttpStatusCode.OK, Data = previous, IsReplay = true };
        }

        if (character.IsDisabled)
        {
            return Error<T>(HttpStatusCode.Forbidden, "era.equipment_character_disabled", text => text.CharacterDisabled);
        }

        if (character.EquipmentVersion != command.ExpectedEquipmentVersion)
        {
            return Error<T>(HttpStatusCode.Conflict, "era.equipment_version_conflict", text => text.VersionConflict);
        }

        // 卸下无需当前定义仍存在；装配必须固定并校验目录版本与兼容关系。
        var configuration = equip != null
            ? await _dataProvider.LockConfigurationAsync(cancellationToken).ConfigureAwait(false)
            : await _dataProvider.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var manifest = DecodeConfiguration(configuration);
        if (equip != null)
        {
            if (manifest == null)
            {
                return Error<T>(HttpStatusCode.ServiceUnavailable, "era.equipment_config_unavailable", text => text.ConfigUnavailable);
            }

            if (manifest.Version != equip.ExpectedManifestVersion)
            {
                return Error<T>(HttpStatusCode.Conflict, "era.equipment_config_version_conflict", text => text.ConfigVersionConflict);
            }
        }

        var snapshot = await _dataProvider.LockCharacterEquipmentAsync(character, cancellationToken).ConfigureAwait(false);
        if (snapshot.CharacterId != character.Id || snapshot.PlayerId != character.PlayerId ||
            snapshot.EquipmentVersion != character.EquipmentVersion || CreateState(snapshot, manifest, null) == null)
        {
            return Error<T>(HttpStatusCode.ServiceUnavailable, "era.equipment_data_unavailable", text => text.DataUnavailable);
        }

        var item = snapshot.Entries.SingleOrDefault(entry => entry.Equipment.Id == command.EquipmentId)?.Equipment;
        if (item == null)
        {
            return Error<T>(HttpStatusCode.NotFound, "era.equipment_not_found", text => text.EquipmentNotFound);
        }

        var current = snapshot.Entries.SingleOrDefault(entry => entry.Equipment.Location == EraEquipmentLocation.Equipped &&
            entry.Equipment.EquippedSlot == command.Slot)?.Equipment;
        if (item.Version != command.ExpectedItemVersion ||
            (equip != null && (item.Location != EraEquipmentLocation.Inventory ||
                current?.Id != equip.ExpectedEquippedEquipmentId || current?.Version != equip.ExpectedEquippedVersion)) ||
            (equip == null && (item.Location != EraEquipmentLocation.Equipped || current?.Id != item.Id)))
        {
            return Error<T>(HttpStatusCode.Conflict, "era.equipment_version_conflict", text => text.VersionConflict);
        }

        if (equip != null)
        {
            var definition = manifest!.Definitions.SingleOrDefault(definition =>
                string.Equals(definition.Id, item.DefinitionId, StringComparison.Ordinal));
            if (definition == null)
            {
                return Error<T>(HttpStatusCode.Conflict, "era.equipment_definition_unavailable", text => text.DefinitionUnavailable);
            }

            if (!definition.CompatibleSlots.Contains(command.Slot))
            {
                return Error<T>(HttpStatusCode.BadRequest, "era.equipment_incompatible_slot", text => text.IncompatibleSlot);
            }
        }

        if (snapshot.EquippedOperationRestriction?.IsRestricted == true && current != null)
        {
            return Error<T>(HttpStatusCode.Forbidden, "era.equipment_operations_restricted", text => text.OperationsRestricted);
        }

        // 按最终分区数量检查；同类替换先减去候选再加回旧装备，不要求额外空格。
        foreach (var partition in System.Enum.GetValues<EraArmorSlot>())
        {
            var count = snapshot.Entries.Count(entry => entry.Equipment.Location == EraEquipmentLocation.Inventory &&
                entry.Equipment.InventorySlot == partition);
            if (equip != null && item.InventorySlot == partition)
            {
                count--;
            }
            var returning = equip != null ? current : item;
            if (returning?.InventorySlot == partition)
            {
                count++;
            }
            if (count > InventoryCapacityPerSlot)
            {
                return Error<T>(HttpStatusCode.Conflict, "era.equipment_inventory_full", text => text.InventoryFull);
            }
        }

        if (character.EquipmentVersion == long.MaxValue || item.Version == long.MaxValue || current?.Version == long.MaxValue)
        {
            return Error<T>(HttpStatusCode.ServiceUnavailable, "era.equipment_data_unavailable", text => text.DataUnavailable);
        }

        var changes = new List<EraEquipmentPositionChange>();
        if (equip != null && current != null)
        {
            changes.Add(new EraEquipmentPositionChange
            {
                EquipmentId = current.Id,
                InventorySlot = current.InventorySlot,
                ExpectedVersion = current.Version,
                PreviousLocation = current.Location,
                PreviousEquippedSlot = current.EquippedSlot,
                Location = EraEquipmentLocation.Inventory,
                EquippedSlot = null
            });
        }

        changes.Add(new EraEquipmentPositionChange
        {
            EquipmentId = item.Id,
            InventorySlot = item.InventorySlot,
            ExpectedVersion = item.Version,
            PreviousLocation = item.Location,
            PreviousEquippedSlot = item.EquippedSlot,
            Location = equip != null ? EraEquipmentLocation.Equipped : EraEquipmentLocation.Inventory,
            EquippedSlot = equip != null ? command.Slot : null
        });

        // 快照实例为 AsNoTracking 读取，只在内存中预先建立完整成功结果；此时还没有数据库写入。
        foreach (var change in changes)
        {
            var changed = snapshot.Entries.Single(entry => entry.Equipment.Id == change.EquipmentId).Equipment;
            changed.Location = change.Location;
            changed.EquippedSlot = change.EquippedSlot;
            changed.Version = change.ExpectedVersion + 1;
        }

        snapshot.EquipmentVersion = character.EquipmentVersion + 1;
        var state = CreateState(snapshot, manifest, null);
        if (state == null)
        {
            return Error<T>(HttpStatusCode.ServiceUnavailable, "era.equipment_data_unavailable", text => text.DataUnavailable);
        }

        var result = new EraEquipmentOperationReceiptDto
        {
            RequestId = command.RequestId,
            CharacterId = character.Id,
            Operation = operation,
            PreviousEquipmentVersion = character.EquipmentVersion,
            State = state
        };
        var newReceipt = new EraEquipmentOperationReceipt
        {
            Id = Guid.NewGuid(),
            PlayerId = character.PlayerId,
            CharacterId = character.Id,
            RequestId = command.RequestId,
            Operation = operation,
            RequestPayload = requestPayload,
            ResultPayload = JsonSerializer.Serialize(result),
            CreatedAt = DateTimeOffset.UtcNow
        };

        try
        {
            await _dataProvider.ApplyPositionChangesAsync(character.Id, character.PlayerId, changes, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException)
        {
            // 不返回失败响应，否则工作单元仍会提交已经执行的第一条 UPDATE。
            throw new EraEquipmentWriteConflictException(
                _localizationService.GetValue<EraEquipmentContextTemplate>(text => text.VersionConflict));
        }

        await _dataProvider.CreateOperationReceiptAsync(newReceipt, cancellationToken).ConfigureAwait(false);
        character.EquipmentVersion = state.EquipmentVersion;
        return new T { Code = HttpStatusCode.OK, Data = result };
    }
}
