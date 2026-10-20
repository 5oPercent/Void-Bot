using System.Linq.Expressions;
using System.Net;
using StackExchange.Redis;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Resources.i18n.ERA;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Settings.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA;

public partial interface IEraWorldService : IScope
{
    Task<RegisterEraWorldResponse> RegisterAsync(RegisterEraWorldCommand command, CancellationToken cancellationToken);
    Task<T> ExecuteAsync<T>(IEraWorldOperation command, CancellationToken cancellationToken) where T : EraResponse<EraWorldDto>, new();
    Task<EraWorldDto?> GetAsync(Guid? worldId, CancellationToken cancellationToken);
}

/// <summary>世界写操作先锁队伍协调行，再锁世界；锁内读取 Redis 权限，事务由 Mediator 提交。</summary>
public sealed partial class EraWorldService : IEraWorldService
{
    private readonly IEraWorldDataProvider _worlds;
    private readonly IEraTeamDataProvider _teams;
    private readonly IEraPlayerContext _players;
    private readonly IEraDataProvider _characters;
    private readonly EraNetworkingSetting _setting;
    private readonly ILocalizationService _localization;
    private readonly TimeProvider _clock;
    private readonly EraWorldLifetimeSetting _lifetime;
    private readonly IEraEquipmentDropDataProvider _drops;

    public EraWorldService(IEraWorldDataProvider worlds, IEraTeamDataProvider teams, IEraPlayerContext players,
        EraNetworkingSetting setting, ILocalizationService localization, TimeProvider clock, IEraDataProvider characters,
        EraWorldLifetimeSetting lifetime, IEraEquipmentDropDataProvider drops)
    {
        _worlds = worlds;
        _teams = teams;
        _players = players;
        _characters = characters;
        _setting = setting;
        _localization = localization;
        _clock = clock;
        _lifetime = lifetime;
        _drops = drops;
    }

    private EraWorldAccessException Error(HttpStatusCode status, string code, Expression<Func<EraWorldContextTemplate, object>> text)
    {
        return new EraWorldAccessException(status, code, _localization.GetValue(text));
    }

    private async Task<(EraTeam Team, EraTeamMember Member)> RequireTeamAsync(Guid teamId, Guid playerId,
        Guid memberId, long epoch, bool hostRequired, CancellationToken cancellationToken)
    {
        if (!_setting.Enabled) throw Error(HttpStatusCode.ServiceUnavailable, "era.world_networking_unavailable", text => text.NetworkingUnavailable);
        EraTeam? team;
        try
        {
            // 上层持有 SQL 队伍锁；先失效旧提交令牌，再读取实时队伍权限。
            await _teams.FenceAuthorityAsync(teamId, cancellationToken).ConfigureAwait(false);
            team = await _teams.GetTeamAsync(teamId, cancellationToken).ConfigureAwait(false);
        }
        catch (RedisException) { throw Error(HttpStatusCode.ServiceUnavailable, "era.world_networking_unavailable", text => text.NetworkingUnavailable); }
        var now = _clock.GetUtcNow();
        var member = team?.Members.SingleOrDefault(item => item.PlayerId == playerId && item.MemberId == memberId && item.ExpiresAt > now);
        if (team == null || team.ExpiresAt <= now || member == null)
            throw Error(HttpStatusCode.Conflict, "era.world_team_unavailable", text => text.TeamUnavailable);
        var character = await _characters.GetCharacterByIdAsync(member.CharacterId, cancellationToken).ConfigureAwait(false);
        if (character == null || character.PlayerId != playerId || character.IsDisabled)
            throw Error(HttpStatusCode.Conflict, "era.world_member_unavailable", text => text.MemberUnavailable);
        if (team.IsMigrating || team.AuthorityEpoch != epoch || !team.Members.Any(item => item.PlayerId == team.HostPlayerId && item.ExpiresAt > now))
            throw Error(HttpStatusCode.Conflict, "era.world_authority_changed", text => text.AuthorityChanged);
        if (hostRequired && team.HostPlayerId != playerId)
            throw Error(HttpStatusCode.Forbidden, "era.world_host_required", text => text.HostRequired);
        return (team, member);
    }

    public async Task<RegisterEraWorldResponse> RegisterAsync(RegisterEraWorldCommand command, CancellationToken cancellationToken)
    {
        if (command.TeamId == Guid.Empty || command.MemberId == Guid.Empty || command.RequestId == Guid.Empty || command.AuthorityEpoch < 1 ||
            string.IsNullOrWhiteSpace(command.WorldDefinitionId) || command.WorldDefinitionId.Length > 128 || command.WorldDefinitionId != command.WorldDefinitionId.Trim())
            throw Error(HttpStatusCode.BadRequest, "era.world_invalid_operation", text => text.InvalidOperation);
        var player = await _players.RequireCurrentAsync(true, cancellationToken).ConfigureAwait(false);
        var teamState = await _worlds.LockTeamAsync(command.TeamId, cancellationToken).ConfigureAwait(false);
        var authority = await RequireTeamAsync(command.TeamId, player.Id, command.MemberId, command.AuthorityEpoch, true, cancellationToken).ConfigureAwait(false);
        var previous = await _worlds.GetCreationAsync(command.TeamId, command.RequestId, cancellationToken).ConfigureAwait(false);
        if (previous != null)
        {
            if (previous.CreatedByPlayerId != player.Id || previous.CreationMemberId != command.MemberId ||
                previous.CreationAuthorityEpoch != command.AuthorityEpoch || previous.WorldDefinitionId != command.WorldDefinitionId)
                throw Error(HttpStatusCode.Conflict, "era.world_creation_conflict", text => text.CreationConflict);
            return new RegisterEraWorldResponse { Code = HttpStatusCode.OK, Data = Map(previous, await _worlds.GetMembersAsync(previous.Id, false, cancellationToken).ConfigureAwait(false)) };
        }
        if (teamState.ActiveWorldId.HasValue)
            throw Error(HttpStatusCode.Conflict, "era.world_already_active", text => text.AlreadyActive);
        var createdAt = _clock.GetUtcNow();
        var members = authority.Team.Members.Where(member => member.ExpiresAt > createdAt).ToList();
        if (!await _worlds.CanRegisterMembersAsync(members, cancellationToken).ConfigureAwait(false))
            throw Error(HttpStatusCode.Conflict, "era.world_member_unavailable", text => text.MemberUnavailable);
        var world = new EraWorld
        {
            Id = Guid.NewGuid(), TeamId = command.TeamId, CreationRequestId = command.RequestId,
            CreatedByPlayerId = player.Id, CreationMemberId = command.MemberId, CreationAuthorityEpoch = command.AuthorityEpoch,
            WorldDefinitionId = command.WorldDefinitionId, State = EraWorldState.Building,
            HostPlayerId = player.Id, AuthorityEpoch = command.AuthorityEpoch, Version = 1,
            CreatedAt = createdAt, LastActiveAt = createdAt
        };
        var registered = members.Select(member => new EraWorldMember
        {
            WorldId = world.Id, PlayerId = member.PlayerId, CharacterId = member.CharacterId, MemberId = member.MemberId,
            IsContinuing = true, ActivePlayerId = member.PlayerId, ActiveCharacterId = member.CharacterId
        }).ToList();
        _worlds.AddWorld(world, registered);
        teamState.ActiveWorldId = world.Id;
        return new RegisterEraWorldResponse { Code = HttpStatusCode.OK, Data = Map(world, registered) };
    }

    public async Task<T> ExecuteAsync<T>(IEraWorldOperation command, CancellationToken cancellationToken) where T : EraResponse<EraWorldDto>, new()
    {
        if (command.WorldInstanceId == Guid.Empty || command.MemberId == Guid.Empty || command.AuthorityEpoch < 1 || command.ExpectedWorldVersion < 1)
            throw Error(HttpStatusCode.BadRequest, "era.world_invalid_operation", text => text.InvalidOperation);
        var player = await _players.RequireCurrentAsync(true, cancellationToken).ConfigureAwait(false);
        var initial = await _worlds.GetWorldAsync(command.WorldInstanceId, cancellationToken).ConfigureAwait(false);
        if (initial == null) throw Error(HttpStatusCode.NotFound, "era.world_not_found", text => text.NotFound);
        var teamState = await _worlds.LockTeamAsync(initial.TeamId, cancellationToken).ConfigureAwait(false);
        var world = await _worlds.LockWorldAsync(initial.Id, cancellationToken).ConfigureAwait(false);
        if (world == null) throw Error(HttpStatusCode.NotFound, "era.world_not_found", text => text.NotFound);
        var members = await _worlds.GetMembersAsync(world.Id, true, cancellationToken).ConfigureAwait(false);
        var member = members.SingleOrDefault(item => item.PlayerId == player.Id && item.MemberId == command.MemberId);
        if (command is not JoinEraWorldCommand && member == null)
            throw Error(HttpStatusCode.NotFound, "era.world_not_found", text => text.NotFound);
        var terminal = world.State is EraWorldState.Ended or EraWorldState.BuildFailed;
        if ((command is EndEraWorldCommand && terminal) || (command is LeaveEraWorldCommand && member is { IsContinuing: false }))
            return new T { Code = HttpStatusCode.OK, Data = Map(world, members) };
        if (terminal || world.State == EraWorldState.Ending || teamState.ActiveWorldId != world.Id)
            throw Error(HttpStatusCode.Conflict, "era.world_ended", text => text.Ended);

        // 本人离开不依赖 Redis 租约仍然存在，不能因断线而无法释放持久成员状态。
        if (command is not LeaveEraWorldCommand)
        {
            var hostRequired = command is not JoinEraWorldCommand;
            var authority = await RequireTeamAsync(world.TeamId, player.Id, command.MemberId, command.AuthorityEpoch, hostRequired, cancellationToken).ConfigureAwait(false);
            var activityAt = _clock.GetUtcNow();
            if (activityAt > world.LastActiveAt) world.LastActiveAt = activityAt;
            world.TimeoutObservedAt = null;
            if (command is JoinEraWorldCommand)
            {
                if (member != null)
                {
                    if (!member.IsContinuing || member.CharacterId != authority.Member.CharacterId)
                        throw Error(HttpStatusCode.Conflict, "era.world_member_unavailable", text => text.MemberUnavailable);
                    return new T { Code = HttpStatusCode.OK, Data = Map(world, members) };
                }
                if (members.Any(item => item.PlayerId == player.Id) || !await _worlds.CanRegisterMembersAsync(new[] { authority.Member }, cancellationToken).ConfigureAwait(false))
                    throw Error(HttpStatusCode.Conflict, "era.world_member_unavailable", text => text.MemberUnavailable);
                member = new EraWorldMember
                {
                    WorldId = world.Id, PlayerId = player.Id, CharacterId = authority.Member.CharacterId, MemberId = command.MemberId,
                    IsContinuing = true, ActivePlayerId = player.Id, ActiveCharacterId = authority.Member.CharacterId
                };
            }
            else if (member is not { IsContinuing: true } || member.CharacterId != authority.Member.CharacterId)
                throw Error(HttpStatusCode.Conflict, "era.world_member_unavailable", text => text.MemberUnavailable);

            if (command is RecoverEraWorldCommand)
            {
                if (world.AuthorityEpoch == command.AuthorityEpoch && world.HostPlayerId == player.Id)
                    return new T { Code = HttpStatusCode.OK, Data = Map(world, members) };
                if (world.AuthorityEpoch >= command.AuthorityEpoch)
                    throw Error(HttpStatusCode.Conflict, "era.world_authority_changed", text => text.AuthorityChanged);
            }
            else if (command is not JoinEraWorldCommand &&
                (world.AuthorityEpoch != command.AuthorityEpoch || world.HostPlayerId != authority.Team.HostPlayerId))
                throw Error(HttpStatusCode.Conflict, "era.world_authority_changed", text => text.AuthorityChanged);
        }

        if (command is ReadyEraWorldCommand && world.State == EraWorldState.Ready)
            return new T { Code = HttpStatusCode.OK, Data = Map(world, members) };
        if (world.Version != command.ExpectedWorldVersion)
            throw Error(HttpStatusCode.Conflict, "era.world_version_conflict", text => text.VersionConflict);
        switch (command)
        {
            case ReadyEraWorldCommand:
                if (world.State != EraWorldState.Building) throw Error(HttpStatusCode.Conflict, "era.world_invalid_operation", text => text.InvalidOperation);
                world.State = EraWorldState.Ready;
                break;
            case RecoverEraWorldCommand:
                world.HostPlayerId = player.Id;
                world.AuthorityEpoch = command.AuthorityEpoch;
                break;
            case SetEraWorldEquipmentRestrictionCommand restriction:
                if (world.IsEquippedOperationsRestricted == restriction.IsRestricted)
                    return new T { Code = HttpStatusCode.OK, Data = Map(world, members) };
                world.IsEquippedOperationsRestricted = restriction.IsRestricted;
                world.RestrictionVersion = checked(world.RestrictionVersion + 1);
                break;
            case JoinEraWorldCommand:
                _worlds.AddMember(member!);
                members.Add(member!);
                break;
            case LeaveEraWorldCommand:
                member!.IsContinuing = false;
                member.ActivePlayerId = null;
                member.ActiveCharacterId = null;
                if (!members.Any(item => item.IsContinuing))
                {
                    await FinishWorldAsync(world, teamState, members, EraWorldEndReason.LastMemberLeft, _clock.GetUtcNow(), cancellationToken).ConfigureAwait(false);
                    return new T { Code = HttpStatusCode.OK, Data = Map(world, members) };
                }
                break;
            case EndEraWorldCommand end:
                if (end.ConstructionFailed && world.State != EraWorldState.Building)
                    throw Error(HttpStatusCode.Conflict, "era.world_invalid_operation", text => text.InvalidOperation);
                await FinishWorldAsync(world, teamState, members,
                    end.ConstructionFailed ? EraWorldEndReason.ConstructionFailed : EraWorldEndReason.Abandoned, _clock.GetUtcNow(), cancellationToken).ConfigureAwait(false);
                return new T { Code = HttpStatusCode.OK, Data = Map(world, members) };
            default:
                throw Error(HttpStatusCode.BadRequest, "era.world_invalid_operation", text => text.InvalidOperation);
        }
        world.Version = checked(world.Version + 1);
        return new T { Code = HttpStatusCode.OK, Data = Map(world, members) };
    }

    public async Task<EraWorldDto?> GetAsync(Guid? worldId, CancellationToken cancellationToken)
    {
        var player = await _players.RequireCurrentAsync(false, cancellationToken).ConfigureAwait(false);
        var snapshot = await _worlds.ReadSnapshotAsync(worldId, player.Id, cancellationToken).ConfigureAwait(false);
        if (snapshot == null && !worldId.HasValue) return null;
        if (snapshot == null) throw Error(HttpStatusCode.NotFound, "era.world_not_found", text => text.NotFound);
        return Map(snapshot.World, snapshot.Members);
    }

    private EraWorldDto Map(EraWorld world, IEnumerable<EraWorldMember> members)
    {
        return new EraWorldDto
        {
            WorldInstanceId = world.Id, TeamId = world.TeamId, WorldDefinitionId = world.WorldDefinitionId,
            State = world.State, HostPlayerId = world.HostPlayerId, AuthorityEpoch = world.AuthorityEpoch, Version = world.Version,
            IsEquippedOperationsRestricted = world.IsEquippedOperationsRestricted, RestrictionVersion = world.RestrictionVersion,
            CreatedAt = world.CreatedAt, EndedAt = world.EndedAt,
            EndReason = world.EndReason, LastActiveAt = world.LastActiveAt,
            HeartbeatIntervalSeconds = _lifetime.HeartbeatIntervalSeconds,
            InactivityTimeoutSeconds = _lifetime.InactivityTimeoutSeconds,
            Members = members.Select(member => new EraWorldMemberDto
            {
                PlayerId = member.PlayerId, CharacterId = member.CharacterId, MemberId = member.MemberId, IsContinuing = member.IsContinuing
            }).ToList()
        };
    }
}
