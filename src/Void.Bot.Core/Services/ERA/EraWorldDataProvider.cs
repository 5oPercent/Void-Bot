using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA;

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
    private readonly VoidBotDbContext _context;

    public EraWorldDataProvider(VoidBotDbContext context) { _context = context; }

    public async Task<EraWorldTeamState> LockTeamAsync(Guid teamId, CancellationToken cancellationToken)
    {
        if (_context.Database.CurrentTransaction == null) throw new InvalidOperationException("World writes require the command transaction.");
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO era_world_team_state (team_id, active_world_id) VALUES ({teamId.ToString()}, NULL) ON DUPLICATE KEY UPDATE team_id = team_id", cancellationToken).ConfigureAwait(false);
        var rows = await _context.Set<EraWorldTeamState>().FromSqlInterpolated(
            $"SELECT * FROM era_world_team_state WHERE team_id = {teamId.ToString()} FOR UPDATE")
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.Single();
    }

    public async Task<EraWorld?> LockWorldAsync(Guid worldId, CancellationToken cancellationToken)
    {
        if (_context.Database.CurrentTransaction == null) throw new InvalidOperationException("World writes require the command transaction.");
        var rows = await _context.Set<EraWorld>().FromSqlInterpolated(
            $"SELECT * FROM era_world WHERE id = {worldId.ToString()} FOR UPDATE")
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public Task<EraWorld?> GetWorldAsync(Guid worldId, CancellationToken cancellationToken) =>
        _context.Set<EraWorld>().AsNoTracking().SingleOrDefaultAsync(world => world.Id == worldId, cancellationToken);

    public Task<EraWorld?> GetCreationAsync(Guid teamId, Guid requestId, CancellationToken cancellationToken) =>
        _context.Set<EraWorld>().AsNoTracking().SingleOrDefaultAsync(world => world.TeamId == teamId && world.CreationRequestId == requestId, cancellationToken);

    public async Task<EraWorldReadSnapshot?> ReadSnapshotAsync(Guid? worldId, Guid playerId, CancellationToken cancellationToken)
    {
        var worlds = _context.Set<EraWorld>().Where(world => _context.Set<EraWorldMember>().Any(member =>
            member.WorldId == world.Id && member.PlayerId == playerId &&
            (worldId.HasValue || member.IsContinuing && member.ActivePlayerId == playerId)));
        if (worldId.HasValue) worlds = worlds.Where(world => world.Id == worldId.Value);
        var rows = await (from world in worlds
            join member in _context.Set<EraWorldMember>() on world.Id equals member.WorldId
            orderby member.PlayerId
            select new { World = world, Member = member }).AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.Count == 0 ? null : new EraWorldReadSnapshot(rows[0].World, rows.Select(row => row.Member).ToList());
    }

    public Task<List<EraWorldMember>> GetMembersAsync(Guid worldId, bool tracking, CancellationToken cancellationToken)
    {
        var query = _context.Set<EraWorldMember>().Where(member => member.WorldId == worldId);
        return (tracking ? query : query.AsNoTracking()).OrderBy(member => member.PlayerId).ToListAsync(cancellationToken);
    }

    public async Task<bool> CanRegisterMembersAsync(IReadOnlyCollection<EraTeamMember> members, CancellationToken cancellationToken)
    {
        var players = members.Select(member => member.PlayerId).ToArray();
        var characters = members.Select(member => member.CharacterId).ToArray();
        if (players.Distinct().Count() != members.Count || characters.Distinct().Count() != members.Count) return false;
        if (await _context.Set<EraWorldMember>().AnyAsync(member => member.ActivePlayerId.HasValue &&
                players.Contains(member.ActivePlayerId.Value), cancellationToken).ConfigureAwait(false)) return false;
        var valid = await _context.Set<EraCharacter>().AsNoTracking()
            .Where(character => characters.Contains(character.Id) && !character.IsDisabled)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return members.All(member => valid.Any(character => character.Id == member.CharacterId && character.PlayerId == member.PlayerId));
    }

    public void AddWorld(EraWorld world, IEnumerable<EraWorldMember> members)
    {
        _context.Set<EraWorld>().Add(world);
        _context.Set<EraWorldMember>().AddRange(members);
    }

    public void AddMember(EraWorldMember member) => _context.Set<EraWorldMember>().Add(member);

    public Task<List<Guid>> GetTimeoutCandidatesAsync(DateTimeOffset activityCutoff, DateTimeOffset checkCutoff, int take, CancellationToken cancellationToken) =>
        _context.Set<EraWorld>().AsNoTracking()
            .Where(world => (world.State == EraWorldState.Building || world.State == EraWorldState.Ready) &&
                world.LastActiveAt <= activityCutoff && (world.LastLifetimeCheckAt == null || world.LastLifetimeCheckAt <= checkCutoff))
            .OrderBy(world => world.LastLifetimeCheckAt).ThenBy(world => world.Id)
            .Take(take).Select(world => world.Id).ToListAsync(cancellationToken);
}
