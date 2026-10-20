using System.Net;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    [Fact]
    [Trait("Category", "ERAWorldLifetimeIntegration")]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Heartbeat_committing_first_should_cancel_a_waiting_timeout_check()
    {
        _worldTestClock = new EraWorldTestClock(DateTimeOffset.UtcNow);
        await SeedAsync(0);
        await using var team = await CreateWorldTeamAsync();
        var world = await RegisterWorldAsync(team);
        await RemoveTestTeamAsync(team);
        _worldTestClock.Advance(TimeSpan.FromSeconds(60));
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Missing);
        _worldTestClock.Advance(TimeSpan.FromSeconds(15));
        var paused = new EraIntegrationSqlInterceptor { PauseWorldSave = true };
        var observer = new EraIntegrationSqlInterceptor();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var heartbeat = HeartbeatWorldAsync(world, team.Team.Members[0].MemberId, interceptor: paused, cancellationToken: timeout.Token);
        Task<CheckEraWorldTimeoutResponse>? check = null;
        try
        {
            await paused.WorldSaveReached.Task.WaitAsync(timeout.Token);
            check = CheckTimeoutAsync(world, observer, timeout.Token);
            await observer.TeamLockReached.Task.WaitAsync(timeout.Token);
            check.IsCompleted.ShouldBeFalse();
            paused.ResumeWorldSave.TrySetResult();
            (await heartbeat).Code.ShouldBe(HttpStatusCode.OK);
            (await check).Status.ShouldBe(EraWorldTimeoutStatus.Deferred);
        }
        finally
        {
            paused.ResumeWorldSave.TrySetResult();
            await timeout.CancelAsync();
            try { if (check == null) await heartbeat; else await Task.WhenAll(heartbeat, check); }
            catch { /* 请求全部收束后才由基类清理；保留主断言异常。 */ }
        }
        await ReadDatabaseAsync(async context =>
        {
            var saved = await context.Set<EraWorld>().SingleAsync();
            saved.State.ShouldBe(EraWorldState.Building);
            saved.Version.ShouldBe(1);
            saved.TimeoutObservedAt.ShouldBeNull();
            saved.LastActiveAt.ShouldBe(_worldTestClock.GetUtcNow());
        });
    }

    [Fact]
    [Trait("Category", "ERAWorldLifetimeIntegration")]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Timeout_committing_first_should_reject_waiting_heartbeat_without_reviving_world()
    {
        _worldTestClock = new EraWorldTestClock(DateTimeOffset.UtcNow);
        await SeedAsync(0);
        await using var team = await CreateWorldTeamAsync();
        var world = await RegisterWorldAsync(team);
        await RemoveTestTeamAsync(team);
        _worldTestClock.Advance(TimeSpan.FromSeconds(60));
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Missing);
        _worldTestClock.Advance(TimeSpan.FromSeconds(15));
        var paused = new EraIntegrationSqlInterceptor { PauseWorldSave = true };
        var observer = new EraIntegrationSqlInterceptor();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var check = CheckTimeoutAsync(world, paused, timeout.Token);
        Task<HeartbeatEraWorldResponse>? heartbeat = null;
        try
        {
            await paused.WorldSaveReached.Task.WaitAsync(timeout.Token);
            heartbeat = HeartbeatWorldAsync(world, team.Team.Members[0].MemberId, interceptor: observer, cancellationToken: timeout.Token);
            await observer.TeamLockReached.Task.WaitAsync(timeout.Token);
            heartbeat.IsCompleted.ShouldBeFalse();
            paused.ResumeWorldSave.TrySetResult();
            (await check).Status.ShouldBe(EraWorldTimeoutStatus.Ended);
            (await Should.ThrowAsync<EraWorldAccessException>(() => heartbeat)).ErrorCode.ShouldBe("era.world_ended");
        }
        finally
        {
            paused.ResumeWorldSave.TrySetResult();
            await timeout.CancelAsync();
            try { if (heartbeat == null) await check; else await Task.WhenAll(check, heartbeat); }
            catch { /* 保留原始断言异常。 */ }
        }
        await ReadDatabaseAsync(async context =>
        {
            var saved = await context.Set<EraWorld>().SingleAsync();
            saved.Version.ShouldBe(2);
            saved.EndReason.ShouldBe(EraWorldEndReason.InactivityTimeout);
            (await context.Set<EraWorldMember>().SingleAsync()).IsContinuing.ShouldBeFalse();
        });
    }

    [Fact]
    [Trait("Category", "ERAWorldLifetimeIntegration")]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Equipment_restriction_read_should_leave_member_rows_unlocked()
    {
        await SeedAsync();
        await using var team = await CreateWorldTeamAsync();
        var world = await RegisterWorldAsync(team);
        var paused = new EraIntegrationSqlInterceptor { PauseFirstUpdate = true };
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var equip = SendEquipAsync(Equip(_inventory[0]), paused, timeout.Token);
        try
        {
            // 暂停在首条装备写入前，确保真实限制查询已完成且其事务仍持有世界共享锁。
            await paused.UpdateReached.Task.WaitAsync(timeout.Token);
            await ReadDatabaseAsync(async context =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.ReadCommitted, timeout.Token);
                // 独立连接直接抢占成员排他锁；若限制 JOIN 也锁了成员，NOWAIT 会立即使本用例失败。
                var members = await context.Set<EraWorldMember>().FromSqlInterpolated(
                        $"SELECT * FROM era_world_member WHERE world_id = {world.WorldInstanceId.ToString()} AND player_id = {_playerId.ToString()} FOR UPDATE NOWAIT")
                    .AsNoTracking().ToListAsync(timeout.Token);
                members.Count.ShouldBe(1);
                members[0].IsContinuing.ShouldBeTrue();
                members[0].ActiveCharacterId.ShouldBe(_characterId);
                await transaction.RollbackAsync(timeout.Token);
            });
            paused.ResumeUpdate.TrySetResult();
            (await equip).Code.ShouldBe(HttpStatusCode.OK);
        }
        finally
        {
            paused.ResumeUpdate.TrySetResult();
            await timeout.CancelAsync();
            try
            {
                await equip;
            }
            catch
            {
                // 请求收束后才清理数据库，保留锁范围断言的原始异常。
            }
        }
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraWorldMember>().SingleAsync()).IsContinuing.ShouldBeTrue();
            (await context.Set<EraWorld>().SingleAsync()).State.ShouldBe(EraWorldState.Building);
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion.ShouldBe(4);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync()).ShouldBe(1);
        });
    }

    [Fact]
    [Trait("Category", "ERAWorldLifetimeIntegration")]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Timeout_and_waiting_equipment_write_should_release_restriction_and_preserve_owned_assets()
    {
        _worldTestClock = new EraWorldTestClock(DateTimeOffset.UtcNow);
        await SeedAsync(equipped: true);
        await using var team = await CreateWorldTeamAsync();
        var world = (await RestrictAsync(await RegisterWorldAsync(team), team.Team.Members[0].MemberId)).Data!;
        await RemoveTestTeamAsync(team);
        _worldTestClock.Advance(TimeSpan.FromSeconds(60));
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Missing);
        _worldTestClock.Advance(TimeSpan.FromSeconds(15));
        var paused = new EraIntegrationSqlInterceptor { PauseWorldSave = true };
        var observer = new EraIntegrationSqlInterceptor();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var check = CheckTimeoutAsync(world, paused, timeout.Token);
        Task<EquipEraEquipmentResponse>? equip = null;
        try
        {
            await paused.WorldSaveReached.Task.WaitAsync(timeout.Token);
            equip = SendEquipAsync(Equip(_inventory[0]), observer, timeout.Token);
            await observer.WorldRestrictionReadReached.Task.WaitAsync(timeout.Token);
            equip.IsCompleted.ShouldBeFalse();
            paused.ResumeWorldSave.TrySetResult();
            (await check).Status.ShouldBe(EraWorldTimeoutStatus.Ended);
            var equipped = await equip;
            equipped.Code.ShouldBe(HttpStatusCode.OK);
            equipped.Data!.State.EquippedOperationRestriction.ShouldBeNull();
        }
        finally
        {
            paused.ResumeWorldSave.TrySetResult();
            await timeout.CancelAsync();
            try { if (equip == null) await check; else await Task.WhenAll(check, equip); }
            catch { /* 不让清理覆盖主异常。 */ }
        }
        await ReadDatabaseAsync(async context =>
        {
            var items = await context.Set<EraEquipmentInstance>().Where(row => row.CharacterId == _characterId).ToListAsync();
            items.Count.ShouldBe(2);
            items.Single(row => row.Id == _inventory[0].Id).Location.ShouldBe(EraEquipmentLocation.Equipped);
            items.Single(row => row.Id == _equipped!.Id).Location.ShouldBe(EraEquipmentLocation.Inventory);
            items.ShouldAllBe(item => item.Version == 6 && item.OwnerPlayerId == _playerId);
            (await context.Set<EraWorld>().SingleAsync()).EndReason.ShouldBe(EraWorldEndReason.InactivityTimeout);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync()).ShouldBe(1);
        });
    }
}
