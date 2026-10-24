using System.Net;
using StackExchange.Redis;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Resources.i18n.ERA;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Core.Services.ERA.Team;
using Void.Bot.Core.Services.ERA.World;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment;

public interface IEraWorldEquipmentService : IScope
{
    Task<GetEraWorldEquipmentResponse> GetAsync(GetEraWorldEquipmentRequest request, CancellationToken cancellationToken);
}

/// <summary>用 SQL 世界成员与实时队伍租约双重核验当前主机，返回最小已穿数据。</summary>
public sealed class EraWorldEquipmentService : IEraWorldEquipmentService
{
    private readonly IEraWorldDataProvider _worlds;
    private readonly IEraTeamDataProvider _teams;
    private readonly IEraPlayerContext _players;
    private readonly IEraEquipmentDataProvider _equipment;
    private readonly ILocalizationService _localization;
    private readonly TimeProvider _clock;

    public EraWorldEquipmentService(IEraWorldDataProvider worlds, IEraTeamDataProvider teams, IEraPlayerContext players,
        IEraEquipmentDataProvider equipment, ILocalizationService localization, TimeProvider clock)
    {
        _worlds = worlds; _teams = teams; _players = players; _equipment = equipment; _localization = localization; _clock = clock;
    }

    public async Task<GetEraWorldEquipmentResponse> GetAsync(GetEraWorldEquipmentRequest request, CancellationToken cancellationToken)
    {
        var actor = await _players.RequireCurrentAsync(false, cancellationToken).ConfigureAwait(false);
        var snapshot = await _worlds.ReadSnapshotAsync(request.WorldInstanceId, actor.Id, cancellationToken).ConfigureAwait(false);
        if (snapshot == null || snapshot.World.State != EraWorldState.Ready || snapshot.World.HostPlayerId != actor.Id ||
            snapshot.World.AuthorityEpoch != request.AuthorityEpoch ||
            !snapshot.Members.Any(member => member.PlayerId == actor.Id && member.MemberId == request.MemberId && member.IsContinuing &&
                member.ActivePlayerId == actor.Id && member.ActiveCharacterId == member.CharacterId))
            throw Error(HttpStatusCode.Forbidden, "era.world_host_required", value => value.HostRequired);
        var team = await RequireTeamAsync(snapshot.World, actor.Id, request, cancellationToken).ConfigureAwait(false);
        if (!snapshot.Members.Any(member => member.PlayerId == actor.Id && team.Members.Any(live => live.PlayerId == actor.Id &&
            live.MemberId == member.MemberId && live.CharacterId == member.CharacterId)))
            throw Error(HttpStatusCode.Forbidden, "era.world_host_required", value => value.HostRequired);
        var manifest = await EraEquipmentService.ReadConfigurationAsync(_equipment, false, cancellationToken).ConfigureAwait(false);
        if (manifest == null) throw Error(HttpStatusCode.ServiceUnavailable, "era.world_equipment_unavailable", value => value.InvalidOperation);
        var result = new EraWorldEquipmentDto { WorldInstanceId = snapshot.World.Id, AuthorityEpoch = team.AuthorityEpoch, ManifestVersion = manifest.Version };
        var now = _clock.GetUtcNow();
        foreach (var member in snapshot.Members.Where(member => member.IsContinuing && member.ActivePlayerId == member.PlayerId &&
            member.ActiveCharacterId == member.CharacterId && team.Members.Any(live => live.PlayerId == member.PlayerId &&
                live.CharacterId == member.CharacterId && live.MemberId == member.MemberId && live.ExpiresAt > now)))
        {
            Void.Bot.Core.Services.ERA.Equipment.Models.EraEquipmentReadSnapshot? assets;
            try
            {
                assets = await _equipment.GetCharacterSnapshotAsync(member.CharacterId, member.PlayerId, cancellationToken).ConfigureAwait(false);
            }
            catch (EraEquipmentConcurrencyException)
            {
                throw Error(HttpStatusCode.Conflict, "era.world_member_unavailable", value => value.MemberUnavailable);
            }
            var state = assets == null ? null : EraEquipmentService.CreateState(assets, manifest, null);
            if (state == null || state.IsCharacterDisabled)
                throw Error(HttpStatusCode.Conflict, "era.world_member_unavailable", value => value.MemberUnavailable);
            var equipped = state.EquippedSlots.Where(slot => slot.Equipment != null).Select(slot => slot.Equipment!).ToList();
            if (equipped.Any(item => item.EquipmentType == EraEquipmentType.Weapon &&
                (item.WeaponTemplate == null || item.Weapon!.Slots.SelectMany(slot => slot.Candidates)
                    .Any(candidate => !manifest.Weapons.Modifications.Any(modification => modification.Id == candidate.ModDefinitionId)))))
                throw Error(HttpStatusCode.ServiceUnavailable, "era.world_equipment_unavailable", value => value.InvalidOperation);
            result.Members.Add(new EraWorldMemberEquipmentDto { CharacterId = member.CharacterId, EquipmentVersion = state.EquipmentVersion, Equipment = equipped });
        }
        // 跨存储只读不能持有写锁；返回前重验主机代次、成员业务版本、SQL 世界和配置版本。
        var after = await RequireTeamAsync(snapshot.World, actor.Id, request, cancellationToken).ConfigureAwait(false);
        var world = await _worlds.GetWorldAsync(snapshot.World.Id, cancellationToken).ConfigureAwait(false);
        var configuration = await _equipment.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        if (after.PeersVersion != team.PeersVersion || world == null || world.State != EraWorldState.Ready ||
            world.HostPlayerId != actor.Id || world.AuthorityEpoch != request.AuthorityEpoch || world.Version != snapshot.World.Version ||
            configuration?.Version != manifest.Version)
            throw Error(HttpStatusCode.Conflict, "era.world_authority_changed", value => value.AuthorityChanged);
        return new GetEraWorldEquipmentResponse { Code = HttpStatusCode.OK, Data = result };
    }

    private async Task<EraTeam> RequireTeamAsync(EraWorld world, Guid playerId, GetEraWorldEquipmentRequest request, CancellationToken cancellationToken)
    {
        EraTeam? team;
        try { team = await _teams.GetTeamAsync(world.TeamId, cancellationToken).ConfigureAwait(false); }
        catch (RedisException) { throw Error(HttpStatusCode.ServiceUnavailable, "era.world_networking_unavailable", value => value.NetworkingUnavailable); }
        var now = _clock.GetUtcNow();
        if (team == null || team.ExpiresAt <= now || team.IsMigrating || team.HostPlayerId != playerId ||
            team.AuthorityEpoch != request.AuthorityEpoch || !team.Members.Any(member => member.PlayerId == playerId &&
                member.MemberId == request.MemberId && member.ExpiresAt > now))
            throw Error(HttpStatusCode.Forbidden, "era.world_host_required", value => value.HostRequired);
        return team;
    }

    private EraWorldAccessException Error(HttpStatusCode status, string code,
        System.Linq.Expressions.Expression<Func<EraWorldContextTemplate, object>> text) => new EraWorldAccessException(status, code, _localization.GetValue(text));
}
