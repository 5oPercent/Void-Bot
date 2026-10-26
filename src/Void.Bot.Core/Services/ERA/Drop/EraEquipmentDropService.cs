using SystemEnum = System.Enum;
using System.Linq.Expressions;
using System.Net;
using System.Text.Json;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Resources.i18n.ERA;
using Void.Bot.Core.Services.ERA.Drop.Models;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Core.Services.ERA.Equipment.Models;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Core.Services.ERA.Reward;
using Void.Bot.Core.Services.ERA.World;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Drop;

public interface IEraEquipmentDropService : IScope
{
    Task<SubmitEraGroundRewardResponse> GrantAsync(GrantEraGroundRewardCommand command, CancellationToken cancellationToken);
    Task<SubmitEraGroundRewardResponse> SubmitAsync(SubmitEraGroundRewardCommand command, CancellationToken cancellationToken);
    Task<GetEraEquipmentDropsResponse> GetAsync(GetEraEquipmentDropsRequest request, CancellationToken cancellationToken);
    Task<PickupEraEquipmentDropResponse> PickupAsync(PickupEraEquipmentDropCommand command, CancellationToken cancellationToken);
    Task<DropEraInventoryEquipmentResponse> DropAsync(DropEraInventoryEquipmentCommand command, CancellationToken cancellationToken);
}

public sealed partial class EraEquipmentDropService : IEraEquipmentDropService
{
    private readonly IEraEquipmentDropDataProvider _drops;
    private readonly IEraWorldDataProvider _worlds;
    private readonly IEraWorldService _worldService;
    private readonly IEraRewardDataProvider _rewards;
    private readonly IEraEquipmentDataProvider _equipment;
    private readonly IEraDataProvider _characters;
    private readonly IEraPlayerContext _players;
    private readonly IEraCharacterContext _characterContext;
    private readonly IEraArmorGenerator _armor;
    private readonly IEraRewardPoolRoller _pools;
    private readonly ILocalizationService _localization;
    private readonly TimeProvider _clock;
    private readonly IEraWeaponGenerator? _weapons;
    private readonly IEraEquipmentNotificationService? _notifications;

    public EraEquipmentDropService(IEraEquipmentDropDataProvider drops, IEraWorldDataProvider worlds, IEraWorldService worldService,
        IEraRewardDataProvider rewards, IEraEquipmentDataProvider equipment, IEraDataProvider characters, IEraPlayerContext players,
        IEraCharacterContext characterContext, IEraArmorGenerator armor, IEraRewardPoolRoller pools,
        ILocalizationService localization, TimeProvider clock, IEraWeaponGenerator? weapons = null,
        IEraEquipmentNotificationService? notifications = null)
    {
        _drops = drops;
        _worlds = worlds;
        _worldService = worldService;
        _rewards = rewards;
        _equipment = equipment;
        _characters = characters;
        _players = players;
        _characterContext = characterContext;
        _armor = armor;
        _pools = pools;
        _localization = localization;
        _clock = clock;
        _weapons = weapons;
        _notifications = notifications;
    }

    private EraRewardException Error(HttpStatusCode status, string code, Expression<Func<EraEquipmentDropContextTemplate, object>> text)
    {
        return new EraRewardException(status, code, _localization.GetValue(text));
    }

    public async Task<GetEraEquipmentDropsResponse> GetAsync(GetEraEquipmentDropsRequest request, CancellationToken cancellationToken)
    {
        if (request.WorldInstanceId == Guid.Empty)
        {
            throw Error(HttpStatusCode.BadRequest, "era.drop_invalid_operation", text => text.InvalidOperation);
        }
        var player = await _players.RequireCurrentAsync(false, cancellationToken).ConfigureAwait(false);
        var snapshot = await _drops.ReadVisibleAsync(request.WorldInstanceId, player.Id, cancellationToken).ConfigureAwait(false);
        if (snapshot == null)
        {
            throw Error(HttpStatusCode.NotFound, "era.drop_not_found", text => text.NotFound);
        }
        if (snapshot.World.State != EraWorldState.Ready || snapshot.ActiveWorldId != snapshot.World.Id)
        {
            throw Error(HttpStatusCode.Conflict, "era.drop_world_unavailable", text => text.WorldUnavailable);
        }
        if (!snapshot.Member.IsContinuing || snapshot.Member.ActivePlayerId != player.Id ||
            snapshot.Member.ActiveCharacterId != snapshot.Character.Id || snapshot.Character.IsDisabled)
        {
            throw Error(HttpStatusCode.Forbidden, "era.drop_character_unavailable", text => text.CharacterUnavailable);
        }
        var manifest = await EraEquipmentService.ReadConfigurationAsync(_equipment, false, cancellationToken).ConfigureAwait(false);
        var result = new EraEquipmentDropStateDto
        {
            WorldInstanceId = snapshot.World.Id, WorldVersion = snapshot.World.Version,
            CharacterId = snapshot.Character.Id, EquipmentVersion = snapshot.Character.EquipmentVersion, ManifestVersion = manifest?.Version
        };
        var now = _clock.GetUtcNow();
        foreach (var entry in snapshot.Entries)
        {
            if (!CanPickup(entry.Drop, entry.Share, player.Id, now))
            {
                continue;
            }
            if (!IsValidEntry(entry))
            {
                throw Error(HttpStatusCode.ServiceUnavailable, "era.drop_data_unavailable", text => text.DataUnavailable);
            }
            var item = entry.Equipment!;
            var armor = entry.Armor;
            result.Drops.Add(new EraEquipmentDropDto
            {
                DropId = entry.Drop.Id, DropVersion = entry.Drop.Version, EquipmentId = item.Id, EquipmentVersion = item.Version,
                PrivateOwnerPlayerId = entry.Drop.PrivateOwnerPlayerId, DroppedByPlayerId = entry.Drop.DroppedByPlayerId,
                ShareUntil = entry.Share?.ShareUntil,
                DefinitionId = item.DefinitionId, DefinitionVersion = item.DefinitionVersion,
                Definition = manifest?.Armors.Templates.SingleOrDefault(definition => definition.Id == item.DefinitionId),
                Grade = item.Grade, CreatedAt = item.CreatedAt,
                EquipmentType = item.EquipmentType, Weapon = entry.Weapon,
                ArmorModifications = armor?.Modifications,
                ArmorEffectiveAttributes = armor == null || manifest == null ? null : EraArmorModificationRules.Evaluate(armor, manifest.Armors),
                WeaponTemplate = manifest?.Weapons.Templates.SingleOrDefault(template => template.Id == item.DefinitionId),
                GenerationRuleVersion = armor?.GenerationRuleVersion ?? 0,
                ArmorAttributes = armor == null ? new EraArmorAttributesDto() : new EraArmorAttributesDto
                {
                    Strength = armor.Strength, Survival = armor.Survival, Mobility = armor.Mobility,
                    Focus = armor.Focus, Efficiency = armor.Efficiency, Resistance = armor.Resistance
                },
                Placement = new EraEquipmentDropPlacementDto
                {
                    X = entry.Drop.X, Y = entry.Drop.Y, Z = entry.Drop.Z, YawRadians = entry.Drop.YawRadians
                }
            });
        }
        return new GetEraEquipmentDropsResponse { Code = HttpStatusCode.OK, Data = result };
    }

    public async Task<PickupEraEquipmentDropResponse> PickupAsync(PickupEraEquipmentDropCommand command, CancellationToken cancellationToken)
    {
        if (command.WorldInstanceId == Guid.Empty || command.DropId == Guid.Empty || command.CharacterId == Guid.Empty ||
            command.RequestId == Guid.Empty || command.ExpectedEquipmentVersion < 0 || command.ExpectedDropVersion < 1 || command.ExpectedItemVersion < 1)
        {
            throw Error(HttpStatusCode.BadRequest, "era.drop_invalid_operation", text => text.InvalidOperation);
        }
        var character = await _characterContext.RequireOwnedAsync(command.CharacterId, true, cancellationToken).ConfigureAwait(false);
        var payload = JsonSerializer.Serialize(new
        {
            SchemaVersion = 1, Operation = EraEquipmentOperation.Pickup, command.WorldInstanceId, command.DropId, command.CharacterId,
            command.ExpectedEquipmentVersion, command.ExpectedDropVersion, command.ExpectedItemVersion
        });
        var existing = await _equipment.GetOperationReceiptAsync(character.PlayerId, command.RequestId, cancellationToken).ConfigureAwait(false);
        if (existing != null)
        {
            if (existing.Operation != EraEquipmentOperation.Pickup || existing.CharacterId != character.Id || existing.RequestPayload != payload)
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
                previous.Operation != EraEquipmentOperation.Pickup || previous.State == null || previous.State.CharacterId != character.Id ||
                previous.PreviousEquipmentVersion != command.ExpectedEquipmentVersion || previous.PreviousEquipmentVersion == long.MaxValue ||
                previous.State.EquipmentVersion != previous.PreviousEquipmentVersion + 1 || previous.Pickup == null ||
                previous.Pickup.DropId != command.DropId || previous.Pickup.WorldInstanceId != command.WorldInstanceId ||
                command.ExpectedDropVersion == long.MaxValue || previous.Pickup.DropVersion != command.ExpectedDropVersion + 1 ||
                previous.State.InventoryItems == null || !previous.State.InventoryItems.Any(item => item.Id == previous.Pickup.EquipmentId))
            {
                throw Error(HttpStatusCode.ServiceUnavailable, "era.drop_data_unavailable", text => text.DataUnavailable);
            }
            return new PickupEraEquipmentDropResponse { Code = HttpStatusCode.OK, Data = previous, IsReplay = true };
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
        var drop = await _drops.LockDropAsync(command.DropId, cancellationToken).ConfigureAwait(false);
        var share = drop == null ? null : await _drops.LockShareAsync(drop.EquipmentId, cancellationToken).ConfigureAwait(false);
        if (drop == null || drop.WorldInstanceId != world.Id || !CanPickup(drop, share, character.PlayerId, _clock.GetUtcNow()))
        {
            throw Error(HttpStatusCode.NotFound, "era.drop_not_found", text => text.NotFound);
        }
        if (drop.Status != EraEquipmentDropStatus.Available || drop.Version != command.ExpectedDropVersion)
        {
            throw Error(HttpStatusCode.Conflict, "era.drop_version_conflict", text => text.VersionConflict);
        }
        var snapshot = await _equipment.LockCharacterEquipmentAsync(character, cancellationToken).ConfigureAwait(false);
        var entry = await _drops.LockEquipmentAsync(drop.EquipmentId, cancellationToken).ConfigureAwait(false);
        if (entry == null || !IsValidEntry(new EraEquipmentDropReadEntry(drop, entry.Equipment, entry.Armor, share, entry.Weapon)))
        {
            throw Error(HttpStatusCode.ServiceUnavailable, "era.drop_data_unavailable", text => text.DataUnavailable);
        }
        var equipment = entry.Equipment;
        if (equipment.Version != command.ExpectedItemVersion)
        {
            throw Error(HttpStatusCode.Conflict, "era.drop_version_conflict", text => text.VersionConflict);
        }
        if (snapshot.Entries.Count(row => row.Equipment.Location == EraEquipmentLocation.Inventory) >= EraEquipmentRules.InventoryCapacity)
        {
            throw Error(HttpStatusCode.Conflict, "era.drop_inventory_full", text => text.InventoryFull);
        }
        if (character.EquipmentVersion == long.MaxValue || equipment.Version == long.MaxValue || drop.Version == long.MaxValue)
        {
            throw Error(HttpStatusCode.ServiceUnavailable, "era.drop_data_unavailable", text => text.DataUnavailable);
        }
        // 取得所有写锁后重判期限，等待背包／装备锁的时间不能延长他人的分享资格。
        var now = _clock.GetUtcNow();
        if (!CanPickup(drop, share, character.PlayerId, now))
        {
            throw Error(HttpStatusCode.NotFound, "era.drop_not_found", text => text.NotFound);
        }
        if (drop.DroppedByPlayerId.HasValue && (share == null || share.ShareUntil - equipment.CreatedAt != TimeSpan.FromHours(1)))
        {
            throw Error(HttpStatusCode.ServiceUnavailable, "era.drop_data_unavailable", text => text.DataUnavailable);
        }
        var manifest = await EraEquipmentService.ReadConfigurationAsync(_equipment, false, cancellationToken).ConfigureAwait(false);
        equipment.Location = EraEquipmentLocation.Inventory;
        equipment.OwnerPlayerId = character.PlayerId;
        equipment.CharacterId = character.Id;
        equipment.Version++;
        snapshot.Entries.Add(entry);
        snapshot.EquipmentVersion = character.EquipmentVersion + 1;
        var state = EraEquipmentService.CreateState(snapshot, manifest, null);
        if (state == null)
        {
            throw Error(HttpStatusCode.ServiceUnavailable, "era.drop_data_unavailable", text => text.DataUnavailable);
        }
        drop.Status = EraEquipmentDropStatus.PickedUp;
        drop.ActiveEquipmentId = null;
        drop.Version++;
        var receipt = new EraEquipmentOperationReceiptDto
        {
            RequestId = command.RequestId, CharacterId = character.Id, Operation = EraEquipmentOperation.Pickup,
            PreviousEquipmentVersion = character.EquipmentVersion, State = state,
            Pickup = new EraEquipmentDropPickupDto { WorldInstanceId = world.Id, DropId = drop.Id, EquipmentId = equipment.Id, DropVersion = drop.Version }
        };
        await _equipment.CreateOperationReceiptAsync(new EraEquipmentOperationReceipt
        {
            Id = Guid.NewGuid(), PlayerId = character.PlayerId, CharacterId = character.Id, RequestId = command.RequestId,
            Operation = EraEquipmentOperation.Pickup, RequestPayload = payload, ResultPayload = JsonSerializer.Serialize(receipt), CreatedAt = now
        }, cancellationToken).ConfigureAwait(false);
        character.EquipmentVersion = state.EquipmentVersion;
        if (_notifications != null)
            await _notifications.EnqueueAsync(character.PlayerId, character.Id, state.EquipmentVersion, command.RequestId, cancellationToken).ConfigureAwait(false);
        if (now > world.LastActiveAt)
        {
            world.LastActiveAt = now;
        }
        world.TimeoutObservedAt = null;
        return new PickupEraEquipmentDropResponse { Code = HttpStatusCode.OK, Data = receipt };
    }

    private async Task<EraWorld> LockActiveWorldAsync(Guid worldId, EraCharacter character, CancellationToken cancellationToken)
    {
        var initial = await _worlds.GetWorldAsync(worldId, cancellationToken).ConfigureAwait(false);
        if (initial == null)
        {
            throw Error(HttpStatusCode.NotFound, "era.drop_not_found", text => text.NotFound);
        }
        var team = await _worlds.LockTeamAsync(initial.TeamId, cancellationToken).ConfigureAwait(false);
        var world = await _worlds.LockWorldAsync(initial.Id, cancellationToken).ConfigureAwait(false);
        if (world == null || world.State != EraWorldState.Ready || team.ActiveWorldId != world.Id)
        {
            throw Error(HttpStatusCode.Conflict, "era.drop_world_unavailable", text => text.WorldUnavailable);
        }
        var members = await _worlds.GetMembersAsync(world.Id, false, cancellationToken).ConfigureAwait(false);
        if (!members.Any(member => member.PlayerId == character.PlayerId && member.CharacterId == character.Id && member.IsContinuing &&
            member.ActiveCharacterId == character.Id && member.ActivePlayerId == character.PlayerId))
        {
            throw Error(HttpStatusCode.Forbidden, "era.drop_character_unavailable", text => text.CharacterUnavailable);
        }
        return world;
    }

    private static List<Guid>? ReadSharePlayers(EraEquipmentShare? share)
    {
        if (share == null || share.EquipmentId == Guid.Empty || share.ShareUntil == default)
        {
            return null;
        }
        try
        {
            var players = JsonSerializer.Deserialize<List<Guid>>(share.AllowedPlayerIdsPayload);
            return players != null && players.Count > 0 &&
                players.All(player => player != Guid.Empty) && players.Distinct().Count() == players.Count ? players : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static bool CanPickup(EraEquipmentDrop drop, EraEquipmentShare? share, Guid playerId, DateTimeOffset now)
    {
        if (drop.PrivateOwnerPlayerId.HasValue)
        {
            return drop.PrivateOwnerPlayerId == playerId && !drop.DroppedByPlayerId.HasValue;
        }
        var players = ReadSharePlayers(share);
        return drop.DroppedByPlayerId.HasValue && players != null && players.Contains(drop.DroppedByPlayerId.Value) &&
            (now < share!.ShareUntil ? players.Contains(playerId) : drop.DroppedByPlayerId == playerId);
    }

    private static bool IsValidEntry(EraEquipmentDropReadEntry entry)
    {
        var drop = entry.Drop;
        var item = entry.Equipment;
        var armor = entry.Armor;
        return drop.Status == EraEquipmentDropStatus.Available && drop.Version > 0 && drop.CreatedAt != default &&
            (drop.PrivateOwnerPlayerId.HasValue != drop.DroppedByPlayerId.HasValue) &&
            double.IsFinite(drop.X) && double.IsFinite(drop.Y) && double.IsFinite(drop.Z) && double.IsFinite(drop.YawRadians) &&
            item != null && drop.EquipmentId == item.Id && drop.ActiveEquipmentId == item.Id && item.Location == EraEquipmentLocation.Ground &&
            !item.OwnerPlayerId.HasValue && !item.CharacterId.HasValue && !item.EquippedSlot.HasValue &&
            item.Version > 0 && item.DefinitionVersion > 0 && item.DefinitionId != Guid.Empty && item.CreatedAt != default &&
            (!drop.DroppedByPlayerId.HasValue || entry.Share != null && entry.Share.EquipmentId == item.Id &&
                entry.Share.ShareUntil - item.CreatedAt == TimeSpan.FromHours(1)) &&
            SystemEnum.IsDefined(item.Grade) && EraEquipmentItemRules.ValidPayload(new EraEquipmentReadEntry(item, armor, entry.Weapon));
    }
}
