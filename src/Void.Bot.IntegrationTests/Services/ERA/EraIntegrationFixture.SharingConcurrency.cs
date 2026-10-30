using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("Category", "ERADropIntegration")]
    public async Task Concurrent_inventory_drops_should_create_one_record_and_replay_only_the_same_request(bool sameRequest)
    {
        await SeedAsync();
        await SeedShareAsync(_inventory[0], _playerId);
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        var first = DropCommand(world, _inventory[0].Id);
        var second = JsonSerializer.Deserialize<DropEraInventoryEquipmentCommand>(JsonSerializer.Serialize(first))!;
        if (!sameRequest)
        {
            second.RequestId = Guid.NewGuid();
        }
        var paused = new EraIntegrationSqlInterceptor { PauseDropSave = true };
        var observer = new EraIntegrationSqlInterceptor();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var firstTask = DropInventoryAsync(first, paused, timeout.Token);
        Task<DropEraInventoryEquipmentResponse>? secondTask = null;
        try
        {
            await Task.WhenAny(paused.DropSaveReached.Task, firstTask).WaitAsync(timeout.Token);
            if (firstTask.IsCompleted)
            {
                await firstTask;
            }
            await paused.DropSaveReached.Task.WaitAsync(timeout.Token);
            secondTask = DropInventoryAsync(second, observer, timeout.Token);
            await Task.WhenAny(observer.PlayerLockReached.Task, secondTask).WaitAsync(timeout.Token);
            if (secondTask.IsCompleted)
            {
                await secondTask;
            }
            await observer.PlayerLockReached.Task.WaitAsync(timeout.Token);
            secondTask.IsCompleted.ShouldBeFalse();
            paused.ResumeDropSave.TrySetResult();
            var saved = await firstTask;
            saved.Code.ShouldBe(HttpStatusCode.OK);
            if (sameRequest)
            {
                var replay = await secondTask;
                replay.IsReplay.ShouldBeTrue();
                JsonSerializer.Serialize(replay.Data).ShouldBe(JsonSerializer.Serialize(saved.Data));
            }
            else
            {
                (await Should.ThrowAsync<EraRewardException>(() => secondTask)).ErrorCode.ShouldBe("era.drop_version_conflict");
            }
        }
        finally
        {
            paused.ResumeDropSave.TrySetResult();
            await timeout.CancelAsync();
            try
            {
                if (secondTask == null)
                {
                    await firstTask;
                }
                else
                {
                    await Task.WhenAll(firstTask, secondTask);
                }
            }
            catch
            {
                // 收束全部请求后清理，保留主断言异常。
            }
        }
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentDrop>().CountAsync()).ShouldBe(1);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync()).ShouldBe(1);
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion.ShouldBe(4);
        });
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Two_original_roster_players_should_not_both_pick_up_the_same_shared_drop()
    {
        await SeedAsync();
        await SeedShareAsync(_inventory[0], _playerId, _otherPlayerId);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = await ReadyDropWorldAsync(team);
        await DropInventoryAsync(DropCommand(world, _inventory[0].Id));
        var drop = (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops.Single();
        var first = PickupCommand(world, drop, 4);
        var second = PickupCommand(world, drop);
        second.CharacterId = _otherCharacterId;
        var paused = new EraIntegrationSqlInterceptor { PauseDropSave = true };
        var observer = new EraIntegrationSqlInterceptor();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var firstTask = PickupDropAsync(first, paused, timeout.Token);
        Task<PickupEraEquipmentDropResponse>? secondTask = null;
        try
        {
            await Task.WhenAny(paused.DropSaveReached.Task, firstTask).WaitAsync(timeout.Token);
            if (firstTask.IsCompleted)
            {
                await firstTask;
            }
            await paused.DropSaveReached.Task.WaitAsync(timeout.Token);
            secondTask = PickupDropAsync(second, observer, timeout.Token, _otherAccountId);
            await Task.WhenAny(observer.TeamLockReached.Task, secondTask).WaitAsync(timeout.Token);
            if (secondTask.IsCompleted)
            {
                await secondTask;
            }
            await observer.TeamLockReached.Task.WaitAsync(timeout.Token);
            secondTask.IsCompleted.ShouldBeFalse();
            paused.ResumeDropSave.TrySetResult();
            (await firstTask).Code.ShouldBe(HttpStatusCode.OK);
            (await Should.ThrowAsync<EraRewardException>(() => secondTask)).ErrorCode.ShouldBe("era.drop_version_conflict");
        }
        finally
        {
            paused.ResumeDropSave.TrySetResult();
            await timeout.CancelAsync();
            try
            {
                if (secondTask == null)
                {
                    await firstTask;
                }
                else
                {
                    await Task.WhenAll(firstTask, secondTask);
                }
            }
            catch
            {
                // 不让任务取消或清理异常覆盖原始并发断言。
            }
        }
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentDrop>().SingleAsync()).Status.ShouldBe(EraEquipmentDropStatus.PickedUp);
            (await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Id == _inventory[0].Id)).OwnerPlayerId.ShouldBe(_playerId);
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _otherCharacterId)).EquipmentVersion.ShouldBe(3);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync()).ShouldBe(2);
        });
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task World_end_waiting_for_inventory_drop_should_destroy_the_committed_shared_drop_once()
    {
        await SeedAsync();
        await SeedShareAsync(_inventory[0], _playerId, _otherPlayerId);
        await using var team = await CreateWorldTeamAsync(includeOther: true, otherHost: true);
        var world = await ReadyDropWorldAsync(team);
        var command = DropCommand(world, _inventory[0].Id);
        var paused = new EraIntegrationSqlInterceptor { PauseDropSave = true };
        var observer = new EraIntegrationSqlInterceptor();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var dropping = DropInventoryAsync(command, paused, timeout.Token);
        Task<EndEraWorldResponse>? ending = null;
        try
        {
            await Task.WhenAny(paused.DropSaveReached.Task, dropping).WaitAsync(timeout.Token);
            if (dropping.IsCompleted)
            {
                await dropping;
            }
            await paused.DropSaveReached.Task.WaitAsync(timeout.Token);
            ending = EndDropWorldAsync(world, team.Team.Members[1].MemberId, observer, timeout.Token, _otherAccountId);
            await Task.WhenAny(observer.TeamLockReached.Task, ending).WaitAsync(timeout.Token);
            if (ending.IsCompleted)
            {
                await ending;
            }
            await observer.TeamLockReached.Task.WaitAsync(timeout.Token);
            ending.IsCompleted.ShouldBeFalse();
            paused.ResumeDropSave.TrySetResult();
            (await dropping).Code.ShouldBe(HttpStatusCode.OK);
            (await ending).Code.ShouldBe(HttpStatusCode.OK);
        }
        finally
        {
            paused.ResumeDropSave.TrySetResult();
            await timeout.CancelAsync();
            try
            {
                if (ending == null)
                {
                    await dropping;
                }
                else
                {
                    await Task.WhenAll(dropping, ending);
                }
            }
            catch
            {
                // 两个事务收束后才交给 Respawn，保留主异常。
            }
        }
        (await DropInventoryAsync(command)).IsReplay.ShouldBeTrue();
        await ReadDatabaseAsync(async context =>
        {
            var drop = await context.Set<EraEquipmentDrop>().SingleAsync();
            drop.Status.ShouldBe(EraEquipmentDropStatus.Invalidated);
            drop.ActiveEquipmentInstanceId.ShouldBeNull();
            drop.Version.ShouldBe(2);
            var item = await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Id == _inventory[0].Id);
            item.Location.ShouldBe(EraEquipmentLocation.Destroyed);
            item.Version.ShouldBe(7);
            (await context.Set<EraEquipmentShare>().CountAsync()).ShouldBe(1);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync()).ShouldBe(1);
        });
    }
}
