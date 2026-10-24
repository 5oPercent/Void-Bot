using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Domain.Social;

namespace Void.Bot.Core.Services.ERA.Social;

public interface IEraSocialDataProvider : IScope
{
    Task<List<Friendship>> GetFriendEdgesAsync(IReadOnlyCollection<Guid> accounts, CancellationToken cancellationToken);
    Task<Dictionary<Guid, Guid>> GetAccountIdsAsync(IReadOnlyCollection<Guid> playerIds, CancellationToken cancellationToken);
    Task<Guid?> GetAccountIdAsync(Guid playerId, CancellationToken cancellationToken);
    Task<Dictionary<Guid, Guid>> GetPlayerIdsAsync(IReadOnlyCollection<Guid> accountIds, CancellationToken cancellationToken);
    Task<List<Guid>> GetFriendAccountIdsAsync(Guid accountId, CancellationToken cancellationToken);
}

public class EraSocialDataProvider : IEraSocialDataProvider
{
    private readonly IRepository<Friendship> _friendshipRepository;
    private readonly IRepository<EraPlayerAccountLink> _eraPlayerAccountLinkRepository;

    public EraSocialDataProvider(
        IRepository<Friendship> friendshipRepository,
        IRepository<EraPlayerAccountLink> eraPlayerAccountLinkRepository)
    {
        _friendshipRepository = friendshipRepository;
        _eraPlayerAccountLinkRepository = eraPlayerAccountLinkRepository;
    }

    public async Task<List<Friendship>> GetFriendEdgesAsync(IReadOnlyCollection<Guid> accounts,
        CancellationToken cancellationToken)
    {
        return await _friendshipRepository.QueryNoTracking().Where(value =>
            accounts.Contains(value.FirstAccountId) || accounts.Contains(value.SecondAccountId))
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Dictionary<Guid, Guid>> GetAccountIdsAsync(IReadOnlyCollection<Guid> playerIds,
        CancellationToken cancellationToken)
    {
        return await _eraPlayerAccountLinkRepository.QueryNoTracking().Where(value => playerIds.Contains(value.PlayerId))
            .ToDictionaryAsync(value => value.PlayerId, value => value.AccountId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Guid?> GetAccountIdAsync(Guid playerId, CancellationToken cancellationToken)
    {
        return await _eraPlayerAccountLinkRepository.QueryNoTracking()
            .Where(value => value.PlayerId == playerId).Select(value => (Guid?)value.AccountId)
            .SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Dictionary<Guid, Guid>> GetPlayerIdsAsync(IReadOnlyCollection<Guid> accountIds,
        CancellationToken cancellationToken)
    {
        return await _eraPlayerAccountLinkRepository.QueryNoTracking()
            .Where(value => accountIds.Contains(value.AccountId))
            .ToDictionaryAsync(value => value.AccountId, value => value.PlayerId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<List<Guid>> GetFriendAccountIdsAsync(Guid accountId, CancellationToken cancellationToken)
    {
        return await _friendshipRepository.QueryNoTracking()
            .Where(value => value.FirstAccountId == accountId || value.SecondAccountId == accountId)
            .Select(value => value.FirstAccountId == accountId ? value.SecondAccountId : value.FirstAccountId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
