using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    private async Task SeedShareAsync(EraEquipmentInstance equipment, params Guid[] players)
    {
        _worldTestClock = new EraWorldTestClock(equipment.CreatedAt.AddMinutes(10));
        await ReadDatabaseAsync(async context =>
        {
            context.Set<EraEquipmentShare>().Add(new EraEquipmentShare
            {
                EquipmentId = equipment.Id, ShareUntil = equipment.CreatedAt.AddHours(1),
                AllowedPlayerIdsPayload = JsonSerializer.Serialize(players)
            });
            await context.SaveChangesAsync();
        });
    }

    private DropEraInventoryEquipmentCommand DropCommand(EraWorldDto world, Guid equipmentId, long equipmentVersion = 3,
        long itemVersion = 5, Guid? characterId = null)
    {
        return new DropEraInventoryEquipmentCommand
        {
            WorldInstanceId = world.WorldInstanceId, CharacterId = characterId ?? _characterId, EquipmentId = equipmentId,
            RequestId = Guid.NewGuid(), ExpectedEquipmentVersion = equipmentVersion, ExpectedItemVersion = itemVersion,
            Placement = new EraEquipmentDropPlacementDto { X = 8.5, Y = 1, Z = -3, YawRadians = 0.4 }
        };
    }

    private Task<DropEraInventoryEquipmentResponse> DropInventoryAsync(DropEraInventoryEquipmentCommand command,
        EraIntegrationSqlInterceptor? interceptor = null, CancellationToken cancellationToken = default, Guid? accountId = null)
    {
        return RequestAsync((mediator, token) => mediator.SendAsync<DropEraInventoryEquipmentCommand, DropEraInventoryEquipmentResponse>(command, token),
            interceptor, cancellationToken, accountId);
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Sharing_should_keep_original_roster_deadline_and_identity_across_repeated_transfers_and_team_loss()
    {
        await SeedAsync();
        var original = _inventory[0];
        await SeedShareAsync(original, _playerId, _otherPlayerId);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = await ReadyDropWorldAsync(team);
        world = (await RestrictAsync(world, team.Team.Members[0].MemberId)).Data!;
        var firstCommand = DropCommand(world, original.Id);
        var first = await DropInventoryAsync(firstCommand);
        first.Data!.State.InventoryItems.ShouldBeEmpty();
        first.Data.State.EquipmentVersion.ShouldBe(4);
        first.Data.State.EquippedOperationRestriction!.IsRestricted.ShouldBeTrue();
        var shared = (await QueryDropsAsync(world.WorldInstanceId, _otherAccountId)).Data!.Drops.Single();
        shared.PrivateOwnerPlayerId.ShouldBeNull();
        shared.DroppedByPlayerId.ShouldBe(_playerId);
        shared.ShareUntil.ShouldBe(original.CreatedAt.AddHours(1));
        shared.Placement.X.ShouldBe(firstCommand.Placement.X);
        shared.ArmorAttributes.Strength.ShouldBe(120);
        await RemoveTestTeamAsync(team); // 当前队伍消失不撤销世界内仍有效角色的原名单资格。
        var pickup = PickupCommand(world, shared);
        pickup.CharacterId = _otherCharacterId;
        var picked = await PickupDropAsync(pickup, accountId: _otherAccountId);
        picked.Data!.State.InventoryItems.Single(item => item.Id == original.Id).Version.ShouldBe(7);
        var secondCommand = DropCommand(world, original.Id, 4, 7, _otherCharacterId);
        var second = await DropInventoryAsync(secondCommand, accountId: _otherAccountId);
        second.Data!.Dropped!.DropId.ShouldNotBe(first.Data.Dropped!.DropId);
        second.Data.Dropped.EquipmentId.ShouldBe(original.Id);
        second.Data.Dropped.ShareUntil.ShouldBe(first.Data.Dropped.ShareUntil);
        _worldTestClock!.Advance(TimeSpan.FromMinutes(50)); // 恰好到原始一小时边界。
        (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops.ShouldBeEmpty();
        var expired = (await QueryDropsAsync(world.WorldInstanceId, _otherAccountId)).Data!.Drops.Single();
        expired.DropId.ShouldBe(second.Data.Dropped.DropId);
        var denied = PickupCommand(world, expired, 4);
        (await Should.ThrowAsync<EraRewardException>(() => PickupDropAsync(denied))).ErrorCode.ShouldBe("era.drop_not_found");
        var reclaim = PickupCommand(world, expired, 5);
        reclaim.CharacterId = _otherCharacterId;
        var reclaimed = await PickupDropAsync(reclaim, accountId: _otherAccountId);
        var item = reclaimed.Data!.State.InventoryItems.Single(row => row.Id == original.Id);
        item.CreatedAt.ShouldBe(original.CreatedAt);
        item.Version.ShouldBe(9);
        item.ArmorAttributes.Strength.ShouldBe(120);
        (await Should.ThrowAsync<EraRewardException>(() => DropInventoryAsync(
            DropCommand(world, original.Id, 6, 9, _otherCharacterId), accountId: _otherAccountId)))
            .ErrorCode.ShouldBe("era.drop_share_expired");
        var replay = await DropInventoryAsync(firstCommand);
        replay.IsReplay.ShouldBeTrue();
        JsonSerializer.Serialize(replay.Data).ShouldBe(JsonSerializer.Serialize(first.Data));
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentDrop>().CountAsync()).ShouldBe(2);
            (await context.Set<EraEquipmentDrop>().CountAsync(row => row.Status == EraEquipmentDropStatus.Available)).ShouldBe(0);
            var saved = await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Id == original.Id);
            saved.OwnerPlayerId.ShouldBe(_otherPlayerId);
            saved.CharacterId.ShouldBe(_otherCharacterId);
            saved.Location.ShouldBe(EraEquipmentLocation.Inventory);
            var share = await context.Set<EraEquipmentShare>().SingleAsync();
            share.ShareUntil.ShouldBe(original.CreatedAt.AddHours(1));
            JsonSerializer.Deserialize<List<Guid>>(share.AllowedPlayerIdsPayload)!.ShouldBe(new[] { _playerId, _otherPlayerId });
        });
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [Trait("Category", "ERADropIntegration")]
    public async Task Inventory_drop_should_use_the_exact_original_deadline(long offsetTicks)
    {
        await SeedAsync();
        await SeedShareAsync(_inventory[0], _playerId);
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        _worldTestClock!.Advance(TimeSpan.FromMinutes(50).Add(TimeSpan.FromTicks(offsetTicks)));
        var command = DropCommand(world, _inventory[0].Id);
        if (offsetTicks < 0)
        {
            (await DropInventoryAsync(command)).Code.ShouldBe(HttpStatusCode.OK);
        }
        else
        {
            (await Should.ThrowAsync<EraRewardException>(() => DropInventoryAsync(command))).ErrorCode.ShouldBe("era.drop_share_expired");
            await AssertOriginalStateAsync();
            await ReadDatabaseAsync(async context => (await context.Set<EraEquipmentDrop>().CountAsync()).ShouldBe(0));
        }
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Current_world_teammates_outside_the_original_roster_should_neither_see_nor_pick_shared_equipment()
    {
        await SeedAsync();
        await SeedShareAsync(_inventory[0], _playerId);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = await ReadyDropWorldAsync(team);
        await DropInventoryAsync(DropCommand(world, _inventory[0].Id));
        (await QueryDropsAsync(world.WorldInstanceId, _otherAccountId)).Data!.Drops.ShouldBeEmpty();
        var visible = (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops.Single();
        var command = PickupCommand(world, visible);
        command.CharacterId = _otherCharacterId;
        (await Should.ThrowAsync<EraRewardException>(() => PickupDropAsync(command, accountId: _otherAccountId)))
            .ErrorCode.ShouldBe("era.drop_not_found");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("Category", "ERADropIntegration")]
    public async Task Inventory_drop_should_reject_equipped_items_and_items_without_original_share_eligibility(bool equipped)
    {
        await SeedAsync(equipped: equipped);
        var item = equipped ? _equipped! : _inventory[0];
        if (equipped)
        {
            await SeedShareAsync(item, _playerId);
        }
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        (await Should.ThrowAsync<EraRewardException>(() => DropInventoryAsync(DropCommand(world, item.Id))))
            .ErrorCode.ShouldBe(equipped ? "era.drop_not_found" : "era.drop_share_unavailable");
        await AssertOriginalStateAsync();
    }

    [Theory]
    [InlineData("foreign")]
    [InlineData("disabled")]
    [InlineData("left")]
    [InlineData("ended")]
    [InlineData("stale_role")]
    [InlineData("stale_item")]
    [Trait("Category", "ERADropIntegration")]
    public async Task Inventory_drop_should_validate_asset_ownership_character_world_and_versions_before_writing(string scenario)
    {
        await SeedAsync();
        await SeedShareAsync(_inventory[0], _playerId);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = await ReadyDropWorldAsync(team);
        var command = DropCommand(world, _inventory[0].Id);
        var expected = "era.drop_version_conflict";
        switch (scenario)
        {
            case "foreign":
                await ReadDatabaseAsync(async context =>
                {
                    command.EquipmentId = (await context.Set<EraEquipmentInstance>()
                        .SingleAsync(row => row.OwnerPlayerId == _otherPlayerId)).Id;
                });
                expected = "era.drop_not_found";
                break;
            case "disabled":
                await ReadDatabaseAsync(async context =>
                {
                    (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).IsDisabled = true;
                    await context.SaveChangesAsync();
                });
                expected = "era.drop_character_unavailable";
                break;
            case "left":
                await RequestAsync((mediator, token) => mediator.SendAsync<LeaveEraWorldCommand, LeaveEraWorldResponse>(new LeaveEraWorldCommand
                {
                    WorldInstanceId = world.WorldInstanceId, MemberId = team.Team.Members[0].MemberId,
                    AuthorityEpoch = world.AuthorityEpoch, ExpectedWorldVersion = world.Version
                }, token));
                expected = "era.drop_character_unavailable";
                break;
            case "ended":
                await EndDropWorldAsync(world, team.Team.Members[0].MemberId);
                expected = "era.drop_world_unavailable";
                break;
            case "stale_role":
                command.ExpectedEquipmentVersion--;
                break;
            case "stale_item":
                command.ExpectedItemVersion--;
                break;
        }
        (await Should.ThrowAsync<EraRewardException>(() => DropInventoryAsync(command))).ErrorCode.ShouldBe(expected);
        await AssertOriginalStateAsync();
        await ReadDatabaseAsync(async context => (await context.Set<EraEquipmentDrop>().CountAsync()).ShouldBe(0));
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Expired_shared_equipment_should_remain_on_ground_when_the_dropper_inventory_is_full()
    {
        await SeedAsync(120);
        await SeedShareAsync(_inventory[0], _playerId, _otherPlayerId);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = await ReadyDropWorldAsync(team);
        await DropInventoryAsync(DropCommand(world, _inventory[0].Id));
        var filler = NewItem();
        await ReadDatabaseAsync(async context =>
        {
            AddItems(context, new[] { filler });
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion++;
            await context.SaveChangesAsync();
        });
        _worldTestClock!.Advance(TimeSpan.FromHours(1));
        var drop = (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops.Single();
        (await QueryDropsAsync(world.WorldInstanceId, _otherAccountId)).Data!.Drops.ShouldBeEmpty();
        (await Should.ThrowAsync<EraRewardException>(() => PickupDropAsync(PickupCommand(world, drop, 5))))
            .ErrorCode.ShouldBe("era.drop_inventory_full");
        await ReadDatabaseAsync(async context =>
        {
            var saved = await context.Set<EraEquipmentDrop>().SingleAsync();
            saved.Status.ShouldBe(EraEquipmentDropStatus.Available);
            saved.Version.ShouldBe(1);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync()).ShouldBe(1);
            var item = await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Id == drop.EquipmentId);
            item.Location.ShouldBe(EraEquipmentLocation.Ground);
            item.OwnerPlayerId.ShouldBeNull();
        });
        await ReadDatabaseAsync(async context =>
        {
            var released = await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Id == filler.Id);
            released.Location = EraEquipmentLocation.Destroyed;
            released.OwnerPlayerId = null;
            released.CharacterId = null;
            released.Version++;
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion++;
            await context.SaveChangesAsync();
        });
        var reclaimed = await PickupDropAsync(PickupCommand(world, drop, 6));
        reclaimed.Data!.State.EquipmentVersion.ShouldBe(7);
        reclaimed.Data.State.InventoryItems.Single(item => item.Id == drop.EquipmentId).Version.ShouldBe(7);
        (await Should.ThrowAsync<EraRewardException>(() => DropInventoryAsync(DropCommand(world, drop.EquipmentId, 7, 7))))
            .ErrorCode.ShouldBe("era.drop_share_expired");
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Inventory_drop_save_failure_should_rollback_all_assets_and_allow_the_same_request_to_retry()
    {
        await SeedAsync();
        await SeedShareAsync(_inventory[0], _playerId);
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        var command = DropCommand(world, _inventory[0].Id);
        var failure = new EraIntegrationSqlInterceptor { FailReceiptInsert = true };
        var error = await Should.ThrowAsync<DbUpdateException>(() => DropInventoryAsync(command, failure));
        failure.ReceiptFailureInjected.ShouldBeTrue();
        error.InnerException.ShouldBeOfType<MySqlException>().Number.ShouldBe(1644);
        await AssertOriginalStateAsync();
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentDrop>().CountAsync()).ShouldBe(0);
            (await context.Set<EraEquipmentShare>().CountAsync()).ShouldBe(1);
        });
        (await DropInventoryAsync(command)).IsReplay.ShouldBeFalse();
        (await DropInventoryAsync(command)).IsReplay.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Inventory_drop_retries_should_reject_parameter_and_operation_changes_and_not_restore_ended_world_assets()
    {
        await SeedAsync();
        await SeedShareAsync(_inventory[0], _playerId);
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        var command = DropCommand(world, _inventory[0].Id);
        var first = await DropInventoryAsync(command);
        var changed = JsonSerializer.Deserialize<DropEraInventoryEquipmentCommand>(JsonSerializer.Serialize(command))!;
        changed.Placement.X++;
        (await Should.ThrowAsync<EraRewardException>(() => DropInventoryAsync(changed))).ErrorCode.ShouldBe("era.drop_idempotency_conflict");
        (await SendEquipAsync(Equip(_inventory[0], command.RequestId))).ErrorCode.ShouldBe("era.equipment_idempotency_conflict");
        await EndDropWorldAsync(world, team.Team.Members[0].MemberId);
        var replay = await DropInventoryAsync(command);
        replay.IsReplay.ShouldBeTrue();
        JsonSerializer.Serialize(replay.Data).ShouldBe(JsonSerializer.Serialize(first.Data));
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentDrop>().SingleAsync()).Status.ShouldBe(EraEquipmentDropStatus.Invalidated);
            (await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Id == _inventory[0].Id)).Location.ShouldBe(EraEquipmentLocation.Destroyed);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync()).ShouldBe(1);
        });
    }
}
