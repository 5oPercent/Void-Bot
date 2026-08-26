using System.Security.Cryptography;
using System.Text.Json;
using Serilog;
using Void.Bot.Core.Services;

namespace Void.Bot.Core.Caching;

public interface IAccountPasswordResetLinkStore : IScope
{
    Task<CreatePasswordResetLinkResult> CreateAsync(Guid userId, string email, TimeSpan ttl, CancellationToken cancellationToken);
    Task<TakePasswordResetLinkResult> TakeAsync(string token, CancellationToken cancellationToken);
    Task RemoveAsync(string token, CancellationToken cancellationToken);
}

public sealed class AccountPasswordResetLinkStore(
    ICacheManager cacheManager,
    TimeProvider timeProvider,
    ILogger logger) : IAccountPasswordResetLinkStore
{
    private const string KeyPrefix = "account:password-reset-link:";
    private const string ActiveKeyPrefix = "account:password-reset-link:active:";

    public async Task<CreatePasswordResetLinkResult> CreateAsync(
        Guid userId, string email, TimeSpan ttl, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(email) || ttl <= TimeSpan.Zero)
            return new CreatePasswordResetLinkResult.Rejected();

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var now = timeProvider.GetUtcNow();
        var record = new PasswordResetLinkRecord(userId, email, now, now.Add(ttl));
        try
        {
            var activeKey = $"{ActiveKeyPrefix}{userId:N}";
            var previous = await cacheManager.Get<string>(activeKey, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(previous))
                await cacheManager.Remove($"{KeyPrefix}{previous}", cancellationToken).ConfigureAwait(false);
            await cacheManager.Set($"{KeyPrefix}{token}", record, ttl, cancellationToken).ConfigureAwait(false);
            await cacheManager.Set(activeKey, token, ttl, cancellationToken).ConfigureAwait(false);
            return new CreatePasswordResetLinkResult.Created(token, record.ExpiresAt);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.Warning(exception, "Password reset link store is unavailable");
            return new CreatePasswordResetLinkResult.Unavailable();
        }
    }

    public async Task<TakePasswordResetLinkResult> TakeAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(token) || token.Length != 43)
            return new TakePasswordResetLinkResult.Missing();
        try
        {
            var record = await cacheManager.Take<PasswordResetLinkRecord>($"{KeyPrefix}{token}", cancellationToken).ConfigureAwait(false);
            if (record is null || record.ExpiresAt <= timeProvider.GetUtcNow())
                return new TakePasswordResetLinkResult.Missing();
            await cacheManager.Remove($"{ActiveKeyPrefix}{record.UserId:N}", cancellationToken).ConfigureAwait(false);
            return new TakePasswordResetLinkResult.Found(record);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (JsonException exception)
        {
            logger.Warning(exception, "Password reset link record could not be deserialized");
            return new TakePasswordResetLinkResult.Missing();
        }
        catch (Exception exception)
        {
            logger.Warning(exception, "Password reset link store is unavailable");
            return new TakePasswordResetLinkResult.Unavailable();
        }
    }

    public Task RemoveAsync(string token, CancellationToken cancellationToken) =>
        cacheManager.Remove($"{KeyPrefix}{token}", cancellationToken);
}

public sealed record PasswordResetLinkRecord(Guid UserId, string Email, DateTimeOffset IssuedAt, DateTimeOffset ExpiresAt);
public abstract record CreatePasswordResetLinkResult
{
    public sealed record Created(string Token, DateTimeOffset ExpiresAt) : CreatePasswordResetLinkResult;
    public sealed record Rejected : CreatePasswordResetLinkResult;
    public sealed record Unavailable : CreatePasswordResetLinkResult;
}
public abstract record TakePasswordResetLinkResult
{
    public sealed record Found(PasswordResetLinkRecord Record) : TakePasswordResetLinkResult;
    public sealed record Missing : TakePasswordResetLinkResult;
    public sealed record Unavailable : TakePasswordResetLinkResult;
}
