using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Services.ERA.Team;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Concurrent_ground_submissions_should_create_one_batch_for_the_business_event()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = await ReadyDropWorldAsync(team);
        var first = GroundCommand(world, team);
        var second = JsonSerializer.Deserialize<SubmitEraGroundRewardCommand>(JsonSerializer.Serialize(first))!;
        second.RequestId = Guid.NewGuid();
        var paused = new EraIntegrationSqlInterceptor { PauseRewardSave = true };
        var observer = new EraIntegrationSqlInterceptor();
        var responses = await RaceRewardsAsync(token => SubmitGroundAsync(first, paused, cancellationToken: token),
            token => SubmitGroundAsync(second, observer, cancellationToken: token), paused, observer);
        responses[0].IsReplay.ShouldBeFalse();
        responses[1].IsReplay.ShouldBeTrue();
        JsonSerializer.Serialize(responses[0].Data).ShouldBe(JsonSerializer.Serialize(responses[1].Data));
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardEvent>().CountAsync()).ShouldBe(1);
            (await context.Set<EraRewardRequest>().CountAsync()).ShouldBe(2);
            (await context.Set<EraEquipmentDrop>().CountAsync()).ShouldBe(2);
            (await context.Set<EraEquipmentShare>().CountAsync()).ShouldBe(2);
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("Category", "ERADropIntegration")]
    public async Task Concurrent_pickups_should_replay_same_request_or_reject_a_second_last_slot_write(bool differentDrop)
    {
        await SeedAsync(119);
        await ConfigureRewardsAsync(count: differentDrop ? 2 : 1, source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        await SubmitGroundAsync(GroundCommand(world, team));
        var drops = (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops;
        var first = PickupCommand(world, drops[0]);
        var second = differentDrop ? PickupCommand(world, drops[1])
            : JsonSerializer.Deserialize<PickupEraEquipmentDropCommand>(JsonSerializer.Serialize(first))!;
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
            secondTask = PickupDropAsync(second, observer, timeout.Token);
            await Task.WhenAny(observer.PlayerLockReached.Task, secondTask).WaitAsync(timeout.Token);
            if (secondTask.IsCompleted)
            {
                await secondTask;
            }
            await observer.PlayerLockReached.Task.WaitAsync(timeout.Token);
            secondTask.IsCompleted.ShouldBeFalse();
            paused.ResumeDropSave.TrySetResult();
            (await firstTask).Code.ShouldBe(HttpStatusCode.OK);
            if (differentDrop)
            {
                (await Should.ThrowAsync<EraRewardException>(() => secondTask)).ErrorCode.ShouldBe("era.drop_version_conflict");
                second.ExpectedEquipmentVersion = 4;
                (await Should.ThrowAsync<EraRewardException>(() => PickupDropAsync(second))).ErrorCode.ShouldBe("era.drop_inventory_full");
            }
            else
            {
                (await secondTask).IsReplay.ShouldBeTrue();
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
                // 原始请求异常或断言保留，所有连接收束后再交给 Respawn 清理。
            }
        }
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentInstance>().CountAsync(row => row.CharacterId == _characterId)).ShouldBe(120);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync()).ShouldBe(1);
            (await context.Set<EraEquipmentDrop>().CountAsync(row => row.Status == EraEquipmentDropStatus.Available)).ShouldBe(differentDrop ? 1 : 0);
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion.ShouldBe(4);
        });
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Another_hosts_world_end_should_wait_for_pickup_and_preserve_the_owned_equipment()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync(includeOther: true, otherHost: true);
        var world = await ReadyDropWorldAsync(team);
        var generation = GroundCommand(world, team);
        await SubmitGroundAsync(generation, accountId: _otherAccountId);
        var drop = (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops.Single();
        var paused = new EraIntegrationSqlInterceptor { PauseDropSave = true };
        var observer = new EraIntegrationSqlInterceptor();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var pickup = PickupDropAsync(PickupCommand(world, drop), paused, timeout.Token);
        Task<EndEraWorldResponse>? end = null;
        try
        {
            await Task.WhenAny(paused.DropSaveReached.Task, pickup).WaitAsync(timeout.Token);
            if (pickup.IsCompleted)
            {
                await pickup;
            }
            await paused.DropSaveReached.Task.WaitAsync(timeout.Token);
            end = EndDropWorldAsync(world, generation.MemberId, observer, timeout.Token, _otherAccountId);
            await Task.WhenAny(observer.TeamLockReached.Task, end).WaitAsync(timeout.Token);
            if (end.IsCompleted)
            {
                await end;
            }
            await observer.TeamLockReached.Task.WaitAsync(timeout.Token);
            end.IsCompleted.ShouldBeFalse();
            paused.ResumeDropSave.TrySetResult();
            (await pickup).Code.ShouldBe(HttpStatusCode.OK);
            (await end).Data!.State.ShouldBe(EraWorldState.Ended);
        }
        finally
        {
            paused.ResumeDropSave.TrySetResult();
            await timeout.CancelAsync();
            try
            {
                if (end == null)
                {
                    await pickup;
                }
                else
                {
                    await Task.WhenAll(pickup, end);
                }
            }
            catch
            {
                // 收束独立请求，不覆盖原失败。
            }
        }
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Id == drop.EquipmentId)).Location.ShouldBe(EraEquipmentLocation.Inventory);
            (await context.Set<EraEquipmentDrop>().CountAsync(row => row.Status == EraEquipmentDropStatus.Available)).ShouldBe(0);
        });
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Redis_migration_should_wait_for_ground_generation_and_new_host_should_replay_original_results()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = await ReadyDropWorldAsync(team);
        var command = GroundCommand(world, team);
        var paused = new EraIntegrationSqlInterceptor { PauseRewardSave = true };
        var intent = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var generation = SubmitGroundAsync(command, paused, cancellationToken: timeout.Token);
        Task<EraTeam>? migration = null;
        SubmitEraGroundRewardResponse original;
        try
        {
            await Task.WhenAny(paused.RewardSaveReached.Task, generation).WaitAsync(timeout.Token);
            if (generation.IsCompleted)
            {
                await generation;
            }
            await paused.RewardSaveReached.Task.WaitAsync(timeout.Token);
            migration = team.Provider.ChangeAsync(team.Team.TeamId, current =>
            {
                EraTeamLifecycle.SelectNext(current!, DateTimeOffset.UtcNow, 30).ShouldBeTrue();
                intent.TrySetResult();
                return new EraTeamChange<EraTeam> { Changed = true, Team = current, Result = current! };
            }, timeout.Token);
            await intent.Task.WaitAsync(timeout.Token);
            migration.IsCompleted.ShouldBeFalse();
            (await team.Provider.GetTeamAsync(team.Team.TeamId, timeout.Token))!.AuthorityEpoch.ShouldBe(1);
            paused.ResumeRewardSave.TrySetResult();
            original = await generation;
            (await migration).AuthorityEpoch.ShouldBe(2);
        }
        finally
        {
            paused.ResumeRewardSave.TrySetResult();
            await timeout.CancelAsync();
            try
            {
                if (migration == null)
                {
                    await generation;
                }
                else
                {
                    await Task.WhenAll(generation, migration);
                }
            }
            catch
            {
                // 防止数据库或 Redis 请求越过测试清理边界。
            }
        }
        await Should.ThrowAsync<EraWorldAccessException>(() => SubmitGroundAsync(command));
        var newMember = team.Team.Members.Single(member => member.PlayerId == _otherPlayerId);
        await RequestAsync((mediator, token) => mediator.SendAsync<CompleteEraHostMigrationCommand, CompleteEraHostMigrationResponse>(new CompleteEraHostMigrationCommand
        {
            TeamId = world.TeamId, MemberId = newMember.MemberId, AuthorityEpoch = 2, Success = true
        }, token), accountId: _otherAccountId);
        await RequestAsync((mediator, token) => mediator.SendAsync<RecoverEraWorldCommand, RecoverEraWorldResponse>(new RecoverEraWorldCommand
        {
            WorldInstanceId = world.WorldInstanceId, MemberId = newMember.MemberId, AuthorityEpoch = 2, ExpectedWorldVersion = world.Version
        }, token), accountId: _otherAccountId);
        command.MemberId = newMember.MemberId;
        command.AuthorityEpoch = 2;
        command.RequestId = Guid.NewGuid();
        var replay = await SubmitGroundAsync(command, accountId: _otherAccountId);
        replay.IsReplay.ShouldBeTrue();
        JsonSerializer.Serialize(replay.Data).ShouldBe(JsonSerializer.Serialize(original.Data));
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardEvent>().CountAsync()).ShouldBe(1);
            (await context.Set<EraEquipmentDrop>().CountAsync()).ShouldBe(2);
        });
    }
}
