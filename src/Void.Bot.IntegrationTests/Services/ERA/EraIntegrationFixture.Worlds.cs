using System.Net;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Services.ERA.Team;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    private async Task<EraWorldIntegrationTeam> CreateWorldTeamAsync(bool includeOther = false, bool otherHost = false)
    {
        _enableWorldNetworking = true;
        var expires = DateTimeOffset.UtcNow.AddMinutes(10);
        var team = new EraTeam
        {
            TeamId = Guid.NewGuid(), TeamCode = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
            HostPlayerId = otherHost ? _otherPlayerId : _playerId, AuthorityEpoch = 1,
            MaxMembers = 4, ExpiresAt = expires, CreationRequestId = Guid.NewGuid(),
            Members = new List<EraTeamMember>
            {
                new EraTeamMember { PlayerId = _playerId, CharacterId = _characterId, MemberId = Guid.NewGuid(),
                    CanHost = true, JoinedAt = DateTimeOffset.UtcNow, ExpiresAt = expires }
            }
        };
        if (includeOther) team.Members.Add(new EraTeamMember
        {
            PlayerId = _otherPlayerId, CharacterId = _otherCharacterId, MemberId = Guid.NewGuid(),
            CanHost = true, JoinedAt = DateTimeOffset.UtcNow.AddSeconds(1), ExpiresAt = expires
        });
        var provider = _root.Resolve<IEraTeamDataProvider>();
        (await provider.TryCreateAsync(team, CancellationToken.None)).ShouldBe(EraTeamWriteStatus.Success);
        return new EraWorldIntegrationTeam(team, provider);
    }

    private async Task<EraWorldDto> RegisterWorldAsync(EraWorldIntegrationTeam team, Guid? requestId = null, string definition = "test-world")
    {
        var host = team.Team.Members.Single(member => member.PlayerId == team.Team.HostPlayerId);
        var response = await RequestAsync((mediator, token) => mediator.SendAsync<RegisterEraWorldCommand, RegisterEraWorldResponse>(
            new RegisterEraWorldCommand
            {
                TeamId = team.Team.TeamId, MemberId = host.MemberId, RequestId = requestId ?? Guid.NewGuid(),
                AuthorityEpoch = team.Team.AuthorityEpoch, WorldDefinitionId = definition
            }, token), accountId: host.PlayerId == _playerId ? null : _otherAccountId);
        response.Code.ShouldBe(HttpStatusCode.OK);
        return response.Data!;
    }

    private Task<SetEraWorldEquipmentRestrictionResponse> RestrictAsync(EraWorldDto world, Guid memberId, bool restricted = true,
        Guid? accountId = null, EraIntegrationSqlInterceptor? interceptor = null, CancellationToken cancellationToken = default)
    {
        return RequestAsync((mediator, token) => mediator.SendAsync<SetEraWorldEquipmentRestrictionCommand, SetEraWorldEquipmentRestrictionResponse>(
            new SetEraWorldEquipmentRestrictionCommand
            {
                WorldInstanceId = world.WorldInstanceId, MemberId = memberId, AuthorityEpoch = world.AuthorityEpoch,
                ExpectedWorldVersion = world.Version, IsRestricted = restricted
            }, token), interceptor, cancellationToken, accountId);
    }

    [Fact]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task World_registration_should_be_idempotent_and_reject_another_active_world()
    {
        await SeedAsync(0);
        await using var team = await CreateWorldTeamAsync();
        var requestId = Guid.NewGuid();
        var world = await RegisterWorldAsync(team, requestId);
        var replay = await RegisterWorldAsync(team, requestId);
        replay.WorldInstanceId.ShouldBe(world.WorldInstanceId);
        replay.Version.ShouldBe(1);
        (await Should.ThrowAsync<EraWorldAccessException>(() => RegisterWorldAsync(team, requestId, "changed")))
            .ErrorCode.ShouldBe("era.world_creation_conflict");
        (await Should.ThrowAsync<EraWorldAccessException>(() => RegisterWorldAsync(team)))
            .ErrorCode.ShouldBe("era.world_already_active");
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraWorld>().CountAsync()).ShouldBe(1);
            (await context.Set<EraWorldMember>().SingleAsync()).ActiveCharacterId.ShouldBe(_characterId);
            (await context.Set<EraWorldTeamState>().SingleAsync(row => row.TeamId == team.Team.TeamId)).ActiveWorldId.ShouldBe(world.WorldInstanceId);
        });
        await AssertOriginalStateAsync();
    }

    [Fact]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Ready_and_read_queries_should_keep_world_identity_and_not_repeat_the_transition()
    {
        await SeedAsync(0);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = await RegisterWorldAsync(team);
        var command = new ReadyEraWorldCommand
        {
            WorldInstanceId = world.WorldInstanceId, MemberId = team.Team.Members[0].MemberId,
            AuthorityEpoch = 1, ExpectedWorldVersion = 1
        };
        var ready = await RequestAsync((mediator, token) => mediator.SendAsync<ReadyEraWorldCommand, ReadyEraWorldResponse>(command, token));
        ready.Data!.State.ShouldBe(EraWorldState.Ready);
        ready.Data.Version.ShouldBe(2);
        (await RequestAsync((mediator, token) => mediator.SendAsync<ReadyEraWorldCommand, ReadyEraWorldResponse>(command, token))).Data!.Version.ShouldBe(2);
        var current = await RequestAsync((mediator, token) => mediator.RequestAsync<GetCurrentEraWorldRequest, GetCurrentEraWorldResponse>(new GetCurrentEraWorldRequest(), token));
        current.Data!.WorldInstanceId.ShouldBe(world.WorldInstanceId);
        current.Data.Members.Count.ShouldBe(2);
        current.Data.Version.ShouldBe(2);
        var other = await RequestAsync((mediator, token) => mediator.RequestAsync<GetEraWorldRequest, GetEraWorldResponse>(
            new GetEraWorldRequest { WorldInstanceId = world.WorldInstanceId }, token), accountId: _otherAccountId);
        other.Data!.WorldInstanceId.ShouldBe(world.WorldInstanceId);
    }

    [Fact]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task World_permissions_should_reject_non_host_writes_and_non_member_reads()
    {
        await SeedAsync();
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = await RegisterWorldAsync(team);
        var error = await Should.ThrowAsync<EraWorldAccessException>(() => RestrictAsync(world, team.Team.Members[1].MemberId, accountId: _otherAccountId));
        error.Code.ShouldBe(HttpStatusCode.Forbidden);
        error.ErrorCode.ShouldBe("era.world_host_required");
        // 移除世界成员关系，账号／玩家仍有效，读取必须按世界归属拒绝。
        await ReadDatabaseAsync(async context =>
        {
            context.Set<EraWorldMember>().Remove(await context.Set<EraWorldMember>().SingleAsync(row => row.WorldId == world.WorldInstanceId && row.PlayerId == _otherPlayerId));
            await context.SaveChangesAsync();
        });
        (await Should.ThrowAsync<EraWorldAccessException>(() => RequestAsync((mediator, token) =>
            mediator.RequestAsync<GetEraWorldRequest, GetEraWorldResponse>(new GetEraWorldRequest { WorldInstanceId = world.WorldInstanceId }, token),
            accountId: _otherAccountId))).ErrorCode.ShouldBe("era.world_not_found");
        await AssertOriginalStateAsync();
    }

    [Fact]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Restriction_should_reject_swap_and_unequip_without_changing_equipment_or_receipts()
    {
        await SeedAsync(equipped: true);
        await using var team = await CreateWorldTeamAsync();
        var world = await RegisterWorldAsync(team);
        var restricted = await RestrictAsync(world, team.Team.Members[0].MemberId);
        restricted.Data!.RestrictionVersion.ShouldBe(1);
        (await SendEquipAsync(Equip(_inventory[0]))).ErrorCode.ShouldBe("era.equipment_operations_restricted");
        (await SendUnequipAsync(_equipped!.Id)).ErrorCode.ShouldBe("era.equipment_operations_restricted");
        var query = await RequestAsync((mediator, token) => mediator.RequestAsync<GetEraCharacterEquipmentRequest, GetEraCharacterEquipmentResponse>(
            new GetEraCharacterEquipmentRequest { CharacterId = _characterId }, token));
        query.Data!.EquippedOperationRestriction!.IsRestricted.ShouldBeTrue();
        query.Data.EquippedOperationRestriction.WorldInstanceId.ShouldBe(world.WorldInstanceId);
        query.Data.EquippedOperationRestriction.RestrictionVersion.ShouldBe(1);
        await AssertOriginalStateAsync();
    }

    [Fact]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Restriction_should_allow_empty_slot_equip_and_historical_replay()
    {
        await SeedAsync();
        await using var team = await CreateWorldTeamAsync();
        var world = await RegisterWorldAsync(team);
        await RestrictAsync(world, team.Team.Members[0].MemberId);
        var command = Equip(_inventory[0]);
        var response = await SendEquipAsync(command);
        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data!.State.EquippedOperationRestriction!.IsRestricted.ShouldBeTrue();
        (await SendEquipAsync(command)).IsReplay.ShouldBeTrue();
        (await SendUnequipAsync(command.EquipmentId, 4, 6)).ErrorCode.ShouldBe("era.equipment_operations_restricted");
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion.ShouldBe(4);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync()).ShouldBe(1);
        });
    }

    [Fact]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Real_team_migration_should_fence_old_host_and_recover_same_world_with_restriction()
    {
        await SeedAsync(0);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = (await RestrictAsync(await RegisterWorldAsync(team), team.Team.Members[0].MemberId)).Data!;
        var migrated = await team.Provider.ChangeAsync(team.Team.TeamId, current =>
        {
            EraTeamLifecycle.SelectNext(current!, DateTimeOffset.UtcNow, 30).ShouldBeTrue();
            return new EraTeamChange<EraTeam> { Changed = true, Team = current, Result = current! };
        }, CancellationToken.None);
        var newMember = migrated.Members.Single(member => member.PlayerId == _otherPlayerId);
        var completed = await RequestAsync((mediator, token) => mediator.SendAsync<CompleteEraHostMigrationCommand, CompleteEraHostMigrationResponse>(
            new CompleteEraHostMigrationCommand { TeamId = migrated.TeamId, MemberId = newMember.MemberId, AuthorityEpoch = 2, Success = true }, token),
            accountId: _otherAccountId);
        completed.Code.ShouldBe(HttpStatusCode.OK);
        (await Should.ThrowAsync<EraWorldAccessException>(() => RestrictAsync(world, team.Team.Members[0].MemberId, false)))
            .ErrorCode.ShouldBe("era.world_authority_changed");
        var recovered = await RequestAsync((mediator, token) => mediator.SendAsync<RecoverEraWorldCommand, RecoverEraWorldResponse>(
            new RecoverEraWorldCommand { WorldInstanceId = world.WorldInstanceId, MemberId = newMember.MemberId, AuthorityEpoch = 2, ExpectedWorldVersion = world.Version }, token),
            accountId: _otherAccountId);
        recovered.Data!.WorldInstanceId.ShouldBe(world.WorldInstanceId);
        recovered.Data.AuthorityEpoch.ShouldBe(2);
        recovered.Data.HostPlayerId.ShouldBe(_otherPlayerId);
        recovered.Data.IsEquippedOperationsRestricted.ShouldBeTrue();
        recovered.Data.RestrictionVersion.ShouldBe(1);
        (await Should.ThrowAsync<EraWorldAccessException>(() => RestrictAsync(recovered.Data, team.Team.Members[0].MemberId, false)))
            .ErrorCode.ShouldBe("era.world_host_required");
        var unlocked = await RestrictAsync(recovered.Data, newMember.MemberId, false, _otherAccountId);
        unlocked.Data!.RestrictionVersion.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Only_last_member_leaving_should_end_world_and_release_active_membership()
    {
        await SeedAsync(0);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = await RegisterWorldAsync(team);
        var first = await RequestAsync((mediator, token) => mediator.SendAsync<LeaveEraWorldCommand, LeaveEraWorldResponse>(
            new LeaveEraWorldCommand { WorldInstanceId = world.WorldInstanceId, MemberId = team.Team.Members[0].MemberId, AuthorityEpoch = 1, ExpectedWorldVersion = 1 }, token));
        first.Data!.State.ShouldBe(EraWorldState.Building);
        var command = new LeaveEraWorldCommand
        {
            WorldInstanceId = world.WorldInstanceId, MemberId = team.Team.Members[1].MemberId, AuthorityEpoch = 1, ExpectedWorldVersion = 2
        };
        var last = await RequestAsync((mediator, token) => mediator.SendAsync<LeaveEraWorldCommand, LeaveEraWorldResponse>(command, token), accountId: _otherAccountId);
        last.Data!.State.ShouldBe(EraWorldState.Ended);
        (await RequestAsync((mediator, token) => mediator.SendAsync<LeaveEraWorldCommand, LeaveEraWorldResponse>(command, token), accountId: _otherAccountId)).Data!.Version.ShouldBe(3);
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraWorldMember>().CountAsync(row => row.ActivePlayerId != null || row.ActiveCharacterId != null)).ShouldBe(0);
            (await context.Set<EraWorldTeamState>().SingleAsync(row => row.TeamId == world.TeamId)).ActiveWorldId.ShouldBeNull();
        });
    }

    [Fact]
    [Trait("Category", "ERAWorldIntegration")]
    public async Task Failed_build_and_repeated_old_end_should_not_affect_new_world()
    {
        await SeedAsync(0);
        await using var team = await CreateWorldTeamAsync();
        var world = await RegisterWorldAsync(team);
        var command = new EndEraWorldCommand
        {
            WorldInstanceId = world.WorldInstanceId, MemberId = team.Team.Members[0].MemberId,
            AuthorityEpoch = 1, ExpectedWorldVersion = 1, ConstructionFailed = true
        };
        (await RequestAsync((mediator, token) => mediator.SendAsync<EndEraWorldCommand, EndEraWorldResponse>(command, token))).Data!.State.ShouldBe(EraWorldState.BuildFailed);
        var next = await RegisterWorldAsync(team);
        next.WorldInstanceId.ShouldNotBe(world.WorldInstanceId);
        await RequestAsync((mediator, token) => mediator.SendAsync<EndEraWorldCommand, EndEraWorldResponse>(command, token));
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraWorldTeamState>().SingleAsync(row => row.TeamId == world.TeamId)).ActiveWorldId.ShouldBe(next.WorldInstanceId);
            (await context.Set<EraWorld>().SingleAsync(row => row.Id == next.WorldInstanceId)).State.ShouldBe(EraWorldState.Building);
        });
    }
}
