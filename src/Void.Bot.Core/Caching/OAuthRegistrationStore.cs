using System.Security.Cryptography;
using System.Text.Json;
using Serilog;
using Void.Bot.Core.Services;
using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Caching;

public interface IOAuthRegistrationStore : IScope
{
    Task<CreateOAuthRegistrationResult> CreateAsync(
        OAuthRegistrationIdentity identity,
        TimeSpan timeToLive,
        CancellationToken cancellationToken = default);

    Task<GetOAuthRegistrationResult> GetAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<TakeOAuthRegistrationResult> TakeAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task RestoreAsync(
        OAuthRegistrationRecord record,
        CancellationToken cancellationToken = default);
}

public sealed class OAuthRegistrationStore : IOAuthRegistrationStore
{
    private const int CodeByteLength = 32;
    private const int EncodedCodeLength = 43;
    private const string KeyPrefix = "oauth:registration:";
    private readonly ICacheManager _cacheManager;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger _logger;

    public OAuthRegistrationStore(ICacheManager cacheManager, TimeProvider timeProvider, ILogger logger)
    {
        _cacheManager = cacheManager;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<CreateOAuthRegistrationResult> CreateAsync(
        OAuthRegistrationIdentity identity,
        TimeSpan timeToLive,
        CancellationToken cancellationToken = default)
    {
        if (identity.Platform is AppPlatformEnum.Void ||
            string.IsNullOrWhiteSpace(identity.ProviderUserId) ||
            string.IsNullOrWhiteSpace(identity.AccessToken) ||
            timeToLive <= TimeSpan.Zero)
        {
            return new CreateOAuthRegistrationResult.Rejected();
        }

        var issuedAt = _timeProvider.GetUtcNow();
        var code = Convert.ToBase64String(RandomNumberGenerator.GetBytes(CodeByteLength))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        var record = new OAuthRegistrationRecord
        {
            Code = code,
            Platform = identity.Platform,
            ProviderUserId = identity.ProviderUserId,
            ProviderUserName = identity.ProviderUserName,
            AccessToken = identity.AccessToken,
            AccessTokenExpiresAt = identity.AccessTokenExpiresAt,
            RefreshToken = identity.RefreshToken,
            RefreshTokenExpiresAt = identity.RefreshTokenExpiresAt,
            ExtraData = identity.ExtraData,
            IssuedAt = issuedAt,
            ExpiresAt = issuedAt.Add(timeToLive)
        };

        try
        {
            await _cacheManager.Set(
                $"{KeyPrefix}{code}",
                record,
                timeToLive,
                cancellationToken).ConfigureAwait(false);
            _logger.Information(
                "OAuth registration identity stored for {Platform} until {ExpiresAt}",
                identity.Platform,
                record.ExpiresAt);
            return new CreateOAuthRegistrationResult.Created(code);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.Warning(exception, "OAuth registration identity store is unavailable");
            return new CreateOAuthRegistrationResult.Unavailable();
        }
    }

    public async Task<GetOAuthRegistrationResult> GetAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidCode(code))
        {
            return new GetOAuthRegistrationResult.Missing();
        }

        try
        {
            var record = await _cacheManager.Get<OAuthRegistrationRecord>(
                $"{KeyPrefix}{code}",
                cancellationToken).ConfigureAwait(false);
            return IsValidRecord(record, code)
                ? new GetOAuthRegistrationResult.Found(record!)
                : new GetOAuthRegistrationResult.Missing();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (JsonException exception)
        {
            _logger.Warning(exception, "OAuth registration identity could not be deserialized");
            return new GetOAuthRegistrationResult.Missing();
        }
        catch (Exception exception)
        {
            _logger.Warning(exception, "OAuth registration identity store is unavailable");
            return new GetOAuthRegistrationResult.Unavailable();
        }
    }

    public async Task<TakeOAuthRegistrationResult> TakeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidCode(code))
        {
            return new TakeOAuthRegistrationResult.Missing();
        }

        try
        {
            var record = await _cacheManager.Take<OAuthRegistrationRecord>(
                $"{KeyPrefix}{code}",
                cancellationToken).ConfigureAwait(false);
            return IsValidRecord(record, code)
                ? new TakeOAuthRegistrationResult.Found(record!)
                : new TakeOAuthRegistrationResult.Missing();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (JsonException exception)
        {
            _logger.Warning(exception, "OAuth registration identity could not be deserialized");
            return new TakeOAuthRegistrationResult.Missing();
        }
        catch (Exception exception)
        {
            _logger.Warning(exception, "OAuth registration identity store is unavailable");
            return new TakeOAuthRegistrationResult.Unavailable();
        }
    }

    public async Task RestoreAsync(
        OAuthRegistrationRecord record,
        CancellationToken cancellationToken = default)
    {
        var remaining = record.ExpiresAt - _timeProvider.GetUtcNow();
        if (remaining <= TimeSpan.Zero)
        {
            return;
        }

        try
        {
            await _cacheManager.Set(
                $"{KeyPrefix}{record.Code}",
                record,
                remaining,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.Warning(exception, "OAuth registration identity could not be restored");
        }
    }

    private bool IsValidRecord(OAuthRegistrationRecord? record, string code)
    {
        var now = _timeProvider.GetUtcNow();
        return record is not null &&
               record.Code == code &&
               record.Platform is not AppPlatformEnum.Void &&
               !string.IsNullOrWhiteSpace(record.ProviderUserId) &&
               !string.IsNullOrWhiteSpace(record.AccessToken) &&
               record.IssuedAt <= now &&
               record.ExpiresAt > now &&
               record.ExpiresAt > record.IssuedAt;
    }

    private static bool IsValidCode(string code)
    {
        return !string.IsNullOrEmpty(code) &&
               code.Length == EncodedCodeLength &&
               code.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_');
    }
}

public record OAuthRegistrationIdentity
{
    public required AppPlatformEnum Platform { get; init; }

    public required string ProviderUserId { get; init; }

    public string ProviderUserName { get; init; } = string.Empty;

    public required string AccessToken { get; init; }

    public DateTimeOffset? AccessTokenExpiresAt { get; init; }

    public string RefreshToken { get; init; } = string.Empty;

    public DateTimeOffset? RefreshTokenExpiresAt { get; init; }

    public string ExtraData { get; init; } = string.Empty;
}

public sealed record OAuthRegistrationRecord : OAuthRegistrationIdentity
{
    public required string Code { get; init; }

    public required DateTimeOffset IssuedAt { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }
}

public abstract record CreateOAuthRegistrationResult
{
    private CreateOAuthRegistrationResult() { }

    public sealed record Created(string Code) : CreateOAuthRegistrationResult;

    public sealed record Rejected : CreateOAuthRegistrationResult;

    public sealed record Unavailable : CreateOAuthRegistrationResult;
}

public abstract record GetOAuthRegistrationResult
{
    private GetOAuthRegistrationResult() { }

    public sealed record Found(OAuthRegistrationRecord Record) : GetOAuthRegistrationResult;

    public sealed record Missing : GetOAuthRegistrationResult;

    public sealed record Unavailable : GetOAuthRegistrationResult;
}

public abstract record TakeOAuthRegistrationResult
{
    private TakeOAuthRegistrationResult() { }

    public sealed record Found(OAuthRegistrationRecord Record) : TakeOAuthRegistrationResult;

    public sealed record Missing : TakeOAuthRegistrationResult;

    public sealed record Unavailable : TakeOAuthRegistrationResult;
}
