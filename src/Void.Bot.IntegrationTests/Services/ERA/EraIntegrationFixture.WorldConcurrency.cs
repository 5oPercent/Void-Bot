using System.Net;
using System.Reflection;
using System.Text.Json;
using Autofac;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Shouldly;
using StackExchange.Redis;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Core.Settings.System;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Constants.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    [Fact]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Production_redis_script_should_reject_delayed_authority_change_after_world_fence()
    {
        await SeedAsync(0);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var setting = _root.Resolve<CacheSetting>();
        using var redis = await ConnectionMultiplexer.ConnectAsync(setting.RedisConnection);
        var database = redis.GetDatabase(setting.RedisDatabaseIndex);
        var fenceKey = "era:world:{era-teams}:authority-fence:v1:" + team.Team.TeamId.ToString("D");
        var oldFence = await database.StringGetAsync(fenceKey);
        var pending = (await team.Provider.GetTeamAsync(team.Team.TeamId, CancellationToken.None))!;
        var oldStorageVersion = pending.StorageVersion;
        EraTeamLifecycle.SelectNext(pending, DateTimeOffset.UtcNow, 30).ShouldBeTrue();
        pending.StorageVersion = Guid.NewGuid();
        var keys = new List<RedisKey>
        {
            EraNetworkingConstants.RedisTeamIndex,
            EraNetworkingConstants.RedisTeamPrefix + pending.TeamId.ToString("D"),
            EraNetworkingConstants.RedisTeamCodePrefix + pending.TeamCode
        };
        keys.AddRange(pending.Members.Select(member => (RedisKey)(EraNetworkingConstants.RedisPlayerPrefix + member.PlayerId.ToString("D"))));
        keys.Add(fenceKey);
        var values = new RedisValue[]
        {
            oldStorageVersion.ToString("D"), JsonSerializer.Serialize(pending), pending.TeamId.ToString("D"),
            pending.ExpiresAt.ToUnixTimeMilliseconds(),
            JsonSerializer.Serialize(pending.Members.Select((member, index) => new { KeyIndex = index + 4, ExpiresAt = member.ExpiresAt.ToUnixTimeMilliseconds() })),
            string.Empty, DateTimeOffset.UtcNow.ToString("O"), Guid.NewGuid().ToString("D"), oldFence
        };
        // 重放实际生产 Lua 模拟网络晚到，不复制其栅栏或 CAS 逻辑。
        var script = (string)typeof(EraTeamDataProvider).GetField("CommitScript", BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
        var world = await RegisterWorldAsync(team); // 推进栅栏，但不改变 Redis 队伍版本。
        ((int)await database.ScriptEvaluateAsync(script, keys.ToArray(), values)).ShouldBe((int)EraTeamWriteStatus.Conflict);
        var unchanged = (await team.Provider.GetTeamAsync(team.Team.TeamId, CancellationToken.None))!;
        unchanged.StorageVersion.ShouldBe(oldStorageVersion);
        unchanged.HostPlayerId.ShouldBe(_playerId);
        unchanged.AuthorityEpoch.ShouldBe(1);
        // 同一完整提交换成新令牌可成功，排除其他无效参数导致的假阳性。
        values[8] = await database.StringIncrementAsync(fenceKey);
        ((int)await database.ScriptEvaluateAsync(script, keys.ToArray(), values)).ShouldBe((int)EraTeamWriteStatus.Success);
        (await Should.ThrowAsync<EraWorldAccessException>(() => RestrictAsync(world, team.Team.Members[0].MemberId)))
            .ErrorCode.ShouldBe("era.world_authority_changed");
        await ReadDatabaseAsync(async context =>
        {
            var saved = await context.Set<EraWorld>().SingleAsync();
            saved.HostPlayerId.ShouldBe(_playerId);
            saved.Version.ShouldBe(1);
            saved.IsEquippedOperationsRestricted.ShouldBeFalse();
        });
    }

    [Fact]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Database_should_reject_a_second_active_world_membership_for_the_same_player()
    {
        await SeedAsync(0);
        await using var team = await CreateWorldTeamAsync();
        var world = await RegisterWorldAsync(team);
        var error = await Should.ThrowAsync<DbUpdateException>(() => ReadDatabaseAsync(async context =>
        {
            var duplicate = new EraWorld
            {
                Id = Guid.NewGuid(), TeamId = world.TeamId, CreationRequestId = Guid.NewGuid(),
                CreatedByPlayerId = _playerId, CreationMemberId = team.Team.Members[0].MemberId,
                CreationAuthorityEpoch = 1, WorldDefinitionId = "duplicate-test", State = EraWorldState.Building,
                HostPlayerId = _playerId, AuthorityEpoch = 1, Version = 1
            };
            context.Set<EraWorld>().Add(duplicate);
            context.Set<EraWorldMember>().Add(new EraWorldMember
            {
                WorldId = duplicate.Id, PlayerId = _playerId, CharacterId = _characterId,
                MemberId = team.Team.Members[0].MemberId, IsContinuing = true,
                ActivePlayerId = _playerId, ActiveCharacterId = _characterId
            });
            await context.SaveChangesAsync();
        }));
        error.InnerException.ShouldBeOfType<MySqlException>().Number.ShouldBe(1062);
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraWorld>().CountAsync()).ShouldBe(1);
            (await context.Set<EraWorldMember>().CountAsync()).ShouldBe(1);
            (await context.Set<EraWorldTeamState>().SingleAsync(row => row.TeamId == world.TeamId)).ActiveWorldId.ShouldBe(world.WorldInstanceId);
        });
    }

    [Fact]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Late_team_member_should_join_existing_world_once_without_creating_assets()
    {
        await SeedAsync(0);
        await using var team = await CreateWorldTeamAsync();
        var world = await RegisterWorldAsync(team);
        var newMember = new EraTeamMember
        {
            MemberId = Guid.NewGuid(), PlayerId = _otherPlayerId, CharacterId = _otherCharacterId,
            JoinedAt = DateTimeOffset.UtcNow, ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10), CanHost = true
        };
        await team.Provider.ChangeAsync(team.Team.TeamId, current =>
        {
            current!.Members.Add(newMember);
            return new EraTeamChange<bool> { Changed = true, Team = current, Result = true };
        }, CancellationToken.None);
        var command = new JoinEraWorldCommand
        {
            WorldInstanceId = world.WorldInstanceId, MemberId = newMember.MemberId, AuthorityEpoch = 1, ExpectedWorldVersion = 1
        };
        var joined = await RequestAsync((mediator, token) => mediator.SendAsync<JoinEraWorldCommand, JoinEraWorldResponse>(command, token), accountId: _otherAccountId);
        joined.Data!.WorldInstanceId.ShouldBe(world.WorldInstanceId);
        joined.Data.Version.ShouldBe(2);
        joined.Data.Members.Count.ShouldBe(2);
        (await RequestAsync((mediator, token) => mediator.SendAsync<JoinEraWorldCommand, JoinEraWorldResponse>(command, token), accountId: _otherAccountId)).Data!.Version.ShouldBe(2);
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraWorld>().CountAsync()).ShouldBe(1);
            (await context.Set<EraWorldMember>().CountAsync()).ShouldBe(2);
            (await context.Set<EraEquipmentInstance>().CountAsync()).ShouldBe(1);
        });
    }

    [Fact]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Stale_restriction_version_should_not_change_world_or_equipment()
    {
        await SeedAsync();
        await using var team = await CreateWorldTeamAsync();
        var world = await RegisterWorldAsync(team);
        world.Version = 99;
        (await Should.ThrowAsync<EraWorldAccessException>(() => RestrictAsync(world, team.Team.Members[0].MemberId)))
            .ErrorCode.ShouldBe("era.world_version_conflict");
        await ReadDatabaseAsync(async context =>
        {
            var saved = await context.Set<EraWorld>().SingleAsync();
            saved.Version.ShouldBe(1);
            saved.RestrictionVersion.ShouldBe(0);
            saved.IsEquippedOperationsRestricted.ShouldBeFalse();
        });
        await AssertOriginalStateAsync();
    }

    [Fact]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Restriction_from_another_player_should_wait_for_equipment_transaction_then_block_unequip()
    {
        await SeedAsync();
        await using var team = await CreateWorldTeamAsync(includeOther: true, otherHost: true);
        var world = await RegisterWorldAsync(team);
        var paused = new EraIntegrationSqlInterceptor { PauseFirstUpdate = true };
        var observer = new EraIntegrationSqlInterceptor();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var equip = SendEquipAsync(Equip(_inventory[0]), paused, timeout.Token);
        Task<SetEraWorldEquipmentRestrictionResponse>? restriction = null;
        try
        {
            await paused.UpdateReached.Task.WaitAsync(timeout.Token);
            restriction = RestrictAsync(world, team.Team.Members[1].MemberId, accountId: _otherAccountId, interceptor: observer, cancellationToken: timeout.Token);
            await observer.WorldLockReached.Task.WaitAsync(timeout.Token);
            restriction.IsCompleted.ShouldBeFalse();
            paused.ResumeUpdate.TrySetResult();
            (await equip).Code.ShouldBe(HttpStatusCode.OK);
            (await restriction).Data!.IsEquippedOperationsRestricted.ShouldBeTrue();
        }
        finally
        {
            paused.ResumeUpdate.TrySetResult();
            await timeout.CancelAsync();
            try { if (restriction == null) await equip; else await Task.WhenAll(equip, restriction); }
            catch { /* 保留原断言异常，全部请求收束后才清理数据库。 */ }
        }
        (await SendUnequipAsync(_inventory[0].Id, 4, 6)).ErrorCode.ShouldBe("era.equipment_operations_restricted");
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion.ShouldBe(4);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync()).ShouldBe(1);
            (await context.Set<EraWorld>().SingleAsync()).RestrictionVersion.ShouldBe(1);
        });
    }

    [Fact]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Real_redis_migration_should_wait_for_an_authorized_world_write_to_commit()
    {
        await SeedAsync(0);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = await RegisterWorldAsync(team);
        var paused = new EraIntegrationSqlInterceptor { PauseWorldSave = true };
        var intent = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var restriction = RestrictAsync(world, team.Team.Members[0].MemberId, interceptor: paused, cancellationToken: timeout.Token);
        Task<EraTeam>? migration = null;
        try
        {
            await paused.WorldSaveReached.Task.WaitAsync(timeout.Token);
            migration = team.Provider.ChangeAsync(team.Team.TeamId, current =>
            {
                EraTeamLifecycle.SelectNext(current!, DateTimeOffset.UtcNow, 30).ShouldBeTrue();
                intent.TrySetResult();
                return new EraTeamChange<EraTeam> { Changed = true, Team = current, Result = current! };
            }, timeout.Token);
            await intent.Task.WaitAsync(timeout.Token);
            migration.IsCompleted.ShouldBeFalse();
            (await team.Provider.GetTeamAsync(team.Team.TeamId, timeout.Token))!.AuthorityEpoch.ShouldBe(1);
            paused.ResumeWorldSave.TrySetResult();
            (await restriction).Data!.RestrictionVersion.ShouldBe(1);
            (await migration).AuthorityEpoch.ShouldBe(2);
        }
        finally
        {
            paused.ResumeWorldSave.TrySetResult();
            await timeout.CancelAsync();
            try { if (migration == null) await restriction; else await Task.WhenAll(restriction, migration); }
            catch { /* 防止清理覆盖原始异常。 */ }
        }
        (await Should.ThrowAsync<EraWorldAccessException>(() => RestrictAsync(world, team.Team.Members[0].MemberId, false)))
            .ErrorCode.ShouldBe("era.world_authority_changed");
        await ReadDatabaseAsync(async context =>
        {
            var saved = await context.Set<EraWorld>().SingleAsync();
            saved.Version.ShouldBe(2);
            saved.IsEquippedOperationsRestricted.ShouldBeTrue();
        });
    }
}
