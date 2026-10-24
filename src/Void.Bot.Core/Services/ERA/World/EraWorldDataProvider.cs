using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.World.Models;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.World;

public interface IEraWorldDataProvider : IScope
{
    Task<EraWorldTeamState> LockTeamAsync(Guid teamId, CancellationToken cancellationToken);
    Task<EraWorld?> LockWorldAsync(Guid worldId, CancellationToken cancellationToken);
    Task<EraWorld?> GetWorldAsync(Guid worldId, CancellationToken cancellationToken);
    Task<EraWorld?> GetCreationAsync(Guid teamId, Guid requestId, CancellationToken cancellationToken);
    Task<EraWorldReadSnapshot?> ReadSnapshotAsync(Guid? worldId, Guid playerId, CancellationToken cancellationToken);
    Task<List<EraWorldMember>> GetMembersAsync(Guid worldId, bool tracking, CancellationToken cancellationToken);
    Task<bool> CanRegisterMembersAsync(IReadOnlyCollection<EraTeamMember> members, CancellationToken cancellationToken);
    void AddWorld(EraWorld world, IEnumerable<EraWorldMember> members);
    void AddMember(EraWorldMember member);
    Task<List<Guid>> GetTimeoutCandidatesAsync(DateTimeOffset activityCutoff, DateTimeOffset checkCutoff, int take, CancellationToken cancellationToken);
}

public sealed class EraWorldDataProvider : IEraWorldDataProvider
{
    private readonly IRepository<EraWorldTeamState> _eraWorldTeamStateRepository;
    private readonly IRepository<EraWorld> _eraWorldRepository;
    private readonly IRepository<EraWorldMember> _eraWorldMemberRepository;
    private readonly IRepository<EraCharacter> _eraCharacterRepository;

    public EraWorldDataProvider(
        IRepository<EraWorldTeamState> eraWorldTeamStateRepository,
        IRepository<EraWorld> eraWorldRepository,
        IRepository<EraWorldMember> eraWorldMemberRepository,
        IRepository<EraCharacter> eraCharacterRepository)
    {
        _eraWorldTeamStateRepository = eraWorldTeamStateRepository;
        _eraWorldRepository = eraWorldRepository;
        _eraWorldMemberRepository = eraWorldMemberRepository;
        _eraCharacterRepository = eraCharacterRepository;
    }

    public async Task<EraWorldTeamState> LockTeamAsync(Guid teamId, CancellationToken cancellationToken)
    {
        _eraWorldTeamStateRepository.RequireTransaction();
        await _eraWorldTeamStateRepository.ExecuteSqlAsync(
            $"INSERT INTO era_world_team_state (team_id, active_world_id) VALUES ({teamId.ToString()}, NULL) ON DUPLICATE KEY UPDATE team_id = team_id", cancellationToken).ConfigureAwait(false);
        var rows = await _eraWorldTeamStateRepository.QuerySql(
            $"SELECT * FROM era_world_team_state WHERE team_id = {teamId.ToString()} FOR UPDATE")
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.Single();
    }

    public async Task<EraWorld?> LockWorldAsync(Guid worldId, CancellationToken cancellationToken)
    {
        _eraWorldTeamStateRepository.RequireTransaction();
        var rows = await _eraWorldRepository.QuerySql(
            $"SELECT * FROM era_world WHERE id = {worldId.ToString()} FOR UPDATE")
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public Task<EraWorld?> GetWorldAsync(Guid worldId, CancellationToken cancellationToken)
    {
        return _eraWorldRepository.QueryNoTracking().SingleOrDefaultAsync(world => world.Id == worldId, cancellationToken);
    }

    public Task<EraWorld?> GetCreationAsync(Guid teamId, Guid requestId, CancellationToken cancellationToken)
    {
        return _eraWorldRepository.QueryNoTracking().SingleOrDefaultAsync(world => world.TeamId == teamId && world.CreationRequestId == requestId, cancellationToken);
    }

    public async Task<EraWorldReadSnapshot?> ReadSnapshotAsync(Guid? worldId, Guid playerId, CancellationToken cancellationToken)
    {
        var worlds = _eraWorldRepository.Query().Where(world => _eraWorldMemberRepository.Query().Any(member =>
            member.WorldId == world.Id && member.PlayerId == playerId &&
            (worldId.HasValue || member.IsContinuing && member.ActivePlayerId == playerId)));
        if (worldId.HasValue) worlds = worlds.Where(world => world.Id == worldId.Value);
        var rows = await (from world in worlds
            join member in _eraWorldMemberRepository.Query() on world.Id equals member.WorldId
            orderby member.PlayerId
            select new { World = world, Member = member }).AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.Count == 0 ? null : new EraWorldReadSnapshot(rows[0].World, rows.Select(row => row.Member).ToList());
    }

    public Task<List<EraWorldMember>> GetMembersAsync(Guid worldId, bool tracking, CancellationToken cancellationToken)
    {
        var query = _eraWorldMemberRepository.Query().Where(member => member.WorldId == worldId);
        return (tracking ? query : query.AsNoTracking()).OrderBy(member => member.PlayerId).ToListAsync(cancellationToken);
    }

    public async Task<bool> CanRegisterMembersAsync(IReadOnlyCollection<EraTeamMember> members, CancellationToken cancellationToken)
    {
        var players = members.Select(member => member.PlayerId).ToArray();
        var characters = members.Select(member => member.CharacterId).ToArray();
        if (players.Distinct().Count() != members.Count || characters.Distinct().Count() != members.Count) return false;
        if (await _eraWorldMemberRepository.Query().AnyAsync(member => member.ActivePlayerId.HasValue &&
                players.Contains(member.ActivePlayerId.Value), cancellationToken).ConfigureAwait(false)) return false;
        var valid = await _eraCharacterRepository.QueryNoTracking()
            .Where(character => characters.Contains(character.Id) && !character.IsDisabled)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return members.All(member => valid.Any(character => character.Id == member.CharacterId && character.PlayerId == member.PlayerId));
    }

    public void AddWorld(EraWorld world, IEnumerable<EraWorldMember> members)
    {
        _eraWorldRepository.Insert(world);
        _eraWorldMemberRepository.InsertMany(members);
    }

    public void AddMember(EraWorldMember member)
    {
        _eraWorldMemberRepository.Insert(member);
    }

    public Task<List<Guid>> GetTimeoutCandidatesAsync(DateTimeOffset activityCutoff, DateTimeOffset checkCutoff, int take, CancellationToken cancellationToken)
    {
        return _eraWorldRepository.QueryNoTracking()
            .Where(world => (world.State == EraWorldState.Building || world.State == EraWorldState.Ready) &&
                world.LastActiveAt <= activityCutoff && (world.LastLifetimeCheckAt == null || world.LastLifetimeCheckAt <= checkCutoff))
            .OrderBy(world => world.LastLifetimeCheckAt).ThenBy(world => world.Id)
            .Take(take).Select(world => world.Id).ToListAsync(cancellationToken);
    }
}
