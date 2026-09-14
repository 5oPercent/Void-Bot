using Serilog;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Domain.Email;
using Void.Bot.Core.Services.Security;
using Void.Bot.Core.Settings.Email;
using Void.Bot.Message.Commands.Email;
using Void.Bot.Message.Dto.Email;
using Void.Bot.Message.Enum.Email;
using Void.Bot.Message.Request.Email;

namespace Void.Bot.Core.Services.Email;

internal interface IEmailCredentialManager : IScope
{
    Task<AuthorizeEmailResponse> AuthorizeAsync(
        AuthorizeEmailCommand command,
        CancellationToken cancellationToken);

    Task<CompleteEmailAuthorizationResponse> CompleteAuthorizationAsync(
        CompleteEmailAuthorizationCommand command,
        CancellationToken cancellationToken);

    Task<RefreshEmailCredentialResponse> RefreshCredentialAsync(
        RefreshEmailCredentialCommand command,
        CancellationToken cancellationToken);

    Task<GetEmailServiceStatusResponse> GetStatusAsync(
        GetEmailServiceStatusRequest request,
        CancellationToken cancellationToken);

    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken);

    Task<string> RefreshAccessTokenAsync(CancellationToken cancellationToken);
}

internal sealed class EmailCredentialManager(
    IEmailAuthorizationStateStore stateStore,
    IMicrosoftGraphClient graphClient,
    IEmailDataProvider dataProvider,
    ISecretProtector secretProtector,
    ICacheManager cacheManager,
    EmailSetting setting,
    ILogger logger) : IEmailCredentialManager
{
    private const string RefreshLockKey = "email:credential:refresh:lock";
    private const string AccessTokenProtectionPurpose = "email-oauth-access-token";
    private const string RefreshTokenProtectionPurpose = "email-oauth-refresh-token";
    private static readonly TimeSpan AccessTokenRefreshWindow = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan RefreshLockDuration = TimeSpan.FromMinutes(2);

    public async Task<AuthorizeEmailResponse> AuthorizeAsync(
        AuthorizeEmailCommand command,
        CancellationToken cancellationToken)
    {
        var state = await stateStore.CreateAsync(
            command.ForceReauthorization, cancellationToken).ConfigureAwait(false);
        return new AuthorizeEmailResponse { Data = graphClient.CreateAuthorizationUrl(state) };
    }

    public async Task<CompleteEmailAuthorizationResponse> CompleteAuthorizationAsync(
        CompleteEmailAuthorizationCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            await ProcessCallbackAsync(command, cancellationToken).ConfigureAwait(false);
            return new CompleteEmailAuthorizationResponse
            {
                Message = "Email authorization completed."
            };
        }
        catch (EmailReauthorizationRequiredException exception)
        {
            logger.Warning(
                exception,
                "Email authorization requires reauthorization: {ErrorCode}",
                exception.ProviderErrorCode ?? "reauthorization_required");
            return new CompleteEmailAuthorizationResponse
            {
                Code = System.Net.HttpStatusCode.Unauthorized,
                Message = exception.ProviderErrorCode ?? "reauthorization_required"
            };
        }
        catch (EmailProviderUnavailableException exception)
        {
            logger.Warning(
                exception,
                "Email authorization provider unavailable: {ErrorCode}",
                exception.ProviderErrorCode ?? "email_provider_unavailable");
            return new CompleteEmailAuthorizationResponse
            {
                Code = System.Net.HttpStatusCode.BadGateway,
                Message = exception.ProviderErrorCode ?? "email_provider_unavailable"
            };
        }
        catch (EmailServiceException exception)
        {
            logger.Warning(
                exception,
                "Email authorization failed: {ErrorCode}, provider status: {ProviderStatusCode}",
                exception.ProviderErrorCode ?? "authorization_failed",
                exception.ProviderStatusCode);
            return new CompleteEmailAuthorizationResponse
            {
                Code = exception.ProviderStatusCode.HasValue
                    ? System.Net.HttpStatusCode.BadGateway
                    : System.Net.HttpStatusCode.BadRequest,
                Message = exception.ProviderErrorCode ?? "authorization_failed"
            };
        }
    }

    private async Task ProcessCallbackAsync(
        CompleteEmailAuthorizationCommand command,
        CancellationToken cancellationToken)
    {
        var stateRecord = await stateStore.TakeAsync(
            command.State ?? string.Empty, cancellationToken).ConfigureAwait(false);
        if (stateRecord is null)
        {
            throw new EmailServiceException("Email authorization state is invalid or expired.");
        }

        if (!string.IsNullOrWhiteSpace(command.Error))
        {
            throw new EmailServiceException(
                "Email authorization was denied by the provider.",
                providerErrorCode: command.Error);
        }

        if (string.IsNullOrWhiteSpace(command.Code))
        {
            throw new EmailServiceException("The email provider did not return an authorization code.");
        }

        var token = await graphClient.ExchangeAuthorizationCodeAsync(
            command.Code, stateRecord.CodeVerifier, cancellationToken).ConfigureAwait(false);
        var identity = await graphClient.ValidateIdentityAsync(
            token.IdToken, stateRecord.Nonce, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(setting.ExpectedSenderEmail) &&
            !string.Equals(identity.Email, setting.ExpectedSenderEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new EmailServiceException("The authorized account does not match the configured sender email.");
        }

        var now = DateTimeOffset.UtcNow;
        var credential = await dataProvider.GetCredentialAsync(
            true, cancellationToken).ConfigureAwait(false) ?? new EmailCredential();
        credential.SenderEmail = identity.Email;
        credential.ExternalAccountId = identity.AccountId;
        credential.EncryptedAccessToken = secretProtector.Protect(
            token.AccessToken, AccessTokenProtectionPurpose);
        credential.AccessTokenExpiresAt = token.AccessTokenExpiresAt;
        credential.EncryptedRefreshToken = secretProtector.Protect(
            token.RefreshToken, RefreshTokenProtectionPurpose);
        credential.RefreshTokenLastUsedAt = now;
        credential.NextRefreshAt = now.AddDays(setting.TokenRefreshIntervalDays);
        credential.Status = EmailCredentialStatusEnum.Active;
        credential.LastError = null;
        credential.LastErrorAt = null;

        await dataProvider.SaveCredentialAsync(credential, cancellationToken).ConfigureAwait(false);
        logger.Information("Email sender authorization completed for {SenderEmail}", MaskEmail(identity.Email));
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var credential = await dataProvider.GetCredentialAsync(
            false, cancellationToken).ConfigureAwait(false);
        if (credential is null || credential.Status is EmailCredentialStatusEnum.Disabled or
            EmailCredentialStatusEnum.ReauthorizationRequired)
        {
            throw new EmailReauthorizationRequiredException();
        }

        if (credential.AccessTokenExpiresAt > DateTimeOffset.UtcNow.Add(AccessTokenRefreshWindow))
        {
            return secretProtector.Unprotect(
                credential.EncryptedAccessToken, AccessTokenProtectionPurpose);
        }

        await RefreshCredentialInternalAsync(true, false, cancellationToken).ConfigureAwait(false);
        credential = await dataProvider.GetCredentialAsync(false, cancellationToken).ConfigureAwait(false);
        if (credential is null || credential.Status != EmailCredentialStatusEnum.Active)
        {
            throw new EmailReauthorizationRequiredException();
        }

        return secretProtector.Unprotect(
            credential.EncryptedAccessToken, AccessTokenProtectionPurpose);
    }

    public async Task<string> RefreshAccessTokenAsync(CancellationToken cancellationToken)
    {
        await RefreshCredentialInternalAsync(true, true, cancellationToken).ConfigureAwait(false);
        var credential = await dataProvider.GetCredentialAsync(false, cancellationToken).ConfigureAwait(false);
        if (credential is null || credential.Status != EmailCredentialStatusEnum.Active)
        {
            throw new EmailReauthorizationRequiredException();
        }

        return secretProtector.Unprotect(
            credential.EncryptedAccessToken, AccessTokenProtectionPurpose);
    }

    public async Task<RefreshEmailCredentialResponse> RefreshCredentialAsync(
        RefreshEmailCredentialCommand command,
        CancellationToken cancellationToken)
    {
        var credential = await dataProvider.GetCredentialAsync(
            false, cancellationToken).ConfigureAwait(false);
        if (credential is null || credential.Status is EmailCredentialStatusEnum.Disabled or
            EmailCredentialStatusEnum.ReauthorizationRequired ||
            credential.NextRefreshAt > DateTimeOffset.UtcNow)
        {
            return new RefreshEmailCredentialResponse();
        }

        await RefreshCredentialInternalAsync(false, false, cancellationToken).ConfigureAwait(false);
        return new RefreshEmailCredentialResponse();
    }

    public async Task<GetEmailServiceStatusResponse> GetStatusAsync(
        GetEmailServiceStatusRequest request,
        CancellationToken cancellationToken)
    {
        var credential = await dataProvider.GetCredentialAsync(
            false, cancellationToken).ConfigureAwait(false);
        if (credential is null)
        {
            return new GetEmailServiceStatusResponse { Data = new EmailServiceStatusDto() };
        }

        return new GetEmailServiceStatusResponse
        {
            Data = new EmailServiceStatusDto
            {
                IsAuthorized = credential.Status == EmailCredentialStatusEnum.Active,
                SenderEmail = MaskEmail(credential.SenderEmail),
                Status = credential.Status.ToString(),
                AccessTokenExpiresAt = credential.AccessTokenExpiresAt,
                RefreshTokenLastUsedAt = credential.RefreshTokenLastUsedAt,
                NextRefreshAt = credential.NextRefreshAt,
                LastError = credential.LastError,
                LastErrorAt = credential.LastErrorAt
            }
        };
    }

    private async Task RefreshCredentialInternalAsync(
        bool refreshExpiredAccessToken,
        bool forceRefresh,
        CancellationToken cancellationToken)
    {
        if (!await AcquireRefreshLockAsync(cancellationToken).ConfigureAwait(false))
        {
            throw new EmailServiceException("Email credential refresh is already in progress.");
        }

        try
        {
            var credential = await dataProvider.GetCredentialAsync(
                true, cancellationToken).ConfigureAwait(false);
            if (credential is null)
            {
                logger.Error("Email credential is missing and requires reauthorization");
                throw new EmailReauthorizationRequiredException();
            }

            var now = DateTimeOffset.UtcNow;
            if (!forceRefresh && refreshExpiredAccessToken &&
                credential.AccessTokenExpiresAt > now.Add(AccessTokenRefreshWindow))
            {
                return;
            }

            if (!forceRefresh && !refreshExpiredAccessToken && credential.NextRefreshAt > now)
            {
                return;
            }

            try
            {
                var refreshToken = secretProtector.Unprotect(
                    credential.EncryptedRefreshToken, RefreshTokenProtectionPurpose);
                var token = await graphClient.RefreshTokenAsync(
                    refreshToken, cancellationToken).ConfigureAwait(false);
                credential.EncryptedAccessToken = secretProtector.Protect(
                    token.AccessToken, AccessTokenProtectionPurpose);
                credential.AccessTokenExpiresAt = token.AccessTokenExpiresAt;
                credential.EncryptedRefreshToken = secretProtector.Protect(
                    token.RefreshToken, RefreshTokenProtectionPurpose);
                credential.RefreshTokenLastUsedAt = now;
                credential.NextRefreshAt = now.AddDays(setting.TokenRefreshIntervalDays);
                credential.Status = EmailCredentialStatusEnum.Active;
                credential.LastError = null;
                credential.LastErrorAt = null;
                await dataProvider.SaveCredentialAsync(credential, cancellationToken).ConfigureAwait(false);
                logger.Information("Email credential refreshed for {SenderEmail}", MaskEmail(credential.SenderEmail));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                credential.Status = exception is EmailReauthorizationRequiredException
                    ? EmailCredentialStatusEnum.ReauthorizationRequired
                    : EmailCredentialStatusEnum.RefreshFailed;
                credential.LastError = exception is EmailServiceException emailException
                    ? emailException.ProviderErrorCode ?? emailException.Message
                    : "Credential refresh failed.";
                credential.LastErrorAt = now;
                await dataProvider.SaveCredentialAsync(credential, cancellationToken).ConfigureAwait(false);
                if (exception is EmailReauthorizationRequiredException reauthorizationException)
                {
                    logger.Error(
                        exception,
                        "Email credential requires reauthorization for {SenderEmail}: {ErrorCode}",
                        MaskEmail(credential.SenderEmail),
                        reauthorizationException.ProviderErrorCode);
                }
                else
                {
                    logger.Warning(
                        exception,
                        "Email credential refresh failed for {SenderEmail}: {ErrorCode}",
                        MaskEmail(credential.SenderEmail),
                        exception is EmailServiceException providerException
                            ? providerException.ProviderErrorCode
                            : exception.GetType().Name);
                }
                throw;
            }
        }
        finally
        {
            await cacheManager.Remove(RefreshLockKey, CancellationToken.None).ConfigureAwait(false);
        }
    }

    private async Task<bool> AcquireRefreshLockAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            if (await cacheManager.TryLockAsync(
                    RefreshLockKey, RefreshLockDuration, cancellationToken).ConfigureAwait(false))
            {
                return true;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken).ConfigureAwait(false);
        }

        return false;
    }

    private static string MaskEmail(string email)
    {
        var separator = email.IndexOf('@');
        if (separator <= 0)
        {
            return "***";
        }

        return $"{email[0]}***{email[separator..]}";
    }
}
