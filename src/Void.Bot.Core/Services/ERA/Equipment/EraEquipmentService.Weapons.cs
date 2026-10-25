using System.Net;
using System.Text.Json;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment;

public partial interface IEraEquipmentService
{
    Task<SelectEraWeaponModificationResponse> SelectWeaponModificationAsync(SelectEraWeaponModificationCommand command, CancellationToken cancellationToken);
}

public sealed partial class EraEquipmentService
{
    public async Task<SelectEraWeaponModificationResponse> SelectWeaponModificationAsync(SelectEraWeaponModificationCommand command, CancellationToken cancellationToken)
    {
        if (command.CharacterId == Guid.Empty || command.EquipmentId == Guid.Empty || command.RequestId == Guid.Empty ||
            command.CandidateId == Guid.Empty || command.SlotId == Guid.Empty || command.ExpectedManifestVersion < 1 ||
            command.ExpectedEquipmentVersion < 0 || command.ExpectedEquipmentVersion == long.MaxValue ||
            command.ExpectedItemVersion < 1 || command.ExpectedItemVersion == long.MaxValue)
            return Error<SelectEraWeaponModificationResponse>(HttpStatusCode.BadRequest, "era.equipment_invalid_operation", text => text.InvalidOperation);
        var character = await _characterContext.RequireOwnedAsync(command.CharacterId, true, cancellationToken).ConfigureAwait(false);
        var payload = JsonSerializer.Serialize(command);
        var receipt = await _dataProvider.GetOperationReceiptAsync(character.PlayerId, command.RequestId, cancellationToken).ConfigureAwait(false);
        if (receipt != null)
        {
            if (receipt.PlayerId != character.PlayerId || receipt.CharacterId != character.Id || receipt.Operation != EraEquipmentOperation.SelectWeaponModification || receipt.RequestPayload != payload)
                return Error<SelectEraWeaponModificationResponse>(HttpStatusCode.Conflict, "era.equipment_idempotency_conflict", text => text.IdempotencyConflict);
            EraEquipmentOperationReceiptDto? previous;
            try { previous = JsonSerializer.Deserialize<EraEquipmentOperationReceiptDto>(receipt.ResultPayload); }
            catch (JsonException) { previous = null; }
            if (previous == null || previous.RequestId != command.RequestId || previous.CharacterId != character.Id ||
                previous.Operation != EraEquipmentOperation.SelectWeaponModification || previous.State == null || previous.State.CharacterId != character.Id ||
                previous.PreviousEquipmentVersion != command.ExpectedEquipmentVersion ||
                previous.State.EquipmentVersion < previous.PreviousEquipmentVersion ||
                previous.State.EquipmentVersion - previous.PreviousEquipmentVersion > 1)
                return Error<SelectEraWeaponModificationResponse>(HttpStatusCode.ServiceUnavailable, "era.equipment_data_unavailable", text => text.DataUnavailable);
            return new SelectEraWeaponModificationResponse { Code = HttpStatusCode.OK, Data = previous, IsReplay = true };
        }
        if (character.IsDisabled)
            return Error<SelectEraWeaponModificationResponse>(HttpStatusCode.Forbidden, "era.equipment_character_disabled", text => text.CharacterDisabled);
        if (character.EquipmentVersion != command.ExpectedEquipmentVersion)
            return Error<SelectEraWeaponModificationResponse>(HttpStatusCode.Conflict, "era.equipment_version_conflict", text => text.VersionConflict);
        var manifest = await ReadConfigurationAsync(_dataProvider, true, cancellationToken).ConfigureAwait(false);
        if (manifest == null || _weaponData == null)
            return Error<SelectEraWeaponModificationResponse>(HttpStatusCode.ServiceUnavailable, "era.equipment_config_unavailable", text => text.ConfigUnavailable);
        if (manifest.Version != command.ExpectedManifestVersion)
            return Error<SelectEraWeaponModificationResponse>(HttpStatusCode.Conflict, "era.equipment_config_version_conflict", text => text.ConfigVersionConflict);
        var snapshot = await _dataProvider.LockCharacterEquipmentAsync(character, cancellationToken).ConfigureAwait(false);
        if (snapshot.CharacterId != character.Id || snapshot.PlayerId != character.PlayerId ||
            snapshot.EquipmentVersion != character.EquipmentVersion || CreateState(snapshot, manifest, null) == null)
            return Error<SelectEraWeaponModificationResponse>(HttpStatusCode.ServiceUnavailable, "era.equipment_data_unavailable", text => text.DataUnavailable);
        var entry = snapshot.Entries.SingleOrDefault(row => row.Equipment.Id == command.EquipmentId);
        if (entry?.Equipment.EquipmentType != EraEquipmentType.Weapon || entry.Weapon == null)
            return Error<SelectEraWeaponModificationResponse>(HttpStatusCode.NotFound, "era.equipment_not_found", text => text.EquipmentNotFound);
        var slot = entry.Weapon.Slots.SingleOrDefault(row => row.SlotId == command.SlotId);
        var candidate = slot?.Candidates.SingleOrDefault(row => row.CandidateId == command.CandidateId);
        if (candidate == null || !manifest.Weapons.Modifications.Any(row => row.Id == candidate.ModDefinitionId && row.SlotTypeId == slot!.SlotTypeId))
            return Error<SelectEraWeaponModificationResponse>(HttpStatusCode.BadRequest, "era.weapon_invalid_candidate", text => text.InvalidOperation);
        if (entry.Equipment.Version != command.ExpectedItemVersion)
            return Error<SelectEraWeaponModificationResponse>(HttpStatusCode.Conflict, "era.equipment_version_conflict", text => text.VersionConflict);
        if (entry.Equipment.Location == EraEquipmentLocation.Equipped && snapshot.EquippedOperationRestriction?.IsRestricted == true)
            return Error<SelectEraWeaponModificationResponse>(HttpStatusCode.Forbidden, "era.equipment_operations_restricted", text => text.OperationsRestricted);
        var previousCandidate = slot!.SelectedCandidateId;
        var changed = previousCandidate != command.CandidateId;
        if (changed && (character.EquipmentVersion == long.MaxValue || entry.Equipment.Version == long.MaxValue))
            return Error<SelectEraWeaponModificationResponse>(HttpStatusCode.ServiceUnavailable, "era.equipment_data_unavailable", text => text.DataUnavailable);
        slot.SelectedCandidateId = command.CandidateId;
        if (changed) { entry.Equipment.Version++; snapshot.EquipmentVersion++; }
        var state = CreateState(snapshot, manifest, null);
        if (state == null)
            return Error<SelectEraWeaponModificationResponse>(HttpStatusCode.ServiceUnavailable, "era.equipment_data_unavailable", text => text.DataUnavailable);
        var result = new EraEquipmentOperationReceiptDto
        {
            RequestId = command.RequestId, CharacterId = character.Id, Operation = EraEquipmentOperation.SelectWeaponModification,
            PreviousEquipmentVersion = character.EquipmentVersion, State = state
        };
        if (changed)
        {
            try
            {
                await _weaponData.UpdateSelectionAsync(entry.Equipment.Id, command.SlotId, previousCandidate, command.CandidateId,
                    command.ExpectedItemVersion, cancellationToken).ConfigureAwait(false);
            }
            catch (EraEquipmentConcurrencyException)
            {
                throw new EraEquipmentWriteConflictException(_localizationService.GetValue<Void.Bot.Core.Resources.i18n.ERA.EraEquipmentContextTemplate>(text => text.VersionConflict));
            }
        }
        await _dataProvider.CreateOperationReceiptAsync(new EraEquipmentOperationReceipt
        {
            Id = Guid.NewGuid(), PlayerId = character.PlayerId, CharacterId = character.Id, RequestId = command.RequestId,
            Operation = EraEquipmentOperation.SelectWeaponModification, RequestPayload = payload, ResultPayload = JsonSerializer.Serialize(result),
            CreatedAt = DateTimeOffset.UtcNow
        }, cancellationToken).ConfigureAwait(false);
        character.EquipmentVersion = state.EquipmentVersion;
        if (changed && _notifications != null)
            await _notifications.EnqueueAsync(character.PlayerId, character.Id, state.EquipmentVersion, command.RequestId, cancellationToken).ConfigureAwait(false);
        return new SelectEraWeaponModificationResponse { Code = HttpStatusCode.OK, Data = result };
    }
}
