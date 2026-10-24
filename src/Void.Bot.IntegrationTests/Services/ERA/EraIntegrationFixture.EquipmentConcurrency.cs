using System.Net;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    [Fact]
    [Trait("Category", "ERAEquipmentIntegration")]
    public async Task Concurrent_same_slot_commands_should_commit_one_and_reject_the_stale_version()
    {
        await SeedAsync(2, equipped: true);
        var first = Equip(_inventory[0]);
        var second = Equip(_inventory[1]);

        var responses = await RaceAsync(first, second);

        responses[0].Code.ShouldBe(HttpStatusCode.OK);
        responses[0].IsReplay.ShouldBeFalse();
        responses[1].Code.ShouldBe(HttpStatusCode.Conflict);
        responses[1].ErrorCode.ShouldBe("era.equipment_version_conflict");
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion.ShouldBe(4);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync(row => row.PlayerId == _playerId)).ShouldBe(1);
            var items = await context.Set<EraEquipmentInstance>().Where(row => row.CharacterId == _characterId).ToListAsync();
            items.Count.ShouldBe(3);
            items.Count(item => item.Location == EraEquipmentLocation.Equipped).ShouldBe(1);
            var winner = items.Single(item => item.Id == first.EquipmentId);
            winner.Location.ShouldBe(EraEquipmentLocation.Equipped);
            winner.EquippedSlot.ShouldBe(EraEquipmentSlot.Head);
            winner.Version.ShouldBe(6);
            var loser = items.Single(item => item.Id == second.EquipmentId);
            loser.Location.ShouldBe(EraEquipmentLocation.Inventory);
            loser.EquippedSlot.ShouldBeNull();
            loser.Version.ShouldBe(5);
            var old = items.Single(item => item.Id == _equipped!.Id);
            old.Location.ShouldBe(EraEquipmentLocation.Inventory);
            old.Version.ShouldBe(6);
        });
    }

    [Fact]
    [Trait("Category", "ERAEquipmentIntegration")]
    public async Task Concurrent_identical_requests_should_commit_once_and_replay_the_same_receipt()
    {
        await SeedAsync();
        var first = Equip(_inventory[0]);
        var second = Equip(_inventory[0], first.RequestId);

        var responses = await RaceAsync(first, second);

        responses.ShouldAllBe(response => response.Code == HttpStatusCode.OK);
        responses[0].IsReplay.ShouldBeFalse();
        responses[1].IsReplay.ShouldBeTrue();
        foreach (var response in responses)
        {
            response.Data!.RequestId.ShouldBe(first.RequestId);
            response.Data.State.EquipmentVersion.ShouldBe(4);
            response.Data.State.EquippedSlots.Single(slot => slot.Slot == EraEquipmentSlot.Head).Equipment!.Id.ShouldBe(first.EquipmentId);
        }
        await ReadDatabaseAsync(async context =>
        {
            var character = await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId);
            character.EquipmentVersion.ShouldBe(4);
            character.Version.ShouldBe(12);
            var item = await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Id == first.EquipmentId);
            item.Location.ShouldBe(EraEquipmentLocation.Equipped);
            item.Version.ShouldBe(6);
            (await context.Set<EraEquipmentInstance>().CountAsync(row => row.CharacterId == _characterId)).ShouldBe(1);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync(row => row.PlayerId == _playerId)).ShouldBe(1);
        });
    }

    private async Task<EquipEraEquipmentResponse[]> RaceAsync(EquipEraEquipmentCommand first, EquipEraEquipmentCommand second)
    {
        var paused = new EraIntegrationSqlInterceptor { PauseFirstUpdate = true };
        var observer = new EraIntegrationSqlInterceptor();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        // 每个请求都有独立 Autofac 作用域和 DbContext；不能并发使用同一 EF 上下文。
        var firstTask = SendEquipAsync(first, paused, timeout.Token);
        Task<EquipEraEquipmentResponse>? secondTask = null;
        try
        {
            await Task.WhenAny(paused.UpdateReached.Task, firstTask).WaitAsync(timeout.Token);
            if (firstTask.IsCompleted) await firstTask; // 在到达闸门前失败时保留原异常。
            await paused.UpdateReached.Task.WaitAsync(timeout.Token);
            secondTask = SendEquipAsync(second, observer, timeout.Token);
            await Task.WhenAny(observer.PlayerLockReached.Task, secondTask).WaitAsync(timeout.Token);
            if (secondTask.IsCompleted) await secondTask;
            await observer.PlayerLockReached.Task.WaitAsync(timeout.Token);
            secondTask.IsCompleted.ShouldBeFalse();
            paused.ResumeUpdate.TrySetResult();
            return await Task.WhenAll(firstTask, secondTask);
        }
        finally
        {
            paused.ResumeUpdate.TrySetResult();
            await timeout.CancelAsync();
            // 断言或超时失败也先收束全部连接，之后才由基类 Respawn 清理数据库。
            try
            {
                if (secondTask == null) await firstTask;
                else await Task.WhenAll(firstTask, secondTask);
            }
            catch
            {
                // 原始请求或断言异常由 try 主体传播，清理不能覆盖它。
            }
        }
    }
}
