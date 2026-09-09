using System.Net;
using Serilog;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Security;
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

    Task<TwoFactorLoginVerificationResult> VerifyLoginAsync(
        Guid userId, string? code, CancellationToken cancellationToken);
}

public sealed class AccountTwoFactorService(
    IAccountDataProvider dataProvider,
    ITotpService totpService,
    ISecretProtector secretProtector,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    ILogger logger) : IAccountTwoFactorService
{
    private const string Issuer = "Void Bot";

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
                EnabledAt = account.TwoFactorEnabledAt
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
        account.TwoFactorSecret = secretProtector.Protect(secret, GetProtectionPurpose(account.Id));
        await dataProvider.UpdateUserAccountAsync(account, cancellationToken).ConfigureAwait(false);

        logger.Information("Two-factor setup started for account {UserId}", account.Id);
        return new BeginTwoFactorSetupResponse
        {
            Data = new TwoFactorSetupDto
            {
                ManualEntryKey = secret,
                OtpAuthUri = totpService.CreateOtpAuthUri(Issuer, account.DisplayName, secret)
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

        if (string.IsNullOrWhiteSpace(account.TwoFactorSecret))
        {
            return Error<EnableTwoFactorResponse>(HttpStatusCode.BadRequest, "two_factor_setup_required");
        }

        if (!VerifyCode(account.Id, account.TwoFactorSecret, command.Code))
        {
            return Error<EnableTwoFactorResponse>(HttpStatusCode.Unauthorized, "invalid_two_factor_code");
        }

        account.TwoFactorEnabledAt = timeProvider.GetUtcNow();
        account.TokenVersion++;
        await dataProvider.UpdateUserAccountAsync(account, cancellationToken).ConfigureAwait(false);
        logger.Information("Two-factor authentication enabled for account {UserId}", account.Id);
        return new EnableTwoFactorResponse();
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

        if (!VerifyCode(account.Id, account.TwoFactorSecret, command.Code))
        {
            return Error<DisableTwoFactorResponse>(HttpStatusCode.Unauthorized, "invalid_two_factor_code");
        }

        account.TwoFactorSecret = null;
        account.TwoFactorEnabledAt = null;
        account.TokenVersion++;
        await dataProvider.UpdateUserAccountAsync(account, cancellationToken).ConfigureAwait(false);
        logger.Information("Two-factor authentication disabled for account {UserId}", account.Id);
        return new DisableTwoFactorResponse();
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

        return !string.IsNullOrWhiteSpace(account.TwoFactorSecret) &&
               VerifyCode(account.Id, account.TwoFactorSecret, code)
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

    private static string GetProtectionPurpose(Guid userId)
    {
        return $"account-two-factor:{userId:N}";
    }

    private static TResponse Error<TResponse>(HttpStatusCode code, string message)
        where TResponse : VoidBotResponse, new()
    {
        return new TResponse { Code = code, Message = message };
    }
}
