using System.Security.Cryptography;
using System.Text.Json;
using Serilog;
using Void.Bot.Core.Services;
using Void.Bot.Message.Dto.Identity;

namespace Void.Bot.Core.Caching;

public interface IOAuthExchangeStore : IScope
{
    Task<CreateOAuthExchangeResult> CreateAsync(
        JwtToken token,
        TimeSpan timeToLive,
        CancellationToken cancellationToken = default);

    Task<TakeOAuthExchangeResult> TakeAsync(
        string code,
        CancellationToken cancellationToken = default);
}

public sealed class OAuthExchangeStore : IOAuthExchangeStore
{
    private const int CodeByteLength = 32;
    private const int EncodedCodeLength = 43;
    private const string KeyPrefix = "oauth:exchange:";
    private readonly ICacheManager _cacheManager;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger _logger;

    public OAuthExchangeStore(ICacheManager cacheManager, TimeProvider timeProvider, ILogger logger)
    {
        _cacheManager = cacheManager;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<CreateOAuthExchangeResult> CreateAsync(
        JwtToken token,
        TimeSpan timeToLive,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token.AccessToken) ||
            string.IsNullOrWhiteSpace(token.RefreshToken) ||
            timeToLive <= TimeSpan.Zero)
        {
            return new CreateOAuthExchangeResult.Rejected();
        }

        var issuedAt = _timeProvider.GetUtcNow();
        var code = Convert.ToBase64String(RandomNumberGenerator.GetBytes(CodeByteLength))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        var record = new OAuthExchangeRecord
        {
            Code = code,
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
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
            _logger.Information("OAuth exchange code stored until {ExpiresAt}", record.ExpiresAt);
            return new CreateOAuthExchangeResult.Created(code);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.Warning(exception, "OAuth exchange code creation failed because the store is unavailable");
            return new CreateOAuthExchangeResult.Unavailable();
        }
    }

    public async Task<TakeOAuthExchangeResult> TakeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(code) ||
            code.Length != EncodedCodeLength ||
            code.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '-' and not '_'))
        {
            _logger.Warning("OAuth exchange code consumption rejected malformed input");
            return new TakeOAuthExchangeResult.Missing();
        }

        try
        {
            var record = await _cacheManager.Take<OAuthExchangeRecord>(
                $"{KeyPrefix}{code}",
                cancellationToken).ConfigureAwait(false);
            if (record is null)
            {
                _logger.Warning("OAuth exchange code was missing, expired, or already consumed");
                return new TakeOAuthExchangeResult.Missing();
            }

            var now = _timeProvider.GetUtcNow();
            if (record.Code != code ||
                string.IsNullOrWhiteSpace(record.AccessToken) ||
                string.IsNullOrWhiteSpace(record.RefreshToken) ||
                record.IssuedAt > now ||
                record.ExpiresAt <= now ||
                record.ExpiresAt <= record.IssuedAt)
            {
                _logger.Warning("OAuth exchange record failed validation");
                return new TakeOAuthExchangeResult.Missing();
            }

            _logger.Information("OAuth exchange code consumed");
            return new TakeOAuthExchangeResult.Found(
                new JwtToken
                {
                    AccessToken = record.AccessToken,
                    RefreshToken = record.RefreshToken
                });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (JsonException exception)
        {
            _logger.Warning(exception, "OAuth exchange record could not be deserialized");
            return new TakeOAuthExchangeResult.Missing();
        }
        catch (Exception exception)
        {
            _logger.Warning(exception, "OAuth exchange code consumption failed because the store is unavailable");
            return new TakeOAuthExchangeResult.Unavailable();
        }
    }
}

public sealed record OAuthExchangeRecord
{
    public required string Code { get; init; }

    public required string AccessToken { get; init; }

    public required string RefreshToken { get; init; }

    public required DateTimeOffset IssuedAt { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }
}

public abstract record CreateOAuthExchangeResult
{
    private CreateOAuthExchangeResult() { }

    public sealed record Created(string Code) : CreateOAuthExchangeResult;

    public sealed record Rejected : CreateOAuthExchangeResult;

    public sealed record Unavailable : CreateOAuthExchangeResult;
}

public abstract record TakeOAuthExchangeResult
{
    private TakeOAuthExchangeResult() { }

    public sealed record Found(JwtToken Token) : TakeOAuthExchangeResult;

    public sealed record Missing : TakeOAuthExchangeResult;

    public sealed record Unavailable : TakeOAuthExchangeResult;
}
