using System.Net;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.World.Models;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.World;

public partial interface IEraWorldService
{
    /// <summary>调用方先锁奖励作用域及相应玩家／角色，再在此锁队伍、世界并验证实时房主。</summary>
    Task<EraWorldRewardContext> LockRewardWorldAsync(SubmitEraGroundRewardCommand command, Guid playerId, bool historical,
        CancellationToken cancellationToken);
}

public sealed partial class EraWorldService
{
    public async Task<EraWorldRewardContext> LockRewardWorldAsync(SubmitEraGroundRewardCommand command, Guid playerId, bool historical,
        CancellationToken cancellationToken)
    {
        var initial = await _worlds.GetWorldAsync(command.WorldInstanceId, cancellationToken).ConfigureAwait(false);
        if (initial == null)
        {
            throw Error(HttpStatusCode.NotFound, "era.world_not_found", text => text.NotFound);
        }
        var teamState = await _worlds.LockTeamAsync(initial.TeamId, cancellationToken).ConfigureAwait(false);
        var world = await _worlds.LockWorldAsync(initial.Id, cancellationToken).ConfigureAwait(false);
        if (world == null)
        {
            throw Error(HttpStatusCode.NotFound, "era.world_not_found", text => text.NotFound);
        }
        var authority = await RequireTeamAsync(world.TeamId, playerId, command.MemberId, command.AuthorityEpoch, true, cancellationToken).ConfigureAwait(false);
        var members = await _worlds.GetMembersAsync(world.Id, false, cancellationToken).ConfigureAwait(false);
        var actor = members.SingleOrDefault(member => member.PlayerId == playerId && member.MemberId == command.MemberId && member.CharacterId == authority.Member.CharacterId);
        if (actor == null)
        {
            throw Error(HttpStatusCode.Forbidden, "era.world_member_unavailable", text => text.MemberUnavailable);
        }
        if (!historical)
        {
            if (world.State == EraWorldState.Building && teamState.ActiveWorldId == world.Id)
            {
                throw Error(HttpStatusCode.Conflict, "era.world_not_ready", text => text.ReadyRequired);
            }
            if (world.State != EraWorldState.Ready || teamState.ActiveWorldId != world.Id)
            {
                throw Error(HttpStatusCode.Conflict, "era.world_ended", text => text.Ended);
            }
            if (!actor.IsContinuing || actor.ActivePlayerId != playerId || actor.ActiveCharacterId != authority.Member.CharacterId)
            {
                throw Error(HttpStatusCode.Forbidden, "era.world_member_unavailable", text => text.MemberUnavailable);
            }
            if (world.AuthorityEpoch != command.AuthorityEpoch || world.HostPlayerId != playerId)
            {
                throw Error(HttpStatusCode.Conflict, "era.world_authority_changed", text => text.AuthorityChanged);
            }
        }
        return new EraWorldRewardContext(world, authority.Team, members);
    }

    private async Task FinishWorldAsync(EraWorld world, EraWorldTeamState teamState, IReadOnlyList<EraWorldMember> members,
        EraWorldEndReason reason, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (world.State is EraWorldState.Ended or EraWorldState.BuildFailed)
        {
            return;
        }
        await _drops.InvalidateWorldAsync(world.Id, cancellationToken).ConfigureAwait(false);
        EraWorldLifecycle.Finish(world, teamState, members, reason, now);
    }
}
