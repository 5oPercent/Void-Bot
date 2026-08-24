using System.Security.Cryptography;
using System.Text;
using Serilog;
using Void.Bot.Core.Services;
using Void.Bot.Core.Services.Email;
using Void.Bot.Core.Settings.Email;

namespace Void.Bot.Core.Caching;

public interface IEmailAuthorizationStateStore : IScope
{
    Task<EmailAuthorizationStateRecord> CreateAsync(
        bool forceReauthorization,
        CancellationToken cancellationToken);

    Task<EmailAuthorizationStateRecord?> TakeAsync(
        string state,
        CancellationToken cancellationToken);
}

public sealed class EmailAuthorizationStateStore(
    ICacheManager cacheManager,
    EmailSetting setting,
    ILogger logger) : IEmailAuthorizationStateStore
{
    private const string KeyPrefix = "email:oauth:state:";
    private const int StateByteLength = 32;
    private const int CodeVerifierByteLength = 64;

    public async Task<EmailAuthorizationStateRecord> CreateAsync(
        bool forceReauthorization,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var record = new EmailAuthorizationStateRecord
        {
            State = Base64UrlEncode(RandomNumberGenerator.GetBytes(StateByteLength)),
            Nonce = Base64UrlEncode(RandomNumberGenerator.GetBytes(StateByteLength)),
            CodeVerifier = Base64UrlEncode(RandomNumberGenerator.GetBytes(CodeVerifierByteLength)),
            ForceReauthorization = forceReauthorization,
            IssuedAt = now,
            ExpiresAt = now.AddSeconds(setting.StateTtlSeconds)
        };

        try
        {
            await cacheManager.Set(
                $"{KeyPrefix}{record.State}",
                record,
                TimeSpan.FromSeconds(setting.StateTtlSeconds),
                cancellationToken).ConfigureAwait(false);
            return record;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.Warning(exception, "Unable to persist email authorization state");
            throw new EmailServiceException("Email authorization is temporarily unavailable.", exception);
        }
    }

    public async Task<EmailAuthorizationStateRecord?> TakeAsync(
        string state,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(state) || state.Length != 43 ||
            state.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '-' and not '_'))
        {
            return null;
        }

        try
        {
            var record = await cacheManager.Take<EmailAuthorizationStateRecord>(
                $"{KeyPrefix}{state}", cancellationToken).ConfigureAwait(false);
            var now = DateTimeOffset.UtcNow;
            return record is not null &&
                   record.State == state &&
                   record.IssuedAt <= now &&
                   record.ExpiresAt > now
                ? record
                : null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.Warning(exception, "Unable to consume email authorization state");
            throw new EmailServiceException("Email authorization is temporarily unavailable.", exception);
        }
    }

    public static string CreateCodeChallenge(string codeVerifier)
    {
        return Base64UrlEncode(SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier)));
    }

    private static string Base64UrlEncode(byte[] value)
    {
        return Convert.ToBase64String(value)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}

public sealed class EmailAuthorizationStateRecord
{
    public string State { get; init; } = string.Empty;

    public string CodeVerifier { get; init; } = string.Empty;

    public string Nonce { get; init; } = string.Empty;

    public bool ForceReauthorization { get; init; }

    public DateTimeOffset IssuedAt { get; init; }

    public DateTimeOffset ExpiresAt { get; init; }
}
