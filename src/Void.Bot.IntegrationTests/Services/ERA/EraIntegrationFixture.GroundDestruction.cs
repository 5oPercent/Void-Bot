using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    private DestroyEraEquipmentDropsCommand DestroyDropsCommand(EraWorldDto world, IEnumerable<EraEquipmentDropDto> drops,
        long equipmentVersion = 3, Guid? characterId = null)
    {
        return new DestroyEraEquipmentDropsCommand
        {
            WorldInstanceId = world.WorldInstanceId, CharacterId = characterId ?? _characterId,
            RequestId = Guid.NewGuid(), ExpectedEquipmentVersion = equipmentVersion,
            Items = drops.Select(drop => new EraEquipmentDropDestroyItemDto
            {
                DropId = drop.DropId, ExpectedDropVersion = drop.DropVersion, ExpectedItemVersion = drop.EquipmentVersion
            }).ToList()
        };
    }

    private Task<DestroyEraEquipmentDropsResponse> DestroyDropsAsync(DestroyEraEquipmentDropsCommand command,
        EraIntegrationSqlInterceptor? interceptor = null, CancellationToken cancellationToken = default, Guid? accountId = null)
    {
        return RequestAsync((mediator, token) => mediator.SendAsync<DestroyEraEquipmentDropsCommand, DestroyEraEquipmentDropsResponse>(command, token),
            interceptor, cancellationToken, accountId);
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Ground_destruction_should_work_with_full_inventory_and_replay_the_entire_batch()
    {
        await SeedAsync(120);
        await ConfigureRewardsAsync(count: 2, source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        var generation = GroundCommand(world, team);
        await SubmitGroundAsync(generation);
        var drops = (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops;
        var command = DestroyDropsCommand(world, drops);
        var result = await DestroyDropsAsync(command);
        result.Code.ShouldBe(HttpStatusCode.OK);
        result.Data!.Operation.ShouldBe(EraEquipmentOperation.DestroyGround);
        result.Data.State.InventoryItems.Count.ShouldBe(120);
        result.Data.State.EquipmentVersion.ShouldBe(4);
        result.Data.DestroyedDrops!.Select(drop => drop.DropId).OrderBy(id => id).ShouldBe(drops.Select(drop => drop.DropId).OrderBy(id => id));
        (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops.ShouldBeEmpty();
        command.Items.Reverse();
        var replay = await DestroyDropsAsync(command);
        replay.IsReplay.ShouldBeTrue();
        JsonSerializer.Serialize(replay.Data).ShouldBe(JsonSerializer.Serialize(result.Data));
        command.Items[0].ExpectedItemVersion++;
        (await Should.ThrowAsync<EraRewardException>(() => DestroyDropsAsync(command))).ErrorCode.ShouldBe("era.drop_idempotency_conflict");
        command.Items[0].ExpectedItemVersion--;
        await EndDropWorldAsync(world, generation.MemberId);
        (await DestroyDropsAsync(command)).IsReplay.ShouldBeTrue();
        await ReadDatabaseAsync(async context =>
        {
            var ids = drops.Select(drop => drop.DropId).ToArray();
            var saved = await context.Set<EraEquipmentDrop>().Where(drop => ids.Contains(drop.Id)).ToListAsync();
            saved.ShouldAllBe(drop => drop.Status == EraEquipmentDropStatus.Destroyed && drop.ActiveEquipmentId == null && drop.Version == 2);
            var equipmentIds = drops.Select(drop => drop.EquipmentId).ToArray();
            (await context.Set<EraEquipmentInstance>().Where(item => equipmentIds.Contains(item.Id)).ToListAsync())
                .ShouldAllBe(item => item.Location == EraEquipmentLocation.Destroyed && item.Version == 2 &&
                    item.OwnerPlayerId == null && item.CharacterId == null);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync()).ShouldBe(1);
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion.ShouldBe(4);
        });
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Ground_destruction_should_reject_a_batch_containing_another_players_private_drop()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = await ReadyDropWorldAsync(team);
        await SubmitGroundAsync(GroundCommand(world, team));
        var own = (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops.Single();
        var other = (await QueryDropsAsync(world.WorldInstanceId, _otherAccountId)).Data!.Drops.Single();
        var command = DestroyDropsCommand(world, new[] { own, other });
        (await Should.ThrowAsync<EraRewardException>(() => DestroyDropsAsync(command))).ErrorCode.ShouldBe("era.drop_not_found");
        (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops.Single().DropId.ShouldBe(own.DropId);
        (await QueryDropsAsync(world.WorldInstanceId, _otherAccountId)).Data!.Drops.Single().DropId.ShouldBe(other.DropId);
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync()).ShouldBe(0);
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion.ShouldBe(3);
        });
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Shared_ground_destruction_should_allow_roster_players_but_only_the_dropper_after_expiry()
    {
        await SeedAsync(0);
        _worldTestClock = new EraWorldTestClock(DateTimeOffset.UtcNow);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = await ReadyDropWorldAsync(team);
        var authority = GroundCommand(world, team);
        var grant = new GrantEraGroundRewardCommand
        {
            WorldInstanceId = world.WorldInstanceId, CharacterId = _characterId, RequestId = Guid.NewGuid(),
            MemberId = authority.MemberId, AuthorityEpoch = authority.AuthorityEpoch, Placement = authority.Placement, IsPublic = true,
            Entries = new List<EraEquipmentRewardEntry>
            {
                new() { DefinitionId = Guid.Parse("10000000-0000-0000-0000-000000000001"), Count = 2 }
            }
        };
        await RequestAsync((mediator, token) => mediator.SendAsync<GrantEraGroundRewardCommand, SubmitEraGroundRewardResponse>(grant, token));
        var drops = (await QueryDropsAsync(world.WorldInstanceId, _otherAccountId)).Data!.Drops;
        (await DestroyDropsAsync(DestroyDropsCommand(world, new[] { drops[0] }, characterId: _otherCharacterId),
            accountId: _otherAccountId)).Code.ShouldBe(HttpStatusCode.OK);
        _worldTestClock.Advance(TimeSpan.FromHours(1));
        var expired = DestroyDropsCommand(world, new[] { drops[1] }, 4, _otherCharacterId);
        (await Should.ThrowAsync<EraRewardException>(() => DestroyDropsAsync(expired, accountId: _otherAccountId)))
            .ErrorCode.ShouldBe("era.drop_not_found");
        (await DestroyDropsAsync(DestroyDropsCommand(world, new[] { drops[1] }))).Code.ShouldBe(HttpStatusCode.OK);
        (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Stale_items_and_receipt_save_failures_should_leave_the_entire_ground_batch_unchanged()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(count: 2, source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        await SubmitGroundAsync(GroundCommand(world, team));
        var drops = (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops;
        var command = DestroyDropsCommand(world, drops);
        command.Items[1].ExpectedItemVersion++;
        (await Should.ThrowAsync<EraRewardException>(() => DestroyDropsAsync(command))).ErrorCode.ShouldBe("era.drop_version_conflict");
        command.Items[1].ExpectedItemVersion--;
        var failure = new EraIntegrationSqlInterceptor { FailReceiptInsert = true };
        await Should.ThrowAsync<DbUpdateException>(() => DestroyDropsAsync(command, failure));
        failure.ReceiptFailureInjected.ShouldBeTrue();
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentDrop>().ToListAsync()).ShouldAllBe(drop =>
                drop.Status == EraEquipmentDropStatus.Available && drop.ActiveEquipmentId == drop.EquipmentId && drop.Version == 1);
            (await context.Set<EraEquipmentInstance>().Where(item => item.Location == EraEquipmentLocation.Ground).CountAsync()).ShouldBe(2);
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion.ShouldBe(3);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync()).ShouldBe(0);
        });
        (await DestroyDropsAsync(command)).Code.ShouldBe(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("Category", "ERADropIntegration")]
    public async Task Shared_pickup_and_ground_destruction_should_have_only_one_winner(bool destroyFirst)
    {
        await SeedAsync();
        await SeedShareAsync(_inventory[0], _playerId, _otherPlayerId);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = await ReadyDropWorldAsync(team);
        await DropInventoryAsync(DropCommand(world, _inventory[0].Id));
        var drop = (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops.Single();
        var destroy = DestroyDropsCommand(world, new[] { drop }, 4);
        var pickup = PickupCommand(world, drop);
        pickup.CharacterId = _otherCharacterId;
        var paused = new EraIntegrationSqlInterceptor { PauseDropSave = true };
        var observer = new EraIntegrationSqlInterceptor();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        Task first = destroyFirst ? DestroyDropsAsync(destroy, paused, timeout.Token)
            : PickupDropAsync(pickup, paused, timeout.Token, _otherAccountId);
        Task? second = null;
        try
        {
            await Task.WhenAny(paused.DropSaveReached.Task, first).WaitAsync(timeout.Token);
            if (first.IsCompleted) await first;
            await paused.DropSaveReached.Task.WaitAsync(timeout.Token);
            second = destroyFirst ? PickupDropAsync(pickup, observer, timeout.Token, _otherAccountId)
                : DestroyDropsAsync(destroy, observer, timeout.Token);
            await Task.WhenAny(observer.TeamLockReached.Task, second).WaitAsync(timeout.Token);
            if (second.IsCompleted) await second;
            await observer.TeamLockReached.Task.WaitAsync(timeout.Token);
            second.IsCompleted.ShouldBeFalse();
            paused.ResumeDropSave.TrySetResult();
            await first;
            (await Should.ThrowAsync<EraRewardException>(() => second)).ErrorCode.ShouldBe("era.drop_version_conflict");
        }
        finally
        {
            paused.ResumeDropSave.TrySetResult();
            await timeout.CancelAsync();
            try { await Task.WhenAll(second == null ? new[] { first } : new[] { first, second }); }
            catch { /* 收束并发请求，保留原断言异常。 */ }
        }
        await ReadDatabaseAsync(async context =>
        {
            var savedDrop = await context.Set<EraEquipmentDrop>().SingleAsync(row => row.Id == drop.DropId);
            savedDrop.Status.ShouldBe(destroyFirst ? EraEquipmentDropStatus.Destroyed : EraEquipmentDropStatus.PickedUp);
            var savedItem = await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Id == drop.EquipmentId);
            savedItem.Location.ShouldBe(destroyFirst ? EraEquipmentLocation.Destroyed : EraEquipmentLocation.Inventory);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync()).ShouldBe(2); // 丢出与唯一成功的消费。
        });
    }
}
