using System.Net;
using System.Security.Cryptography;
using System.Text;
using Serilog;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Security;
using Void.Bot.Core.Settings.Account;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Enum.Account;
using Void.Bot.Message.Request.Account;

namespace Void.Bot.Core.Services.Account;

public interface IAccountTwoFactorService : IScope
{
    Task<GetTwoFactorStatusResponse> GetStatusAsync(CancellationToken cancellationToken);

    Task<BeginTwoFactorSetupResponse> BeginSetupAsync(
        BeginTwoFactorSetupCommand command, CancellationToken cancellationToken);

    Task<EnableTwoFactorResponse> EnableAsync(
        EnableTwoFactorCommand command, CancellationToken cancellationToken);

    Task<DisableTwoFactorResponse> DisableAsync(
        DisableTwoFactorCommand command, CancellationToken cancellationToken);

    Task<RegenerateTwoFactorRecoveryCodesResponse> RegenerateRecoveryCodesAsync(
        RegenerateTwoFactorRecoveryCodesCommand command, CancellationToken cancellationToken);

    Task<TwoFactorLoginVerificationResult> VerifyLoginAsync(
        Guid userId, string? code, CancellationToken cancellationToken);
}

public sealed class AccountTwoFactorService(
    IAccountDataProvider dataProvider,
    ITotpService totpService,
    ISecretProtector secretProtector,
    TwoFactorSetting twoFactorSetting,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    ILogger logger) : IAccountTwoFactorService
{
    private const string RecoveryCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int RecoveryCodeLength = 12;

    public async Task<GetTwoFactorStatusResponse> GetStatusAsync(CancellationToken cancellationToken)
    {
        var account = await GetCurrentAccountAsync(cancellationToken).ConfigureAwait(false);
        if (account is null)
        {
            return Error<GetTwoFactorStatusResponse>(HttpStatusCode.NotFound, "account_not_found");
        }

        return new GetTwoFactorStatusResponse
        {
            Data = new TwoFactorStatusDto
            {
                IsEnabled = account.TwoFactorEnabledAt.HasValue,
                EnabledAt = account.TwoFactorEnabledAt,
                RecoveryCodesRemaining = account.TwoFactorEnabledAt.HasValue
                    ? await dataProvider.GetTwoFactorRecoveryCodeCountAsync(
                        account.Id, cancellationToken).ConfigureAwait(false)
                    : 0
            }
        };
    }

    public async Task<BeginTwoFactorSetupResponse> BeginSetupAsync(
        BeginTwoFactorSetupCommand command, CancellationToken cancellationToken)
    {
        var account = await GetCurrentAccountAsync(cancellationToken).ConfigureAwait(false);
        if (account is null)
        {
            return Error<BeginTwoFactorSetupResponse>(HttpStatusCode.NotFound, "account_not_found");
        }

        if (account.TwoFactorEnabledAt.HasValue)
        {
            return Error<BeginTwoFactorSetupResponse>(HttpStatusCode.Conflict, "two_factor_already_enabled");
        }

        if (account.Password != command.Password.SHAEncrypt())
        {
            return Error<BeginTwoFactorSetupResponse>(HttpStatusCode.Unauthorized, "invalid_password");
        }

        var secret = totpService.GenerateSecret();
        var now = timeProvider.GetUtcNow();
        account.TwoFactorSecret = secretProtector.Protect(secret, GetProtectionPurpose(account.Id));
        account.TwoFactorSetupAt = now;
        await dataProvider.UpdateUserAccountAsync(account, cancellationToken).ConfigureAwait(false);

        logger.Information("Two-factor setup started for account {UserId}", account.Id);
        return new BeginTwoFactorSetupResponse
        {
            Data = new TwoFactorSetupDto
            {
                ManualEntryKey = secret,
                OtpAuthUri = totpService.CreateOtpAuthUri(twoFactorSetting.Title, account.DisplayName, secret),
                ExpiresAt = now.AddMinutes(twoFactorSetting.SetupExpireMinutes)
            }
        };
    }

    public async Task<EnableTwoFactorResponse> EnableAsync(
        EnableTwoFactorCommand command, CancellationToken cancellationToken)
    {
        var account = await GetCurrentAccountAsync(cancellationToken).ConfigureAwait(false);
        if (account is null)
        {
            return Error<EnableTwoFactorResponse>(HttpStatusCode.NotFound, "account_not_found");
        }

        if (account.TwoFactorEnabledAt.HasValue)
        {
            return Error<EnableTwoFactorResponse>(HttpStatusCode.Conflict, "two_factor_already_enabled");
        }

        if (string.IsNullOrWhiteSpace(account.TwoFactorSecret) || !account.TwoFactorSetupAt.HasValue)
        {
            return Error<EnableTwoFactorResponse>(HttpStatusCode.BadRequest, "two_factor_setup_required");
        }

        var now = timeProvider.GetUtcNow();
        if (account.TwoFactorSetupAt.Value.AddMinutes(twoFactorSetting.SetupExpireMinutes) <= now)
        {
            account.TwoFactorSecret = null;
            account.TwoFactorSetupAt = null;
            await dataProvider.UpdateUserAccountAsync(account, cancellationToken).ConfigureAwait(false);
            return Error<EnableTwoFactorResponse>(HttpStatusCode.BadRequest, "two_factor_setup_expired");
        }

        if (!VerifyCode(account.Id, account.TwoFactorSecret, command.Code))
        {
            return Error<EnableTwoFactorResponse>(HttpStatusCode.Unauthorized, "invalid_two_factor_code");
        }

        var recoveryCodes = GenerateRecoveryCodes();
        account.TwoFactorSetupAt = null;
        account.TwoFactorEnabledAt = now;
        account.TokenVersion++;
        await dataProvider.UpdateUserAccountAsync(account, cancellationToken).ConfigureAwait(false);
        await dataProvider.ReplaceTwoFactorRecoveryCodesAsync(
            account.Id,
            recoveryCodes.Select(code => HashRecoveryCode(account.Id, code)).ToArray(),
            now,
            cancellationToken).ConfigureAwait(false);
        logger.Information("Two-factor authentication enabled for account {UserId}", account.Id);
        return new EnableTwoFactorResponse
        {
            Data = new TwoFactorRecoveryCodesDto { RecoveryCodes = recoveryCodes }
        };
    }

    public async Task<DisableTwoFactorResponse> DisableAsync(
        DisableTwoFactorCommand command, CancellationToken cancellationToken)
    {
        var account = await GetCurrentAccountAsync(cancellationToken).ConfigureAwait(false);
        if (account is null)
        {
            return Error<DisableTwoFactorResponse>(HttpStatusCode.NotFound, "account_not_found");
        }

        if (!account.TwoFactorEnabledAt.HasValue || string.IsNullOrWhiteSpace(account.TwoFactorSecret))
        {
            return Error<DisableTwoFactorResponse>(HttpStatusCode.Conflict, "two_factor_not_enabled");
        }

        if (account.Password != command.Password.SHAEncrypt())
        {
            return Error<DisableTwoFactorResponse>(HttpStatusCode.Unauthorized, "invalid_password");
        }

        if (!await VerifyAuthenticationCodeAsync(
                account, command.Code, cancellationToken).ConfigureAwait(false))
        {
            return Error<DisableTwoFactorResponse>(HttpStatusCode.Unauthorized, "invalid_two_factor_code");
        }

        account.TwoFactorSecret = null;
        account.TwoFactorSetupAt = null;
        account.TwoFactorEnabledAt = null;
        account.TokenVersion++;
        await dataProvider.UpdateUserAccountAsync(account, cancellationToken).ConfigureAwait(false);
        await dataProvider.DeleteTwoFactorRecoveryCodesAsync(
            account.Id, cancellationToken).ConfigureAwait(false);
        logger.Information("Two-factor authentication disabled for account {UserId}", account.Id);
        return new DisableTwoFactorResponse();
    }

    public async Task<RegenerateTwoFactorRecoveryCodesResponse> RegenerateRecoveryCodesAsync(
        RegenerateTwoFactorRecoveryCodesCommand command, CancellationToken cancellationToken)
    {
        var account = await GetCurrentAccountAsync(cancellationToken).ConfigureAwait(false);
        if (account is null)
        {
            return Error<RegenerateTwoFactorRecoveryCodesResponse>(
                HttpStatusCode.NotFound, "account_not_found");
        }

        if (!account.TwoFactorEnabledAt.HasValue || string.IsNullOrWhiteSpace(account.TwoFactorSecret))
        {
            return Error<RegenerateTwoFactorRecoveryCodesResponse>(
                HttpStatusCode.Conflict, "two_factor_not_enabled");
        }

        if (account.Password != command.Password.SHAEncrypt())
        {
            return Error<RegenerateTwoFactorRecoveryCodesResponse>(
                HttpStatusCode.Unauthorized, "invalid_password");
        }

        if (!await VerifyAuthenticationCodeAsync(
                account, command.Code, cancellationToken).ConfigureAwait(false))
        {
            return Error<RegenerateTwoFactorRecoveryCodesResponse>(
                HttpStatusCode.Unauthorized, "invalid_two_factor_code");
        }

        var recoveryCodes = GenerateRecoveryCodes();
        await dataProvider.ReplaceTwoFactorRecoveryCodesAsync(
            account.Id,
            recoveryCodes.Select(code => HashRecoveryCode(account.Id, code)).ToArray(),
            timeProvider.GetUtcNow(),
            cancellationToken).ConfigureAwait(false);
        logger.Information("Two-factor recovery codes regenerated for account {UserId}", account.Id);
        return new RegenerateTwoFactorRecoveryCodesResponse
        {
            Data = new TwoFactorRecoveryCodesDto { RecoveryCodes = recoveryCodes }
        };
    }

    public async Task<TwoFactorLoginVerificationResult> VerifyLoginAsync(
        Guid userId, string? code, CancellationToken cancellationToken)
    {
        var account = await dataProvider.GetUserAccountAsync(
            userId, string.Empty, cancellationToken).ConfigureAwait(false);
        if (account is null)
        {
            return TwoFactorLoginVerificationResult.InvalidCode;
        }

        if (account.TwoFactorEnabledAt is null)
        {
            return TwoFactorLoginVerificationResult.Disabled;
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return TwoFactorLoginVerificationResult.CodeRequired;
        }

        return await VerifyAuthenticationCodeAsync(
            account, code, cancellationToken).ConfigureAwait(false)
            ? TwoFactorLoginVerificationResult.Valid
            : TwoFactorLoginVerificationResult.InvalidCode;
    }

    private async Task<UserAccountDto?> GetCurrentAccountAsync(
        CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        return !userId.HasValue || userId == Guid.Empty
            ? null
            : await dataProvider.GetUserAccountAsync(
                userId, string.Empty, cancellationToken).ConfigureAwait(false);
    }

    private bool VerifyCode(Guid userId, string protectedSecret, string code)
    {
        var secret = secretProtector.Unprotect(protectedSecret, GetProtectionPurpose(userId));
        return totpService.VerifyCode(secret, code);
    }

    private async Task<bool> VerifyAuthenticationCodeAsync(
        UserAccountDto account, string code, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(account.TwoFactorSecret) &&
            VerifyCode(account.Id, account.TwoFactorSecret, code))
        {
            return true;
        }

        var normalizedRecoveryCode = NormalizeRecoveryCode(code);
        return normalizedRecoveryCode is not null &&
               await dataProvider.TryConsumeTwoFactorRecoveryCodeAsync(
                   account.Id,
                   HashRecoveryCode(account.Id, normalizedRecoveryCode),
                   cancellationToken).ConfigureAwait(false);
    }

    private static string GetProtectionPurpose(Guid userId)
    {
        return $"account-two-factor:{userId:N}";
    }

    private IReadOnlyList<string> GenerateRecoveryCodes()
    {
        var recoveryCodes = new List<string>(twoFactorSetting.RecoveryCodeCount);
        while (recoveryCodes.Count < twoFactorSetting.RecoveryCodeCount)
        {
            var randomBytes = RandomNumberGenerator.GetBytes(RecoveryCodeLength);
            try
            {
                var characters = randomBytes
                    .Select(value => RecoveryCodeAlphabet[value & 31])
                    .ToArray();
                var rawCode = new string(characters);
                var code = $"{rawCode[..4]}-{rawCode[4..8]}-{rawCode[8..]}";
                if (!recoveryCodes.Contains(code, StringComparer.Ordinal))
                {
                    recoveryCodes.Add(code);
                }
            }
            finally
            {
                CryptographicOperations.ZeroMemory(randomBytes);
            }
        }

        return recoveryCodes;
    }

    private static string? NormalizeRecoveryCode(string code)
    {
        var normalized = code.Trim().Replace("-", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
        return normalized.Length == RecoveryCodeLength &&
               normalized.All(character => RecoveryCodeAlphabet.Contains(character))
            ? normalized
            : null;
    }

    private static string HashRecoveryCode(Guid userId, string code)
    {
        var normalized = NormalizeRecoveryCode(code) ?? throw new ArgumentException(
            "Recovery code is invalid.", nameof(code));
        return Convert.ToHexStringLower(SHA256.HashData(
            Encoding.UTF8.GetBytes($"{userId:N}:{normalized}")));
    }

    private static TResponse Error<TResponse>(HttpStatusCode code, string message)
        where TResponse : VoidBotResponse, new()
    {
        return new TResponse { Code = code, Message = message };
    }
}
