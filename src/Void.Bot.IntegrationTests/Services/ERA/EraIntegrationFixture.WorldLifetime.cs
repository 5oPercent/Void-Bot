using System.Net;
using Autofac;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using NSubstitute;
using Shouldly;
using StackExchange.Redis;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Services.ERA.Team;
using Void.Bot.Core.Services.ERA.World;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    [Fact]
    [Trait("Category", "ERAWorldLifetimeIntegration")]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Reconnected_team_with_migration_candidate_should_cancel_a_pending_timeout()
    {
        _worldTestClock = new EraWorldTestClock(DateTimeOffset.UtcNow);
        await SeedAsync(0);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = (await RestrictAsync(await RegisterWorldAsync(team), team.Team.Members[0].MemberId)).Data!;
        await RemoveTestTeamAsync(team);
        _worldTestClock.Advance(TimeSpan.FromSeconds(60));
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Missing);
        EraTeamLifecycle.SelectNext(team.Team, DateTimeOffset.UtcNow, 30).ShouldBeTrue();
        (await team.Provider.TryCreateAsync(team.Team, CancellationToken.None)).ShouldBe(EraTeamWriteStatus.Success);
        _worldTestClock.Advance(TimeSpan.FromSeconds(15));
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Alive);
        await ReadDatabaseAsync(async context =>
        {
            var saved = await context.Set<EraWorld>().SingleAsync();
            saved.Id.ShouldBe(world.WorldInstanceId);
            saved.State.ShouldBe(EraWorldState.Building);
            saved.TimeoutObservedAt.ShouldBeNull();
            saved.Version.ShouldBe(2);
            saved.RestrictionVersion.ShouldBe(1);
            saved.IsEquippedOperationsRestricted.ShouldBeTrue();
            // 后台存活检查不替代新房主的显式恢复与授权。
            saved.HostPlayerId.ShouldBe(_playerId);
            saved.AuthorityEpoch.ShouldBe(1);
        });
    }

    [Fact]
    [Trait("Category", "ERAWorldLifetimeIntegration")]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Failed_timeout_save_should_roll_back_world_and_members_and_allow_a_safe_retry()
    {
        _worldTestClock = new EraWorldTestClock(DateTimeOffset.UtcNow);
        await SeedAsync();
        await using var team = await CreateWorldTeamAsync();
        var world = await RegisterWorldAsync(team);
        await RemoveTestTeamAsync(team);
        _worldTestClock.Advance(TimeSpan.FromSeconds(60));
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Missing);
        _worldTestClock.Advance(TimeSpan.FromSeconds(15));
        var failure = new EraIntegrationSqlInterceptor { FailWorldSave = true };
        var error = await Should.ThrowAsync<DbUpdateException>(() => CheckTimeoutAsync(world, failure));
        error.InnerException.ShouldBeOfType<MySqlException>().Number.ShouldBe(1644);
        failure.WorldFailureInjected.ShouldBeTrue();
        await ReadDatabaseAsync(async context =>
        {
            var saved = await context.Set<EraWorld>().SingleAsync();
            saved.State.ShouldBe(EraWorldState.Building);
            saved.Version.ShouldBe(1);
            saved.EndReason.ShouldBeNull();
            saved.EndedAt.ShouldBeNull();
            saved.TimeoutObservedAt.ShouldNotBeNull();
            var member = await context.Set<EraWorldMember>().SingleAsync();
            member.IsContinuing.ShouldBeTrue();
            member.ActiveCharacterId.ShouldBe(_characterId);
            (await context.Set<EraWorldTeamState>().SingleAsync()).ActiveWorldId.ShouldBe(world.WorldInstanceId);
        });
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Ended);
        await AssertOriginalStateAsync();
    }

    [Fact]
    [Trait("Category", "ERAWorldLifetimeIntegration")]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Bounded_scan_should_prioritize_unchecked_worlds_instead_of_repeating_the_first_candidate()
    {
        _worldTestClock = new EraWorldTestClock(DateTimeOffset.UtcNow);
        await SeedAsync(0);
        await using var team = await CreateWorldTeamAsync();
        var first = await RegisterWorldAsync(team);
        var otherWorldId = Guid.NewGuid();
        var otherTeamId = Guid.NewGuid();
        var otherMemberId = Guid.NewGuid();
        await ReadDatabaseAsync(async context =>
        {
            context.Set<EraWorld>().Add(new EraWorld
            {
                Id = otherWorldId, TeamId = otherTeamId, CreationRequestId = Guid.NewGuid(),
                CreatedByPlayerId = _otherPlayerId, CreationMemberId = otherMemberId, CreationAuthorityEpoch = 1,
                WorldDefinitionId = "other-lifetime-test", HostPlayerId = _otherPlayerId, AuthorityEpoch = 1,
                State = EraWorldState.Ready, Version = 1, LastActiveAt = _worldTestClock.GetUtcNow()
            });
            context.Set<EraWorldTeamState>().Add(new EraWorldTeamState { TeamId = otherTeamId, ActiveWorldId = otherWorldId });
            context.Set<EraWorldMember>().Add(new EraWorldMember
            {
                WorldId = otherWorldId, PlayerId = _otherPlayerId, CharacterId = _otherCharacterId,
                MemberId = otherMemberId, IsContinuing = true, ActivePlayerId = _otherPlayerId, ActiveCharacterId = _otherCharacterId
            });
            await context.SaveChangesAsync();
        });
        await RemoveTestTeamAsync(team);
        _worldTestClock.Advance(TimeSpan.FromSeconds(60));
        (await CheckTimeoutAsync(first)).Status.ShouldBe(EraWorldTimeoutStatus.Missing);
        _worldTestClock.Advance(TimeSpan.FromSeconds(15));
        await using var scope = Scope();
        var candidates = await scope.Resolve<IEraWorldDataProvider>().GetTimeoutCandidatesAsync(
            _worldTestClock.GetUtcNow().AddSeconds(-60), _worldTestClock.GetUtcNow().AddSeconds(-10), 1, CancellationToken.None);
        candidates.ShouldBe(new[] { otherWorldId });
    }

    private Task<CheckEraWorldTimeoutResponse> CheckTimeoutAsync(EraWorldDto world,
        EraIntegrationSqlInterceptor? interceptor = null, CancellationToken cancellationToken = default)
    {
        return RequestAsync((mediator, token) => mediator.SendAsync<CheckEraWorldTimeoutCommand, CheckEraWorldTimeoutResponse>(
            new CheckEraWorldTimeoutCommand { WorldInstanceId = world.WorldInstanceId }, token), interceptor, cancellationToken);
    }

    private Task<HeartbeatEraWorldResponse> HeartbeatWorldAsync(EraWorldDto world, Guid memberId,
        Guid? accountId = null, EraIntegrationSqlInterceptor? interceptor = null, CancellationToken cancellationToken = default)
    {
        return RequestAsync((mediator, token) => mediator.SendAsync<HeartbeatEraWorldCommand, HeartbeatEraWorldResponse>(
            new HeartbeatEraWorldCommand { WorldInstanceId = world.WorldInstanceId, MemberId = memberId }, token),
            interceptor, cancellationToken, accountId);
    }

    private static Task RemoveTestTeamAsync(EraWorldIntegrationTeam team)
    {
        return team.Provider.ChangeAsync(team.Team.TeamId,
            current => new EraTeamChange<bool> { Changed = current != null, Team = null, Result = true }, CancellationToken.None);
    }

    [Fact]
    [Trait("Category", "ERAWorldLifetimeIntegration")]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Timeout_should_wait_for_sixty_seconds_and_two_separate_missing_observations()
    {
        _worldTestClock = new EraWorldTestClock(DateTimeOffset.UtcNow);
        await SeedAsync(equipped: true);
        await using var team = await CreateWorldTeamAsync();
        var world = await RegisterWorldAsync(team);
        world.InactivityTimeoutSeconds.ShouldBe(60);
        await RemoveTestTeamAsync(team);
        _worldTestClock.Advance(TimeSpan.FromSeconds(59));
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Deferred);
        _worldTestClock.Advance(TimeSpan.FromSeconds(1));
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Missing);
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Deferred);
        var query = await RequestAsync((mediator, token) => mediator.RequestAsync<GetCurrentEraWorldRequest, GetCurrentEraWorldResponse>(
            new GetCurrentEraWorldRequest(), token));
        query.Data!.LastActiveAt.ShouldBe(world.LastActiveAt); // GET 不续期或重置缺失确认。
        _worldTestClock.Advance(TimeSpan.FromSeconds(14));
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Missing);
        _worldTestClock.Advance(TimeSpan.FromSeconds(10));
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Ended);
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Ended);
        await ReadDatabaseAsync(async context =>
        {
            var saved = await context.Set<EraWorld>().SingleAsync(row => row.Id == world.WorldInstanceId);
            saved.Version.ShouldBe(2);
            saved.EndReason.ShouldBe(EraWorldEndReason.InactivityTimeout);
            saved.EndedAt.ShouldBe(_worldTestClock.GetUtcNow());
            (await context.Set<EraWorldTeamState>().SingleAsync(row => row.TeamId == world.TeamId)).ActiveWorldId.ShouldBeNull();
            (await context.Set<EraWorldMember>().SingleAsync()).ActivePlayerId.ShouldBeNull();
        });
        await AssertOriginalStateAsync(); // 结束不删除或改写已拥有的背包／已穿装备。
    }

    [Fact]
    [Trait("Category", "ERAWorldLifetimeIntegration")]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Heartbeat_after_missing_observation_should_cancel_timeout_without_changing_business_version()
    {
        _worldTestClock = new EraWorldTestClock(DateTimeOffset.UtcNow);
        await SeedAsync(0);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = await RegisterWorldAsync(team);
        await RemoveTestTeamAsync(team);
        _worldTestClock.Advance(TimeSpan.FromSeconds(60));
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Missing);
        _worldTestClock.Advance(TimeSpan.FromSeconds(15));
        var heartbeat = await HeartbeatWorldAsync(world, team.Team.Members[1].MemberId, _otherAccountId);
        heartbeat.Code.ShouldBe(HttpStatusCode.OK);
        heartbeat.Data!.LastActiveAt.ShouldBe(_worldTestClock.GetUtcNow());
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Deferred);
        await ReadDatabaseAsync(async context =>
        {
            var saved = await context.Set<EraWorld>().SingleAsync();
            saved.Version.ShouldBe(1);
            saved.TimeoutObservedAt.ShouldBeNull();
            saved.State.ShouldBe(EraWorldState.Building);
        });
    }

    [Fact]
    [Trait("Category", "ERAWorldLifetimeIntegration")]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Live_team_member_or_migration_candidate_should_keep_original_world_and_restriction()
    {
        _worldTestClock = new EraWorldTestClock(DateTimeOffset.UtcNow);
        await SeedAsync(0);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = (await RestrictAsync(await RegisterWorldAsync(team), team.Team.Members[0].MemberId)).Data!;
        _worldTestClock.Advance(TimeSpan.FromSeconds(60));
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Alive);
        await team.Provider.ChangeAsync(team.Team.TeamId, current =>
        {
            EraTeamLifecycle.SelectNext(current!, DateTimeOffset.UtcNow, 30).ShouldBeTrue();
            return new EraTeamChange<bool> { Changed = true, Team = current, Result = true };
        }, CancellationToken.None);
        _worldTestClock.Advance(TimeSpan.FromSeconds(60));
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Alive);
        await ReadDatabaseAsync(async context =>
        {
            var saved = await context.Set<EraWorld>().SingleAsync();
            saved.Id.ShouldBe(world.WorldInstanceId);
            saved.Version.ShouldBe(2);
            saved.IsEquippedOperationsRestricted.ShouldBeTrue();
            saved.RestrictionVersion.ShouldBe(1);
        });
    }

    [Fact]
    [Trait("Category", "ERAWorldLifetimeIntegration")]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Redis_failure_should_break_confirmation_and_require_two_successful_checks_after_recovery()
    {
        _worldTestClock = new EraWorldTestClock(DateTimeOffset.UtcNow);
        await SeedAsync(0);
        await using var team = await CreateWorldTeamAsync();
        var world = await RegisterWorldAsync(team);
        await RemoveTestTeamAsync(team);
        _worldTestClock.Advance(TimeSpan.FromSeconds(60));
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Missing);
        // 仅本例故障阶段注入 Redis 传输异常；SQL、Mediator 和结束事务仍为真实实现。
        _worldTestTeamProvider = Substitute.For<IEraTeamDataProvider>();
        _worldTestTeamProvider.FenceAuthorityAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _worldTestTeamProvider.GetTeamAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<EraTeam?>(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "world lifetime test")));
        _worldTestClock.Advance(TimeSpan.FromSeconds(15));
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Unavailable);
        await ReadDatabaseAsync(async context =>
        {
            var saved = await context.Set<EraWorld>().SingleAsync();
            saved.State.ShouldBe(EraWorldState.Building);
            saved.TimeoutObservedAt.ShouldBeNull();
            saved.Version.ShouldBe(1);
        });
        _worldTestTeamProvider = null;
        _worldTestClock.Advance(TimeSpan.FromSeconds(15));
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Missing);
        _worldTestClock.Advance(TimeSpan.FromSeconds(15));
        (await CheckTimeoutAsync(world)).Status.ShouldBe(EraWorldTimeoutStatus.Ended);
    }

    [Fact]
    [Trait("Category", "ERAWorldLifetimeIntegration")]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Foreign_or_left_member_should_not_renew_world_and_ended_world_cannot_be_revived()
    {
        _worldTestClock = new EraWorldTestClock(DateTimeOffset.UtcNow);
        await SeedAsync(0);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = await RegisterWorldAsync(team);
        _worldTestClock.Advance(TimeSpan.FromSeconds(20));
        (await Should.ThrowAsync<EraWorldAccessException>(() => HeartbeatWorldAsync(world, team.Team.Members[0].MemberId, _otherAccountId)))
            .ErrorCode.ShouldBe("era.world_not_found");
        var left = await RequestAsync((mediator, token) => mediator.SendAsync<LeaveEraWorldCommand, LeaveEraWorldResponse>(
            new LeaveEraWorldCommand { WorldInstanceId = world.WorldInstanceId, MemberId = team.Team.Members[1].MemberId,
                AuthorityEpoch = 1, ExpectedWorldVersion = 1 }, token), accountId: _otherAccountId);
        (await Should.ThrowAsync<EraWorldAccessException>(() => HeartbeatWorldAsync(world, team.Team.Members[1].MemberId, _otherAccountId)))
            .ErrorCode.ShouldBe("era.world_not_found");
        var ended = await RequestAsync((mediator, token) => mediator.SendAsync<EndEraWorldCommand, EndEraWorldResponse>(
            new EndEraWorldCommand { WorldInstanceId = world.WorldInstanceId, MemberId = team.Team.Members[0].MemberId,
                AuthorityEpoch = 1, ExpectedWorldVersion = left.Data!.Version }, token));
        ended.Data!.EndReason.ShouldBe(EraWorldEndReason.Abandoned);
        (await Should.ThrowAsync<EraWorldAccessException>(() => HeartbeatWorldAsync(world, team.Team.Members[0].MemberId)))
            .ErrorCode.ShouldBe("era.world_ended");
        await ReadDatabaseAsync(async context => (await context.Set<EraWorld>().SingleAsync()).Version.ShouldBe(3));
    }

    [Fact]
    [Trait("Category", "ERAWorldLifetimeIntegration")]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Old_timeout_retry_should_preserve_a_new_world_and_original_end_reason()
    {
        _worldTestClock = new EraWorldTestClock(DateTimeOffset.UtcNow);
        await SeedAsync(0);
        await using var team = await CreateWorldTeamAsync();
        var first = await RegisterWorldAsync(team);
        await RequestAsync((mediator, token) => mediator.SendAsync<EndEraWorldCommand, EndEraWorldResponse>(
            new EndEraWorldCommand { WorldInstanceId = first.WorldInstanceId, MemberId = team.Team.Members[0].MemberId,
                AuthorityEpoch = 1, ExpectedWorldVersion = 1, ConstructionFailed = true }, token));
        var next = await RegisterWorldAsync(team);
        _worldTestClock.Advance(TimeSpan.FromSeconds(120));
        (await CheckTimeoutAsync(first)).Status.ShouldBe(EraWorldTimeoutStatus.Ended);
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraWorldTeamState>().SingleAsync(row => row.TeamId == next.TeamId)).ActiveWorldId.ShouldBe(next.WorldInstanceId);
            (await context.Set<EraWorld>().SingleAsync(row => row.Id == first.WorldInstanceId)).EndReason.ShouldBe(EraWorldEndReason.ConstructionFailed);
            (await context.Set<EraWorld>().SingleAsync(row => row.Id == next.WorldInstanceId)).State.ShouldBe(EraWorldState.Building);
        });
    }
}
