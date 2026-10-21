using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    [Fact]
    [Trait("Category", "ERARewardIntegration")]
    public async Task Concurrent_requests_for_the_same_business_reward_should_persist_one_plan_and_two_request_aliases()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync();
        var first = RewardCommand();
        var second = JsonSerializer.Deserialize<PrepareEraInventoryRewardCommand>(JsonSerializer.Serialize(first))!;
        second.RequestId = Guid.NewGuid();
        var paused = new EraIntegrationSqlInterceptor { PauseRewardSave = true };
        var observer = new EraIntegrationSqlInterceptor();
        var responses = await RaceRewardsAsync(
            token => PrepareRewardAsync(first, paused, token), token => PrepareRewardAsync(second, observer, token), paused, observer);
        responses[0].IsReplay.ShouldBeFalse();
        responses[1].IsReplay.ShouldBeTrue();
        responses[1].Data.ShouldBe(responses[0].Data);
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardEvent>().CountAsync()).ShouldBe(1);
            (await context.Set<EraRewardRequest>().CountAsync()).ShouldBe(2);
            (await context.Set<EraRewardGrant>().CountAsync()).ShouldBe(0);
        });
    }

    [Fact]
    [Trait("Category", "ERARewardIntegration")]
    public async Task Concurrent_grants_should_create_one_batch_and_replay_the_committed_receipt()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(count: 2);
        var prepared = await PrepareRewardAsync(RewardCommand(includeOther: true));
        var paused = new EraIntegrationSqlInterceptor { PauseRewardGrantSave = true };
        var observer = new EraIntegrationSqlInterceptor();
        var responses = await RaceRewardsAsync(
            token => GrantRewardAsync(prepared.Data, paused, token), token => GrantRewardAsync(prepared.Data, observer, token), paused, observer);
        responses[0].IsReplay.ShouldBeFalse();
        responses[1].IsReplay.ShouldBeTrue();
        JsonSerializer.Serialize(responses[1].Data).ShouldBe(JsonSerializer.Serialize(responses[0].Data));
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardGrant>().CountAsync()).ShouldBe(2);
            (await context.Set<EraRewardGrantEquipment>().CountAsync()).ShouldBe(4);
            (await context.Set<EraEquipmentInstance>().CountAsync()).ShouldBe(5);
            (await context.Set<EraCharacter>().ToListAsync()).ShouldAllBe(character => character.EquipmentVersion == 4);
        });
    }

    [Fact]
    [Trait("Category", "ERARewardIntegration")]
    public async Task Different_events_competing_for_the_last_inventory_slot_should_commit_only_one_reward()
    {
        await SeedAsync(119);
        await ConfigureRewardsAsync();
        // 两个不同作用域没有共同事件锁，仍必须通过玩家／角色锁串行化容量检查。
        var first = await PrepareRewardAsync(RewardCommand());
        var second = await PrepareRewardAsync(RewardCommand());
        var paused = new EraIntegrationSqlInterceptor { PauseRewardGrantSave = true };
        var observer = new EraIntegrationSqlInterceptor();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var firstTask = GrantRewardAsync(first.Data, paused, timeout.Token);
        Task<GrantEraInventoryRewardResponse>? secondTask = null;
        try
        {
            await Task.WhenAny(paused.RewardSaveReached.Task, firstTask).WaitAsync(timeout.Token);
            if (firstTask.IsCompleted) await firstTask;
            await paused.RewardSaveReached.Task.WaitAsync(timeout.Token);
            secondTask = GrantRewardAsync(second.Data, observer, timeout.Token);
            await Task.WhenAny(observer.PlayerLockReached.Task, secondTask).WaitAsync(timeout.Token);
            if (secondTask.IsCompleted) await secondTask;
            await observer.PlayerLockReached.Task.WaitAsync(timeout.Token);
            secondTask.IsCompleted.ShouldBeFalse();
            paused.ResumeRewardSave.TrySetResult();
            (await firstTask).IsReplay.ShouldBeFalse();
            (await Should.ThrowAsync<EraRewardException>(() => secondTask)).ErrorCode.ShouldBe("era.reward_inventory_full");
        }
        finally
        {
            paused.ResumeRewardSave.TrySetResult();
            await timeout.CancelAsync();
            try
            {
                if (secondTask == null) await firstTask;
                else await Task.WhenAll(firstTask, secondTask);
            }
            catch { }
        }
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardGrant>().CountAsync()).ShouldBe(1);
            (await context.Set<EraRewardGrantEquipment>().CountAsync()).ShouldBe(1);
            (await context.Set<EraEquipmentInstance>().CountAsync(row => row.CharacterId == _characterId)).ShouldBe(120);
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion.ShouldBe(4);
            (await context.Set<EraRewardEvent>().SingleAsync(row => row.Id == second.Data)).ResultPayload.ShouldBeNull();
        });
    }

    private static async Task<T[]> RaceRewardsAsync<T>(Func<CancellationToken, Task<T>> first, Func<CancellationToken, Task<T>> second,
        EraIntegrationSqlInterceptor paused, EraIntegrationSqlInterceptor observer)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var firstTask = first(timeout.Token);
        Task<T>? secondTask = null;
        try
        {
            await Task.WhenAny(paused.RewardSaveReached.Task, firstTask).WaitAsync(timeout.Token);
            if (firstTask.IsCompleted) await firstTask;
            await paused.RewardSaveReached.Task.WaitAsync(timeout.Token);
            secondTask = second(timeout.Token);
            await Task.WhenAny(observer.RewardScopeReached.Task, secondTask).WaitAsync(timeout.Token);
            if (secondTask.IsCompleted) await secondTask;
            await observer.RewardScopeReached.Task.WaitAsync(timeout.Token);
            secondTask.IsCompleted.ShouldBeFalse();
            paused.ResumeRewardSave.TrySetResult();
            return await Task.WhenAll(firstTask, secondTask);
        }
        finally
        {
            paused.ResumeRewardSave.TrySetResult();
            await timeout.CancelAsync();
            try
            {
                if (secondTask == null) await firstTask;
                else await Task.WhenAll(firstTask, secondTask);
            }
            catch
            {
                // 清理请求连接后保留原始失败，不让 Respawn 与尚未结束的事务竞争。
            }
        }
    }
}
