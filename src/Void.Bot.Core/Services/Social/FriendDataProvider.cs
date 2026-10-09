using Void.Bot.Message.Dto.Social;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Account;
using Void.Bot.Core.Domain.Social;
using Void.Bot.Message.Enum.Social;

namespace Void.Bot.Core.Services.Social;

public interface IFriendDataProvider : IScope
{
    Task<bool> LockAccountsAsync(Guid first, Guid second, CancellationToken cancellationToken);
    Task<bool> LockAccountSetAsync(IReadOnlyCollection<Guid> accounts, CancellationToken cancellationToken);
    Task<FriendRequest?> LockRequestAsync(Guid requestId, CancellationToken cancellationToken);
    Task<Dictionary<Guid, UserAccount>> GetAccountsAsync(IReadOnlyCollection<Guid> accountIds, CancellationToken cancellationToken);
    Task<List<Guid>> GetBlockedAccountIdsAsync(Guid account, CancellationToken cancellationToken);
    Task<UserAccount?> GetAccountAsync(Guid accountId, CancellationToken cancellationToken);
    Task<UserAccount?> FindAccountAsync(string name, string suffix, CancellationToken cancellationToken);
    Task<bool> IsBlockedAsync(Guid first, Guid second, CancellationToken cancellationToken);
    Task<bool> AreFriendsAsync(Guid first, Guid second, CancellationToken cancellationToken);
    Task<FriendRequest?> GetRequestAsync(Guid requestId, CancellationToken cancellationToken);
    Task<FriendRequest?> GetRequestReceiptAsync(Guid sender, Guid requestId, CancellationToken cancellationToken);
    Task<List<FriendRequest>> GetPendingPairAsync(Guid first, Guid second, CancellationToken cancellationToken);
    Task<Friendship?> GetFriendshipAsync(Guid first, Guid second, CancellationToken cancellationToken);
    Task<AccountBlock?> GetBlockAsync(Guid account, Guid target, CancellationToken cancellationToken);
    Task<SocialPageDto<Friendship>> GetFriendshipsAsync(Guid account, int pageIndex, int pageSize, CancellationToken cancellationToken);
    Task<SocialPageDto<FriendRequest>> GetRequestsAsync(Guid account, bool outgoing, int pageIndex, int pageSize, CancellationToken cancellationToken);
    Task<SocialPageDto<AccountBlock>> GetBlocksAsync(Guid account, int pageIndex, int pageSize, CancellationToken cancellationToken);
    Task<int> CountPendingAsync(Guid account, CancellationToken cancellationToken);
    Task<int> CountFriendsAsync(Guid account, CancellationToken cancellationToken);
    Task AddRequestAsync(FriendRequest request, CancellationToken cancellationToken);
    Task AddFriendshipAsync(Friendship friendship, CancellationToken cancellationToken);
    Task AddBlockAsync(AccountBlock block, CancellationToken cancellationToken);
    void RemoveFriendship(Friendship friendship);
    void RemoveBlock(AccountBlock block);
}

public class FriendDataProvider : IFriendDataProvider
{
    private readonly HashSet<Guid> _lockedAccounts = new HashSet<Guid>();
    private readonly VoidBotDbContext _database;

    public FriendDataProvider(VoidBotDbContext database)
    {
        _database = database;
    }

    public async Task<bool> LockAccountsAsync(Guid first, Guid second, CancellationToken cancellationToken)
    {
        return await LockAccountSetAsync(new[] { first, second }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> LockAccountSetAsync(IReadOnlyCollection<Guid> accounts, CancellationToken cancellationToken)
    {
        var ids = accounts.Distinct().OrderBy(value => value.ToString("D"), StringComparer.Ordinal).ToArray();
        if (_lockedAccounts.Count > 0)
        {
            var highest = _lockedAccounts.Select(value => value.ToString("D")).OrderBy(value => value,
                StringComparer.Ordinal).Last();
            if (ids.Any(value => !_lockedAccounts.Contains(value) &&
                StringComparer.Ordinal.Compare(value.ToString("D"), highest) < 0))
            {
                return false;
            }
        }
        foreach (var id in ids)
        {
            var lockedAccountRows = await _database.Set<UserAccount>()
                .FromSqlInterpolated($"SELECT * FROM user_account WHERE id = {id.ToString()} FOR UPDATE")
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            _lockedAccounts.Add(id);
            if (lockedAccountRows.Count != 1)
            {
                return false;
            }
        }
        return true;
    }

    public async Task<Dictionary<Guid, UserAccount>> GetAccountsAsync(IReadOnlyCollection<Guid> accountIds,
        CancellationToken cancellationToken)
    {
        return await _database.Set<UserAccount>().AsNoTracking().Where(value => accountIds.Contains(value.Id))
            .ToDictionaryAsync(value => value.Id, cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<Guid>> GetBlockedAccountIdsAsync(Guid account, CancellationToken cancellationToken)
    {
        return await _database.Set<AccountBlock>().AsNoTracking().Where(value =>
            value.AccountId == account || value.BlockedAccountId == account)
            .Select(value => value.AccountId == account ? value.BlockedAccountId : value.AccountId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<UserAccount?> GetAccountAsync(Guid accountId, CancellationToken cancellationToken)
    {
        return await _database.Set<UserAccount>().AsNoTracking()
            .SingleOrDefaultAsync(value => value.Id == accountId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<UserAccount?> FindAccountAsync(string name, string suffix, CancellationToken cancellationToken)
    {
        return await _database.Set<UserAccount>().AsNoTracking()
            .SingleOrDefaultAsync(value => value.Name == name && value.Suffix == suffix, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> IsBlockedAsync(Guid first, Guid second, CancellationToken cancellationToken)
    {
        if (_database.Database.CurrentTransaction != null)
        {
            var rows = await _database.Set<AccountBlock>().FromSqlInterpolated(
                $"SELECT * FROM account_block WHERE (account_id = {first.ToString()} AND blocked_account_id = {second.ToString()}) OR (account_id = {second.ToString()} AND blocked_account_id = {first.ToString()}) FOR UPDATE")
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            return rows.Count > 0;
        }
        return await _database.Set<AccountBlock>().AnyAsync(value =>
            (value.AccountId == first && value.BlockedAccountId == second) ||
            (value.AccountId == second && value.BlockedAccountId == first), cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> AreFriendsAsync(Guid first, Guid second, CancellationToken cancellationToken)
    {
        if (await IsBlockedAsync(first, second, cancellationToken).ConfigureAwait(false))
        {
            return false;
        }
        if (_database.Database.CurrentTransaction != null)
        {
            var rows = await _database.Set<Friendship>().FromSqlInterpolated(
                $"SELECT * FROM friendship WHERE (first_account_id = {first.ToString()} AND second_account_id = {second.ToString()}) OR (first_account_id = {second.ToString()} AND second_account_id = {first.ToString()}) FOR UPDATE")
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            return rows.Count > 0;
        }
        return await _database.Set<Friendship>().AnyAsync(value =>
            (value.FirstAccountId == first && value.SecondAccountId == second) ||
            (value.FirstAccountId == second && value.SecondAccountId == first), cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<FriendRequest?> GetRequestAsync(Guid requestId, CancellationToken cancellationToken)
    {
        return await _database.Set<FriendRequest>().AsNoTracking().SingleOrDefaultAsync(value => value.Id == requestId,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<FriendRequest?> LockRequestAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var rows = await _database.Set<FriendRequest>()
            .FromSqlInterpolated($"SELECT * FROM friend_request WHERE id = {requestId.ToString()} FOR UPDATE")
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public async Task<FriendRequest?> GetRequestReceiptAsync(Guid sender, Guid requestId,
        CancellationToken cancellationToken)
    {
        return await _database.Set<FriendRequest>().SingleOrDefaultAsync(value =>
            value.SenderAccountId == sender && value.RequestId == requestId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<List<FriendRequest>> GetPendingPairAsync(Guid first, Guid second,
        CancellationToken cancellationToken)
    {
        return await _database.Set<FriendRequest>().Where(value => value.Status == FriendRequestStatus.Pending &&
            ((value.SenderAccountId == first && value.RecipientAccountId == second) ||
             (value.SenderAccountId == second && value.RecipientAccountId == first)))
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Friendship?> GetFriendshipAsync(Guid first, Guid second, CancellationToken cancellationToken)
    {
        return await _database.Set<Friendship>().SingleOrDefaultAsync(value =>
            (value.FirstAccountId == first && value.SecondAccountId == second) ||
            (value.FirstAccountId == second && value.SecondAccountId == first), cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<AccountBlock?> GetBlockAsync(Guid account, Guid target, CancellationToken cancellationToken)
    {
        return await _database.Set<AccountBlock>().SingleOrDefaultAsync(value =>
            value.AccountId == account && value.BlockedAccountId == target, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<SocialPageDto<Friendship>> GetFriendshipsAsync(Guid account, int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        var query = _database.Set<Friendship>().AsNoTracking().Where(value =>
            value.FirstAccountId == account || value.SecondAccountId == account)
            .OrderByDescending(value => value.CreatedAt).ThenBy(value => value.Id);
        return await ReadPageAsync(query, pageIndex, pageSize, cancellationToken).ConfigureAwait(false);
    }

    public async Task<SocialPageDto<FriendRequest>> GetRequestsAsync(Guid account, bool outgoing, int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        var query = _database.Set<FriendRequest>().AsNoTracking().Where(value =>
            outgoing ? value.SenderAccountId == account : value.RecipientAccountId == account)
            .OrderByDescending(value => value.CreatedAt).ThenBy(value => value.Id);
        return await ReadPageAsync(query, pageIndex, pageSize, cancellationToken).ConfigureAwait(false);
    }

    public async Task<SocialPageDto<AccountBlock>> GetBlocksAsync(Guid account, int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        var query = _database.Set<AccountBlock>().AsNoTracking().Where(value => value.AccountId == account)
            .OrderByDescending(value => value.CreatedAt).ThenBy(value => value.Id);
        return await ReadPageAsync(query, pageIndex, pageSize, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<SocialPageDto<T>> ReadPageAsync<T>(IQueryable<T> query,
        int pageIndex, int pageSize, CancellationToken cancellationToken) where T : class
    {
        var count = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return new SocialPageDto<T>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = count,
            Items = items
        };
    }

    public async Task<int> CountPendingAsync(Guid account, CancellationToken cancellationToken)
    {
        return await _database.Set<FriendRequest>().CountAsync(value => value.Status == FriendRequestStatus.Pending &&
            (value.SenderAccountId == account || value.RecipientAccountId == account), cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<int> CountFriendsAsync(Guid account, CancellationToken cancellationToken)
    {
        return await _database.Set<Friendship>().CountAsync(value =>
            value.FirstAccountId == account || value.SecondAccountId == account, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task AddRequestAsync(FriendRequest request, CancellationToken cancellationToken)
    {
        await _database.Set<FriendRequest>().AddAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task AddFriendshipAsync(Friendship friendship, CancellationToken cancellationToken)
    {
        await _database.Set<Friendship>().AddAsync(friendship, cancellationToken).ConfigureAwait(false);
    }

    public async Task AddBlockAsync(AccountBlock block, CancellationToken cancellationToken)
    {
        await _database.Set<AccountBlock>().AddAsync(block, cancellationToken).ConfigureAwait(false);
    }

    public void RemoveFriendship(Friendship friendship)
    {
        _database.Set<Friendship>().Remove(friendship);
    }

    public void RemoveBlock(AccountBlock block)
    {
        _database.Set<AccountBlock>().Remove(block);
    }
}
