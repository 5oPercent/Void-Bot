using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Social;

namespace Void.Bot.Core.Services.Social;

public interface ISocialOutboxDataProvider : IScope
{
    Task AddAsync(SocialNotificationOutbox notification, CancellationToken cancellationToken);
    Task<List<SocialNotificationOutbox>> GetDueAsync(CancellationToken cancellationToken);
    Task CompleteAsync(Guid id, CancellationToken cancellationToken);
    Task RetryAsync(Guid id, int attempts, CancellationToken cancellationToken);
    Task CleanupAsync(CancellationToken cancellationToken);
}

public class SocialOutboxDataProvider : ISocialOutboxDataProvider
{
    private readonly VoidBotDbContext _database;

    public SocialOutboxDataProvider(VoidBotDbContext database)
    {
        _database = database;
    }

    public async Task AddAsync(SocialNotificationOutbox notification, CancellationToken cancellationToken)
    {
        await _database.Set<SocialNotificationOutbox>().AddAsync(notification, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<List<SocialNotificationOutbox>> GetDueAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        return await _database.Set<SocialNotificationOutbox>().AsNoTracking()
            .Where(value => value.CompletedAt == null && value.NextAttemptAt <= now && value.Attempts < 8)
            .OrderBy(value => value.NextAttemptAt).ThenBy(value => value.Id)
            .Take(128).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task CompleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        await _database.Set<SocialNotificationOutbox>().Where(value => value.Id == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(value => value.CompletedAt, now), cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task RetryAsync(Guid id, int attempts, CancellationToken cancellationToken)
    {
        var next = DateTimeOffset.UtcNow.AddSeconds(Math.Min(300, Math.Pow(2, attempts)));
        await _database.Set<SocialNotificationOutbox>().Where(value => value.Id == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(value => value.Attempts, attempts)
                .SetProperty(value => value.NextAttemptAt, next), cancellationToken).ConfigureAwait(false);
    }

    public async Task CleanupAsync(CancellationToken cancellationToken)
    {
        var completedBefore = DateTimeOffset.UtcNow.AddDays(-1);
        var expiredBefore = DateTimeOffset.UtcNow.AddDays(-7);
        await _database.Set<SocialNotificationOutbox>().Where(value =>
            value.CompletedAt < completedBefore || value.CreatedAt < expiredBefore)
            .ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
    }
}
