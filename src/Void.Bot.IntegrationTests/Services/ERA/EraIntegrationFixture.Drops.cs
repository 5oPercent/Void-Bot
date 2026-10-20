using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    private async Task<EraWorldDto> ReadyDropWorldAsync(EraWorldIntegrationTeam team)
    {
        var world = await RegisterWorldAsync(team);
        var host = team.Team.Members.Single(member => member.PlayerId == team.Team.HostPlayerId);
        var ready = await RequestAsync((mediator, token) => mediator.SendAsync<ReadyEraWorldCommand, ReadyEraWorldResponse>(new ReadyEraWorldCommand
        {
            WorldInstanceId = world.WorldInstanceId, MemberId = host.MemberId, AuthorityEpoch = world.AuthorityEpoch, ExpectedWorldVersion = world.Version
        }, token), accountId: host.PlayerId == _playerId ? null : _otherAccountId);
        return ready.Data!;
    }

    private SubmitEraGroundRewardCommand GroundCommand(EraWorldDto world, EraWorldIntegrationTeam team)
    {
        return new SubmitEraGroundRewardCommand
        {
            WorldInstanceId = world.WorldInstanceId, MemberId = team.Team.Members.Single(member => member.PlayerId == team.Team.HostPlayerId).MemberId,
            AuthorityEpoch = world.AuthorityEpoch, RequestId = Guid.NewGuid(), SourceType = EraRewardSourceType.Monster,
            SourceDefinitionId = "test.source", SourceInstanceId = Guid.NewGuid(), TriggerIndex = 1, RewardNodeId = "complete",
            Placement = new EraEquipmentDropPlacementDto { X = 12.5, Y = 3, Z = -4.5, YawRadians = 0.2 }
        };
    }

    private Task<SubmitEraGroundRewardResponse> SubmitGroundAsync(SubmitEraGroundRewardCommand command,
        EraIntegrationSqlInterceptor? interceptor = null, Guid? accountId = null, CancellationToken cancellationToken = default)
    {
        return RequestAsync((mediator, token) => mediator.SendAsync<SubmitEraGroundRewardCommand, SubmitEraGroundRewardResponse>(command, token),
            interceptor, cancellationToken, accountId);
    }

    private Task<GetEraEquipmentDropsResponse> QueryDropsAsync(Guid worldId, Guid? accountId = null)
    {
        return RequestAsync((mediator, token) => mediator.RequestAsync<GetEraEquipmentDropsRequest, GetEraEquipmentDropsResponse>(
            new GetEraEquipmentDropsRequest { WorldInstanceId = worldId }, token), accountId: accountId);
    }

    private PickupEraEquipmentDropCommand PickupCommand(EraWorldDto world, EraEquipmentDropDto drop, long equipmentVersion = 3)
    {
        return new PickupEraEquipmentDropCommand
        {
            WorldInstanceId = world.WorldInstanceId, DropId = drop.DropId, CharacterId = _characterId,
            RequestId = Guid.NewGuid(), ExpectedEquipmentVersion = equipmentVersion,
            ExpectedDropVersion = drop.DropVersion, ExpectedItemVersion = drop.EquipmentVersion
        };
    }

    private Task<PickupEraEquipmentDropResponse> PickupDropAsync(PickupEraEquipmentDropCommand command,
        EraIntegrationSqlInterceptor? interceptor = null, CancellationToken cancellationToken = default)
    {
        return RequestAsync((mediator, token) => mediator.SendAsync<PickupEraEquipmentDropCommand, PickupEraEquipmentDropResponse>(command, token),
            interceptor, cancellationToken);
    }

    private Task<EndEraWorldResponse> EndDropWorldAsync(EraWorldDto world, Guid memberId, EraIntegrationSqlInterceptor? interceptor = null,
        CancellationToken cancellationToken = default, Guid? accountId = null)
    {
        return RequestAsync((mediator, token) => mediator.SendAsync<EndEraWorldCommand, EndEraWorldResponse>(new EndEraWorldCommand
        {
            WorldInstanceId = world.WorldInstanceId, MemberId = memberId, AuthorityEpoch = world.AuthorityEpoch, ExpectedWorldVersion = world.Version
        }, token), interceptor, cancellationToken, accountId);
    }

    [Theory]
    [InlineData(EraRewardSourceType.Monster)]
    [InlineData(EraRewardSourceType.Chest)]
    [Trait("Category", "ERADropIntegration")]
    public async Task Ground_rewards_should_be_private_and_pickup_should_keep_saved_attributes_identity_and_share_deadline(EraRewardSourceType source)
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(source: source, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = await ReadyDropWorldAsync(team);
        var command = GroundCommand(world, team);
        command.SourceType = source;
        var generated = await SubmitGroundAsync(command);
        generated.Data!.Grants.Count.ShouldBe(2);
        var own = (await QueryDropsAsync(world.WorldInstanceId)).Data!;
        var other = (await QueryDropsAsync(world.WorldInstanceId, _otherAccountId)).Data!;
        own.Drops.Count.ShouldBe(1);
        other.Drops.Count.ShouldBe(1);
        own.Drops.Single().DropId.ShouldNotBe(other.Drops.Single().DropId);
        own.Drops.Single().EquipmentId.ShouldBe(generated.Data.Grants.Single(grant => grant.PlayerId == _playerId).EquipmentIds.Single());
        own.Drops.Single().Placement.X.ShouldBe(command.Placement.X);
        own.Drops.Single().Placement.YawRadians.ShouldBe(command.Placement.YawRadians);
        (await Should.ThrowAsync<EraRewardException>(() => PickupDropAsync(PickupCommand(world, other.Drops.Single()))))
            .ErrorCode.ShouldBe("era.drop_not_found");
        var picked = await PickupDropAsync(PickupCommand(world, own.Drops.Single()));
        picked.Data!.State.EquipmentVersion.ShouldBe(4);
        var savedItem = picked.Data.State.InventoryItems.Single();
        savedItem.Id.ShouldBe(own.Drops.Single().EquipmentId);
        JsonSerializer.Serialize(savedItem.ArmorAttributes).ShouldBe(JsonSerializer.Serialize(own.Drops.Single().ArmorAttributes));
        savedItem.Version.ShouldBe(2);
        (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops.ShouldBeEmpty();
        (await QueryDropsAsync(world.WorldInstanceId, _otherAccountId)).Data!.Drops.Count.ShouldBe(1);
        await ReadDatabaseAsync(async context =>
        {
            var drop = await context.Set<EraEquipmentDrop>().SingleAsync(row => row.Id == own.Drops.Single().DropId);
            drop.Status.ShouldBe(EraEquipmentDropStatus.PickedUp);
            drop.ActiveEquipmentId.ShouldBeNull();
            drop.Version.ShouldBe(2);
            var share = await context.Set<EraEquipmentShare>().SingleAsync(row => row.EquipmentId == savedItem.Id);
            share.ShareUntil.ShouldBe(savedItem.CreatedAt.AddHours(1));
            JsonSerializer.Deserialize<List<Guid>>(share.AllowedPlayerIdsPayload)!.OrderBy(id => id)
                .ShouldBe(new[] { _playerId, _otherPlayerId }.OrderBy(id => id));
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _otherCharacterId)).EquipmentVersion.ShouldBe(3);
        });
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Ground_retries_should_fix_results_and_reject_definition_position_and_request_identity_changes()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        var command = GroundCommand(world, team);
        var first = await SubmitGroundAsync(command);
        await ConfigureRewardsAsync(chance: 0, source: EraRewardSourceType.Chest, delivery: EraRewardDelivery.Inventory);
        command.RequestId = Guid.NewGuid();
        var replay = await SubmitGroundAsync(command);
        replay.IsReplay.ShouldBeTrue();
        JsonSerializer.Serialize(replay.Data).ShouldBe(JsonSerializer.Serialize(first.Data));
        command.SourceDefinitionId = "changed";
        (await Should.ThrowAsync<EraRewardException>(() => SubmitGroundAsync(command))).ErrorCode.ShouldBe("era.drop_idempotency_conflict");
        command.RequestId = Guid.NewGuid();
        (await Should.ThrowAsync<EraRewardException>(() => SubmitGroundAsync(command))).ErrorCode.ShouldBe("era.drop_idempotency_conflict");
        command.SourceDefinitionId = "test.source";
        command.Placement.X++;
        (await Should.ThrowAsync<EraRewardException>(() => SubmitGroundAsync(command))).ErrorCode.ShouldBe("era.drop_idempotency_conflict");
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardEvent>().CountAsync()).ShouldBe(1);
            (await context.Set<EraRewardRequest>().CountAsync()).ShouldBe(2);
            (await context.Set<EraEquipmentDrop>().CountAsync()).ShouldBe(1);
        });
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Full_inventory_should_keep_private_drop_available_and_allow_pickup_after_share_expiration()
    {
        _worldTestClock = new EraWorldTestClock(DateTimeOffset.UtcNow);
        await SeedAsync(12);
        await ConfigureRewardsAsync(source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        await SubmitGroundAsync(GroundCommand(world, team));
        var drop = (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops.Single();
        var command = PickupCommand(world, drop);
        (await Should.ThrowAsync<EraRewardException>(() => PickupDropAsync(command))).ErrorCode.ShouldBe("era.drop_inventory_full");
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentDrop>().SingleAsync()).Status.ShouldBe(EraEquipmentDropStatus.Available);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync()).ShouldBe(0);
            var item = await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Id == _inventory[0].Id);
            item.Location = EraEquipmentLocation.Destroyed;
            item.OwnerPlayerId = null;
            item.CharacterId = null;
            item.Version++;
            await context.SaveChangesAsync();
        });
        _worldTestClock.Advance(TimeSpan.FromHours(2));
        (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops.Single().DropId.ShouldBe(drop.DropId);
        (await PickupDropAsync(command)).Data!.State.InventoryItems.Count.ShouldBe(12);
        (await PickupDropAsync(command)).IsReplay.ShouldBeTrue();
        await ReadDatabaseAsync(async context =>
        {
            var item = await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Id == drop.EquipmentId);
            item.CreatedAt.ShouldBe(drop.CreatedAt);
            (await context.Set<EraEquipmentShare>().SingleAsync()).ShareUntil.ShouldBe(drop.CreatedAt.AddHours(1));
        });
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Zero_chance_ground_event_should_be_final_even_after_probability_changes()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(chance: 0, source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        var command = GroundCommand(world, team);
        (await SubmitGroundAsync(command)).Data!.Grants.Single().DropIds.ShouldBeEmpty();
        await ConfigureRewardsAsync(source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        command.RequestId = Guid.NewGuid();
        (await SubmitGroundAsync(command)).Data!.Grants.Single().DropIds.ShouldBeEmpty();
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardEvent>().CountAsync()).ShouldBe(1);
            (await context.Set<EraRewardGrant>().CountAsync()).ShouldBe(1);
            (await context.Set<EraEquipmentDrop>().CountAsync()).ShouldBe(0);
            (await context.Set<EraEquipmentShare>().CountAsync()).ShouldBe(0);
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion.ShouldBe(3);
        });
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Ending_world_should_destroy_only_unpicked_equipment_and_keep_historical_receipts()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(count: 2, source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        var generation = GroundCommand(world, team);
        var original = await SubmitGroundAsync(generation);
        var drops = (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops;
        var pickup = PickupCommand(world, drops[0]);
        var first = await PickupDropAsync(pickup);
        (await EndDropWorldAsync(world, generation.MemberId)).Data!.State.ShouldBe(EraWorldState.Ended);
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Id == drops[0].EquipmentId)).Location.ShouldBe(EraEquipmentLocation.Inventory);
            (await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Id == drops[1].EquipmentId)).Location.ShouldBe(EraEquipmentLocation.Destroyed);
            var expired = await context.Set<EraEquipmentDrop>().SingleAsync(row => row.Id == drops[1].DropId);
            expired.Status.ShouldBe(EraEquipmentDropStatus.Invalidated);
            expired.ActiveEquipmentId.ShouldBeNull();
            (await context.Set<EraArmorInstanceData>().CountAsync()).ShouldBe(3);
            (await context.Set<EraEquipmentShare>().CountAsync()).ShouldBe(2);
        });
        JsonSerializer.Serialize((await SubmitGroundAsync(generation)).Data).ShouldBe(JsonSerializer.Serialize(original.Data));
        JsonSerializer.Serialize((await PickupDropAsync(pickup)).Data).ShouldBe(JsonSerializer.Serialize(first.Data));
        generation.RequestId = Guid.NewGuid();
        generation.SourceInstanceId = Guid.NewGuid();
        await Should.ThrowAsync<EraWorldAccessException>(() => SubmitGroundAsync(generation));
        await Should.ThrowAsync<EraRewardException>(() => PickupDropAsync(PickupCommand(world, drops[1], 4)));
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Building_world_should_not_generate_assets_and_absent_members_cannot_query()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync();
        var building = await RegisterWorldAsync(team);
        await Should.ThrowAsync<EraWorldAccessException>(() => SubmitGroundAsync(GroundCommand(building, team)));
        (await Should.ThrowAsync<EraRewardException>(() => QueryDropsAsync(building.WorldInstanceId, _otherAccountId))).ErrorCode.ShouldBe("era.drop_not_found");
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardEvent>().CountAsync()).ShouldBe(0);
            (await context.Set<EraEquipmentDrop>().CountAsync()).ShouldBe(0);
        });
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Ground_generation_and_pickup_save_failures_should_roll_back_every_asset_change()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        var command = GroundCommand(world, team);
        var generationFailure = new EraIntegrationSqlInterceptor { FailRewardLinkInsert = true };
        var generationError = await Should.ThrowAsync<DbUpdateException>(() => SubmitGroundAsync(command, generationFailure));
        generationError.InnerException.ShouldBeOfType<MySqlException>().Number.ShouldBe(1644);
        generationFailure.RewardFailureInjected.ShouldBeTrue();
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardEvent>().CountAsync()).ShouldBe(0);
            (await context.Set<EraRewardGrant>().CountAsync()).ShouldBe(0);
            (await context.Set<EraEquipmentDrop>().CountAsync()).ShouldBe(0);
            (await context.Set<EraEquipmentShare>().CountAsync()).ShouldBe(0);
            (await context.Set<EraEquipmentInstance>().CountAsync()).ShouldBe(1);
        });
        await SubmitGroundAsync(command);
        var drop = (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops.Single();
        var pickup = PickupCommand(world, drop);
        var pickupFailure = new EraIntegrationSqlInterceptor { FailReceiptInsert = true };
        var pickupError = await Should.ThrowAsync<DbUpdateException>(() => PickupDropAsync(pickup, pickupFailure));
        pickupError.InnerException.ShouldBeOfType<MySqlException>().Number.ShouldBe(1644);
        pickupFailure.ReceiptFailureInjected.ShouldBeTrue();
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentDrop>().SingleAsync()).Status.ShouldBe(EraEquipmentDropStatus.Available);
            (await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Id == drop.EquipmentId)).Location.ShouldBe(EraEquipmentLocation.Ground);
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion.ShouldBe(3);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync()).ShouldBe(0);
        });
        (await PickupDropAsync(pickup)).Code.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Cleanup_failure_should_roll_back_drop_invalidation_and_retry_should_finish_world()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        var command = GroundCommand(world, team);
        await SubmitGroundAsync(command);
        var failure = new EraIntegrationSqlInterceptor { FailWorldSave = true };
        var error = await Should.ThrowAsync<DbUpdateException>(() => EndDropWorldAsync(world, command.MemberId, failure));
        error.InnerException.ShouldBeOfType<MySqlException>().Number.ShouldBe(1644);
        failure.WorldFailureInjected.ShouldBeTrue();
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraWorld>().SingleAsync()).State.ShouldBe(EraWorldState.Ready);
            (await context.Set<EraEquipmentDrop>().SingleAsync()).Status.ShouldBe(EraEquipmentDropStatus.Available);
            (await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Location == EraEquipmentLocation.Ground)).Version.ShouldBe(1);
        });
        (await EndDropWorldAsync(world, command.MemberId)).Data!.State.ShouldBe(EraWorldState.Ended);
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Non_host_and_left_or_disabled_members_should_not_gain_drop_permissions()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = await ReadyDropWorldAsync(team);
        var command = GroundCommand(world, team);
        command.MemberId = team.Team.Members.Single(member => member.PlayerId == _otherPlayerId).MemberId;
        (await Should.ThrowAsync<EraWorldAccessException>(() => SubmitGroundAsync(command, accountId: _otherAccountId)))
            .ErrorCode.ShouldBe("era.world_host_required");
        command.MemberId = team.Team.Members.Single(member => member.PlayerId == _playerId).MemberId;
        await SubmitGroundAsync(command);
        var drop = (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops.Single();
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).IsDisabled = true;
            await context.SaveChangesAsync();
        });
        await Should.ThrowAsync<EraRewardException>(() => QueryDropsAsync(world.WorldInstanceId));
        await Should.ThrowAsync<EraRewardException>(() => PickupDropAsync(PickupCommand(world, drop)));
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).IsDisabled = false;
            await context.SaveChangesAsync();
        });
        var left = await RequestAsync((mediator, token) => mediator.SendAsync<LeaveEraWorldCommand, LeaveEraWorldResponse>(new LeaveEraWorldCommand
        {
            WorldInstanceId = world.WorldInstanceId, MemberId = command.MemberId, AuthorityEpoch = 1, ExpectedWorldVersion = world.Version
        }, token));
        left.Data!.State.ShouldBe(EraWorldState.Ready);
        await Should.ThrowAsync<EraRewardException>(() => QueryDropsAsync(world.WorldInstanceId));
        await Should.ThrowAsync<EraRewardException>(() => PickupDropAsync(PickupCommand(world, drop)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("Category", "ERADropIntegration")]
    public async Task Last_member_leave_or_timeout_should_also_invalidate_unpicked_equipment(bool timeout)
    {
        _worldTestClock = new EraWorldTestClock(DateTimeOffset.UtcNow);
        await SeedAsync(0);
        await ConfigureRewardsAsync(source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        var command = GroundCommand(world, team);
        await SubmitGroundAsync(command);
        if (timeout)
        {
            await RemoveTestTeamAsync(team);
            _worldTestClock.Advance(TimeSpan.FromSeconds(60));
            (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Missing);
            _worldTestClock.Advance(TimeSpan.FromSeconds(15));
            (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Ended);
        }
        else
        {
            var left = await RequestAsync((mediator, token) => mediator.SendAsync<LeaveEraWorldCommand, LeaveEraWorldResponse>(new LeaveEraWorldCommand
            {
                WorldInstanceId = world.WorldInstanceId, MemberId = command.MemberId, AuthorityEpoch = 1, ExpectedWorldVersion = world.Version
            }, token));
            left.Data!.State.ShouldBe(EraWorldState.Ended);
        }
        await ReadDatabaseAsync(async context =>
        {
            var drop = await context.Set<EraEquipmentDrop>().SingleAsync();
            drop.Status.ShouldBe(EraEquipmentDropStatus.Invalidated);
            drop.ActiveEquipmentId.ShouldBeNull();
            (await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Id == drop.EquipmentId)).Location.ShouldBe(EraEquipmentLocation.Destroyed);
        });
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Pickup_request_id_should_conflict_with_an_existing_equip_receipt_for_the_player()
    {
        await SeedAsync();
        await ConfigureRewardsAsync(source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        await SubmitGroundAsync(GroundCommand(world, team));
        var equip = Equip(_inventory[0]);
        (await SendEquipAsync(equip)).Code.ShouldBe(HttpStatusCode.OK);
        var drop = (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops.Single();
        var command = PickupCommand(world, drop, 4);
        command.RequestId = equip.RequestId;
        (await Should.ThrowAsync<EraRewardException>(() => PickupDropAsync(command))).ErrorCode.ShouldBe("era.drop_idempotency_conflict");
        (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops.Single().DropId.ShouldBe(drop.DropId);
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Missing_current_configuration_should_not_regenerate_or_block_existing_drop_pickup()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        await SubmitGroundAsync(GroundCommand(world, team));
        var original = (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops.Single();
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentConfiguration>().SingleAsync()).Payload = "invalid manifest";
            (await context.Set<EraRewardConfiguration>().SingleAsync()).Payload = "invalid reward configuration";
            await context.SaveChangesAsync();
        });
        var query = (await QueryDropsAsync(world.WorldInstanceId)).Data!;
        query.ManifestVersion.ShouldBeNull();
        query.Drops.Single().Definition.ShouldBeNull();
        JsonSerializer.Serialize(query.Drops.Single().ArmorAttributes).ShouldBe(JsonSerializer.Serialize(original.ArmorAttributes));
        var picked = await PickupDropAsync(PickupCommand(world, query.Drops.Single()));
        picked.Data!.State.InventoryItems.Single().Id.ShouldBe(original.EquipmentId);
        JsonSerializer.Serialize(picked.Data.State.InventoryItems.Single().ArmorAttributes).ShouldBe(JsonSerializer.Serialize(original.ArmorAttributes));
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Original_share_roster_should_include_a_current_teammate_without_generating_a_drop_before_world_join()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        await team.Provider.ChangeAsync(team.Team.TeamId, current =>
        {
            current!.Members.Add(new EraTeamMember
            {
                PlayerId = _otherPlayerId, CharacterId = _otherCharacterId, MemberId = Guid.NewGuid(), CanHost = true,
                JoinedAt = DateTimeOffset.UtcNow, ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
            });
            return new EraTeamChange<bool> { Changed = true, Team = current, Result = true };
        }, CancellationToken.None);
        var generated = await SubmitGroundAsync(GroundCommand(world, team));
        generated.Data!.Grants.Single().PlayerId.ShouldBe(_playerId);
        await Should.ThrowAsync<EraRewardException>(() => QueryDropsAsync(world.WorldInstanceId, _otherAccountId));
        await ReadDatabaseAsync(async context =>
        {
            var share = await context.Set<EraEquipmentShare>().SingleAsync();
            JsonSerializer.Deserialize<List<Guid>>(share.AllowedPlayerIdsPayload)!.OrderBy(id => id)
                .ShouldBe(new[] { _playerId, _otherPlayerId }.OrderBy(id => id));
            (await context.Set<EraWorldMember>().CountAsync()).ShouldBe(1);
        });
    }
}
