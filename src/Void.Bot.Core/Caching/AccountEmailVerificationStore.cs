using System.Security.Cryptography;
using System.Text;
using Serilog;
using Void.Bot.Core.Services;
using Void.Bot.Core.Services.Security;
using Void.Bot.Core.Settings.Account;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Enum.Account;

namespace Void.Bot.Core.Caching;

public interface IAccountEmailVerificationStore : IScope
{
    Task<CreateEmailVerificationResult> CreateAsync(
        string email,
        EmailVerificationPurposeEnum purpose,
        Guid? userId,
        CancellationToken cancellationToken);

    Task<VerifyEmailVerificationResult> VerifyAndTakeAsync(
        EmailVerificationDto verification,
        EmailVerificationPurposeEnum purpose,
        Guid? userId,
        CancellationToken cancellationToken);

    Task RestoreAsync(
        AccountEmailVerificationRecord record,
        CancellationToken cancellationToken);

    Task RemoveAsync(Guid challengeId, CancellationToken cancellationToken);
}

public sealed class AccountEmailVerificationStore(
    ICacheManager cacheManager,
    ISecretProtector secretProtector,
    AccountEmailSetting setting,
    ILogger logger) : IAccountEmailVerificationStore
{
    private const string ChallengeKeyPrefix = "account:email:verification:";
    private const string ActiveChallengeKeyPrefix = "account:email:verification:active:";
    private const string RateKeyPrefix = "account:email:verification:rate:";
    private const string LockKeyPrefix = "account:email:verification:lock:";
    private static readonly TimeSpan LockDuration = TimeSpan.FromSeconds(10);

    public async Task<CreateEmailVerificationResult> CreateAsync(
        string email,
        EmailVerificationPurposeEnum purpose,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var emailKey = CreateEmailKey(email);
        var rateKey = $"{RateKeyPrefix}{purpose}:{emailKey}";
        var activeChallengeKey = $"{ActiveChallengeKeyPrefix}{purpose}:{emailKey}";
        var lockKey = $"{LockKeyPrefix}create:{purpose}:{emailKey}";
        if (!await cacheManager.TryLockAsync(lockKey, LockDuration, cancellationToken).ConfigureAwait(false))
        {
            logger.Warning(
                "Account email verification challenge creation was rate limited for {Purpose} and email key {EmailKey}",
                purpose,
                emailKey);
            return new CreateEmailVerificationResult.RateLimited();
        }

        try
        {
            var now = DateTimeOffset.UtcNow;
            var rate = await cacheManager.Get<AccountEmailVerificationRateRecord>(
                rateKey, cancellationToken).ConfigureAwait(false);
            if (rate is not null && now - rate.LastSentAt < TimeSpan.FromSeconds(setting.ResendIntervalSeconds))
            {
                logger.Warning(
                    "Account email verification resend interval was not reached for {Purpose} and email key {EmailKey}",
                    purpose,
                    emailKey);
                return new CreateEmailVerificationResult.RateLimited();
            }

            if (rate is null || now - rate.WindowStartedAt >= TimeSpan.FromHours(1))
            {
                rate = new AccountEmailVerificationRateRecord
                {
                    WindowStartedAt = now,
                    LastSentAt = now,
                    SendCount = 0
                };
            }

            if (rate.SendCount >= setting.MaxSendsPerHour)
            {
                logger.Warning(
                    "Account email verification hourly limit was reached for {Purpose} and email key {EmailKey}",
                    purpose,
                    emailKey);
                return new CreateEmailVerificationResult.RateLimited();
            }

            var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            var expiresAt = now.AddMinutes(setting.CodeTtlMinutes);
            var record = new AccountEmailVerificationRecord
            {
                ChallengeId = Guid.NewGuid(),
                Email = email,
                Purpose = purpose,
                UserId = userId,
                ProtectedCode = secretProtector.Protect(code, GetProtectionPurpose(purpose)),
                IssuedAt = now,
                ExpiresAt = expiresAt
            };
            rate.SendCount++;
            rate.LastSentAt = now;

            var previousChallengeId = await cacheManager.Get<string>(
                activeChallengeKey, cancellationToken).ConfigureAwait(false);
            if (Guid.TryParse(previousChallengeId, out var previousId))
            {
                await cacheManager.Remove(
                    $"{ChallengeKeyPrefix}{previousId:N}", cancellationToken).ConfigureAwait(false);
            }

            await cacheManager.Set(
                $"{ChallengeKeyPrefix}{record.ChallengeId:N}",
                record,
                expiresAt - now,
                cancellationToken).ConfigureAwait(false);
            await cacheManager.Set(
                activeChallengeKey,
                record.ChallengeId.ToString(),
                expiresAt - now,
                cancellationToken).ConfigureAwait(false);
            await cacheManager.Set(
                rateKey,
                rate,
                rate.WindowStartedAt.AddHours(1) - now,
                cancellationToken).ConfigureAwait(false);
            logger.Information(
                "Account email verification challenge {ChallengeId} created for {Purpose}",
                record.ChallengeId,
                purpose);
            return new CreateEmailVerificationResult.Created(record, code);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.Warning(exception, "Account email verification store is unavailable while creating a challenge");
            return new CreateEmailVerificationResult.Unavailable();
        }
        finally
        {
            try
            {
                await cacheManager.Remove(lockKey, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                logger.Warning(exception, "Account email verification creation lock {LockKey} could not be released", lockKey);
            }
        }
    }

    public async Task<VerifyEmailVerificationResult> VerifyAndTakeAsync(
        EmailVerificationDto verification,
        EmailVerificationPurposeEnum purpose,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var challengeKey = $"{ChallengeKeyPrefix}{verification.ChallengeId:N}";
        var lockKey = $"{LockKeyPrefix}verify:{verification.ChallengeId:N}";
        if (!await cacheManager.TryLockAsync(lockKey, LockDuration, cancellationToken).ConfigureAwait(false))
        {
            logger.Warning(
                "Account email verification challenge {ChallengeId} is already being verified",
                verification.ChallengeId);
            return new VerifyEmailVerificationResult.Invalid();
        }

        try
        {
            var record = await cacheManager.Get<AccountEmailVerificationRecord>(
                challengeKey, cancellationToken).ConfigureAwait(false);
            var now = DateTimeOffset.UtcNow;
            if (record is null || record.ExpiresAt <= now || record.Purpose != purpose ||
                record.UserId != userId || !string.Equals(record.Email, verification.Email, StringComparison.Ordinal))
            {
                logger.Warning(
                    "Account email verification challenge {ChallengeId} did not match {Purpose}",
                    verification.ChallengeId,
                    purpose);
                return new VerifyEmailVerificationResult.Invalid();
            }

            var expectedCode = secretProtector.Unprotect(
                record.ProtectedCode, GetProtectionPurpose(record.Purpose));
            if (!FixedTimeEquals(expectedCode, verification.Code))
            {
                record.FailedAttempts++;
                if (record.FailedAttempts >= setting.MaxAttempts)
                {
                    await cacheManager.Remove(challengeKey, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await cacheManager.Set(
                        challengeKey,
                        record,
                        record.ExpiresAt - now,
                        cancellationToken).ConfigureAwait(false);
                }

                logger.Warning(
                    "Account email verification challenge {ChallengeId} failed code validation on attempt {Attempt}",
                    verification.ChallengeId,
                    record.FailedAttempts);
                return new VerifyEmailVerificationResult.Invalid();
            }

            var consumed = await cacheManager.Take<AccountEmailVerificationRecord>(
                challengeKey, cancellationToken).ConfigureAwait(false);
            if (consumed is not null)
            {
                logger.Information(
                    "Account email verification challenge {ChallengeId} was consumed for {Purpose}",
                    verification.ChallengeId,
                    purpose);
            }

            return consumed is null
                ? new VerifyEmailVerificationResult.Invalid()
                : new VerifyEmailVerificationResult.Verified(consumed);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.Warning(exception, "Account email verification store is unavailable while verifying a challenge");
            return new VerifyEmailVerificationResult.Unavailable();
        }
        finally
        {
            try
            {
                await cacheManager.Remove(lockKey, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                logger.Warning(exception, "Account email verification lock {LockKey} could not be released", lockKey);
            }
        }
    }

    public async Task RestoreAsync(
        AccountEmailVerificationRecord record,
        CancellationToken cancellationToken)
    {
        try
        {
            var remaining = record.ExpiresAt - DateTimeOffset.UtcNow;
            if (remaining <= TimeSpan.Zero)
            {
                return;
            }

            await cacheManager.Set(
                $"{ChallengeKeyPrefix}{record.ChallengeId:N}",
                record,
                remaining,
                cancellationToken).ConfigureAwait(false);
            logger.Information(
                "Account email verification challenge {ChallengeId} was restored after persistence failure",
                record.ChallengeId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.Error(
                exception,
                "Account email verification challenge {ChallengeId} could not be restored",
                record.ChallengeId);
        }
    }

    public async Task RemoveAsync(Guid challengeId, CancellationToken cancellationToken)
    {
        try
        {
            await cacheManager.Remove(
                $"{ChallengeKeyPrefix}{challengeId:N}", cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.Error(
                exception,
                "Account email verification challenge {ChallengeId} could not be removed",
                challengeId);
        }
    }

    private static string CreateEmailKey(string email)
    {
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(email)));
    }

    private static string GetProtectionPurpose(EmailVerificationPurposeEnum purpose)
    {
        return $"account-email-verification-{purpose.ToString().ToLowerInvariant()}";
    }

    private static bool FixedTimeEquals(string expected, string actual)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var actualBytes = Encoding.UTF8.GetBytes(actual);
        return expectedBytes.Length == actualBytes.Length &&
               CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }
}

public sealed class AccountEmailVerificationRecord
{
    public Guid ChallengeId { get; init; }

    public required string Email { get; init; }

    public EmailVerificationPurposeEnum Purpose { get; init; }

    public Guid? UserId { get; init; }

    public required string ProtectedCode { get; init; }

    public int FailedAttempts { get; set; }

    public DateTimeOffset IssuedAt { get; init; }

    public DateTimeOffset ExpiresAt { get; init; }
}

public sealed class AccountEmailVerificationRateRecord
{
    public int SendCount { get; set; }

    public DateTimeOffset WindowStartedAt { get; init; }

    public DateTimeOffset LastSentAt { get; set; }
}

public abstract record CreateEmailVerificationResult
{
    public sealed record Created(AccountEmailVerificationRecord Record, string Code)
        : CreateEmailVerificationResult;

    public sealed record RateLimited : CreateEmailVerificationResult;

    public sealed record Unavailable : CreateEmailVerificationResult;
}

public abstract record VerifyEmailVerificationResult
{
    public sealed record Verified(AccountEmailVerificationRecord Record)
        : VerifyEmailVerificationResult;

    public sealed record Invalid : VerifyEmailVerificationResult;

    public sealed record Unavailable : VerifyEmailVerificationResult;
}
