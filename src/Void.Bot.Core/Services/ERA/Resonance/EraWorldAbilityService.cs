using System.Net;
using System.Text.Json;
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

namespace Void.Bot.Core.Services.ERA.Resonance;

public interface IEraWorldAbilityService : IScope
{
    Task<GetEraWorldAbilityProfilesResponse> GetAsync(GetEraWorldAbilityProfilesRequest request, CancellationToken cancellationToken);
}

/// <summary>用 SQL 世界成员与实时队伍租约双重核验当前主机，仅向当前主机返回成员谐振技能配装与学习记录。</summary>
public sealed class EraWorldAbilityService : IEraWorldAbilityService
{
    private readonly IEraWorldDataProvider _worlds;
    private readonly IEraTeamDataProvider _teams;
    private readonly IEraPlayerContext _players;
    private readonly IEraResonanceDataProvider _skills;
    private readonly ILocalizationService _localization;
    private readonly TimeProvider _clock;

    public EraWorldAbilityService(IEraWorldDataProvider worlds, IEraTeamDataProvider teams, IEraPlayerContext players,
        IEraResonanceDataProvider skills, ILocalizationService localization, TimeProvider clock)
    {
        _worlds = worlds; _teams = teams; _players = players; _skills = skills; _localization = localization; _clock = clock;
    }

    public async Task<GetEraWorldAbilityProfilesResponse> GetAsync(GetEraWorldAbilityProfilesRequest request, CancellationToken cancellationToken)
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
        var result = new EraWorldAbilityProfilesDto { WorldInstanceId = snapshot.World.Id, AuthorityEpoch = team.AuthorityEpoch };
        var now = _clock.GetUtcNow();
        foreach (var member in snapshot.Members.Where(member => member.IsContinuing && member.ActivePlayerId == member.PlayerId &&
            member.ActiveCharacterId == member.CharacterId && team.Members.Any(live => live.PlayerId == member.PlayerId &&
                live.CharacterId == member.CharacterId && live.MemberId == member.MemberId && live.ExpiresAt > now)))
        {
            var record = await _skills.GetProfileAsync(member.CharacterId, cancellationToken).ConfigureAwait(false);
            var learned = await _skills.GetLearnedResonancesAsync(member.CharacterId, cancellationToken).ConfigureAwait(false);
            var profile = record == null ? new CharacterResonanceProfile() : JsonSerializer.Deserialize<CharacterResonanceProfile>(record.Payload)
                ?? throw new InvalidOperationException("角色谐振技能配装为空。");
            profile.Version = record?.Version ?? 0;
            result.Members.Add(new EraWorldMemberAbilityProfileDto { MemberId = member.MemberId, CharacterId = member.CharacterId, Profile = profile, LearnedSkills = learned });
        }
        // 跨存储只读不能持有写锁；返回前重验主机代次、成员业务版本、SQL 世界和配置版本。
        var after = await RequireTeamAsync(snapshot.World, actor.Id, request, cancellationToken).ConfigureAwait(false);
        var world = await _worlds.GetWorldAsync(snapshot.World.Id, cancellationToken).ConfigureAwait(false);
        if (after.PeersVersion != team.PeersVersion || world == null || world.State != EraWorldState.Ready ||
            world.HostPlayerId != actor.Id || world.AuthorityEpoch != request.AuthorityEpoch || world.Version != snapshot.World.Version)
            throw Error(HttpStatusCode.Conflict, "era.world_authority_changed", value => value.AuthorityChanged);
        foreach (var member in result.Members)
        {
            var profile = await _skills.GetProfileAsync(member.CharacterId, cancellationToken).ConfigureAwait(false);
            var learned = await _skills.GetLearnedResonancesAsync(member.CharacterId, cancellationToken).ConfigureAwait(false);
            if ((profile?.Version ?? 0) != member.Profile.Version || !learned.Order(StringComparer.Ordinal).SequenceEqual(member.LearnedSkills.Order(StringComparer.Ordinal)))
                throw Error(HttpStatusCode.Conflict, "era.world_member_unavailable", value => value.MemberUnavailable);
        }
        return new GetEraWorldAbilityProfilesResponse { Code = HttpStatusCode.OK, Data = result };
    }

    private async Task<EraTeam> RequireTeamAsync(EraWorld world, Guid playerId, GetEraWorldAbilityProfilesRequest request, CancellationToken cancellationToken)
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
