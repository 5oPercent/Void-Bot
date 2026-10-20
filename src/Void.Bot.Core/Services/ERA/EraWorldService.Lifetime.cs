using System.Net;
using System.Text.Json;
using StackExchange.Redis;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA;

public partial interface IEraWorldService
{
    Task<HeartbeatEraWorldResponse> HeartbeatAsync(HeartbeatEraWorldCommand command, CancellationToken cancellationToken);
    Task<CheckEraWorldTimeoutResponse> CheckTimeoutAsync(CheckEraWorldTimeoutCommand command, CancellationToken cancellationToken);
}

public sealed partial class EraWorldService
{
    public async Task<HeartbeatEraWorldResponse> HeartbeatAsync(HeartbeatEraWorldCommand command, CancellationToken cancellationToken)
    {
        if (command.WorldInstanceId == Guid.Empty || command.MemberId == Guid.Empty)
            throw Error(HttpStatusCode.BadRequest, "era.world_invalid_operation", text => text.InvalidOperation);
        var player = await _players.RequireCurrentAsync(true, cancellationToken).ConfigureAwait(false);
        var initial = await _worlds.GetWorldAsync(command.WorldInstanceId, cancellationToken).ConfigureAwait(false);
        if (initial == null) throw Error(HttpStatusCode.NotFound, "era.world_not_found", text => text.NotFound);
        var teamState = await _worlds.LockTeamAsync(initial.TeamId, cancellationToken).ConfigureAwait(false);
        var world = await _worlds.LockWorldAsync(initial.Id, cancellationToken).ConfigureAwait(false);
        if (world == null) throw Error(HttpStatusCode.NotFound, "era.world_not_found", text => text.NotFound);
        var members = await _worlds.GetMembersAsync(world.Id, false, cancellationToken).ConfigureAwait(false);
        var member = members.SingleOrDefault(item => item.PlayerId == player.Id && item.MemberId == command.MemberId);
        if (member == null) throw Error(HttpStatusCode.NotFound, "era.world_not_found", text => text.NotFound);
        if (world.State is not (EraWorldState.Building or EraWorldState.Ready) || teamState.ActiveWorldId != world.Id)
            throw Error(HttpStatusCode.Conflict, "era.world_ended", text => text.Ended);
        if (!member.IsContinuing) throw Error(HttpStatusCode.NotFound, "era.world_not_found", text => text.NotFound);
        var character = await _characters.GetCharacterByIdAsync(member.CharacterId, cancellationToken).ConfigureAwait(false);
        if (character == null || character.PlayerId != player.Id || character.IsDisabled)
            throw Error(HttpStatusCode.Conflict, "era.world_member_unavailable", text => text.MemberUnavailable);
        var now = _clock.GetUtcNow();
        if (now > world.LastActiveAt) world.LastActiveAt = now;
        world.TimeoutObservedAt = null;
        return new HeartbeatEraWorldResponse
        {
            Code = HttpStatusCode.OK,
            Data = new EraWorldHeartbeatDto
            {
                WorldInstanceId = world.Id, LastActiveAt = world.LastActiveAt,
                HeartbeatIntervalSeconds = _lifetime.HeartbeatIntervalSeconds,
                InactivityTimeoutSeconds = _lifetime.InactivityTimeoutSeconds
            }
        };
    }

    public async Task<CheckEraWorldTimeoutResponse> CheckTimeoutAsync(CheckEraWorldTimeoutCommand command, CancellationToken cancellationToken)
    {
        if (command.WorldInstanceId == Guid.Empty)
            throw Error(HttpStatusCode.BadRequest, "era.world_invalid_operation", text => text.InvalidOperation);
        var initial = await _worlds.GetWorldAsync(command.WorldInstanceId, cancellationToken).ConfigureAwait(false);
        if (initial == null) return new CheckEraWorldTimeoutResponse { Status = EraWorldTimeoutStatus.NotFound };
        var teamState = await _worlds.LockTeamAsync(initial.TeamId, cancellationToken).ConfigureAwait(false);
        var world = await _worlds.LockWorldAsync(initial.Id, cancellationToken).ConfigureAwait(false);
        if (world == null) return new CheckEraWorldTimeoutResponse { Status = EraWorldTimeoutStatus.NotFound };
        if (world.State is EraWorldState.Ended or EraWorldState.BuildFailed)
            return new CheckEraWorldTimeoutResponse { Status = EraWorldTimeoutStatus.Ended };
        var now = _clock.GetUtcNow();
        if ((world.State != EraWorldState.Building && world.State != EraWorldState.Ready) || teamState.ActiveWorldId != world.Id ||
            world.LastActiveAt > now.AddSeconds(-_lifetime.InactivityTimeoutSeconds) ||
            world.LastLifetimeCheckAt > now.AddSeconds(-_lifetime.ScanIntervalSeconds))
            return new CheckEraWorldTimeoutResponse { Status = EraWorldTimeoutStatus.Deferred };
        world.LastLifetimeCheckAt = now;
        if (!_setting.Enabled)
        {
            world.TimeoutObservedAt = null;
            return new CheckEraWorldTimeoutResponse { Status = EraWorldTimeoutStatus.Unavailable };
        }
        var members = await _worlds.GetMembersAsync(world.Id, true, cancellationToken).ConfigureAwait(false);
        try
        {
            // 与队伍迁移、续期共用锁并失效晚到提交；读取失败不能当作队伍不存在。
            await _teams.FenceAuthorityAsync(world.TeamId, cancellationToken).ConfigureAwait(false);
            var team = await _teams.GetTeamAsync(world.TeamId, cancellationToken).ConfigureAwait(false);
            var continuing = team?.Members.Where(candidate => candidate.ExpiresAt > now && members.Any(member =>
                member.IsContinuing && member.PlayerId == candidate.PlayerId && member.MemberId == candidate.MemberId &&
                member.CharacterId == candidate.CharacterId)).ToList();
            if (team != null && team.ExpiresAt > now && continuing != null && continuing.Any(candidate =>
                    candidate.CanHost || !team.IsMigrating && candidate.PlayerId == team.HostPlayerId))
            {
                world.LastActiveAt = now;
                world.TimeoutObservedAt = null;
                return new CheckEraWorldTimeoutResponse { Status = EraWorldTimeoutStatus.Alive };
            }
        }
        catch (Exception exception) when (exception is RedisException or JsonException)
        {
            // 故障打断缺失确认；恢复后重新做两次成功观察，不把停机时间计作确认。
            world.TimeoutObservedAt = null;
            return new CheckEraWorldTimeoutResponse { Status = EraWorldTimeoutStatus.Unavailable };
        }
        if (!world.TimeoutObservedAt.HasValue)
        {
            world.TimeoutObservedAt = now;
            return new CheckEraWorldTimeoutResponse { Status = EraWorldTimeoutStatus.Missing };
        }
        if (world.TimeoutObservedAt > now.AddSeconds(-_lifetime.MissingConfirmationSeconds))
            return new CheckEraWorldTimeoutResponse { Status = EraWorldTimeoutStatus.Missing };
        EraWorldLifecycle.Finish(world, teamState, members, EraWorldEndReason.InactivityTimeout, now);
        return new CheckEraWorldTimeoutResponse { Status = EraWorldTimeoutStatus.Ended };
    }
}
