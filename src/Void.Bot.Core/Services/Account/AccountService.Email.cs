using System.Net;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Resources.i18n.Email;
using Void.Bot.Core.Services.Email;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Dto.Email;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Account;
using Void.Bot.Message.Request.Account;

namespace Void.Bot.Core.Services.Account;

public partial interface IAccountService
{
    Task<SendRegistrationEmailCodeResponse> SendRegistrationEmailCodeAsync(
        SendRegistrationEmailCodeCommand command,
        CancellationToken cancellationToken);

    Task<SendEmailBindingCodeResponse> SendEmailBindingCodeAsync(
        SendEmailBindingCodeCommand command,
        CancellationToken cancellationToken);

    Task<BindUserEmailResponse> BindUserEmailAsync(
        BindUserEmailCommand command,
        CancellationToken cancellationToken);

    Task<SendEmailUnbindingCodeResponse> SendEmailUnbindingCodeAsync(
        SendEmailUnbindingCodeCommand command,
        CancellationToken cancellationToken);

    Task<UnbindUserEmailResponse> UnbindUserEmailAsync(
        UnbindUserEmailCommand command,
        CancellationToken cancellationToken);

    Task<GetUserEmailStatusResponse> GetUserEmailStatusAsync(
        GetUserEmailStatusRequest request,
        CancellationToken cancellationToken);

    Task<SendPasswordResetCodeResponse> SendPasswordResetCodeAsync(
        SendPasswordResetCodeCommand command,
        CancellationToken cancellationToken);

    Task<ResetUserAccountPasswordResponse> ResetUserAccountPasswordAsync(
        ResetUserAccountPasswordCommand command,
        CancellationToken cancellationToken);

    Task<SendUserAccountPasswordResetLinkResponse> SendUserAccountPasswordResetLinkAsync(
        SendUserAccountPasswordResetLinkCommand command, CancellationToken cancellationToken);

    Task<ResetUserAccountPasswordByLinkResponse> ResetUserAccountPasswordByLinkAsync(
        ResetUserAccountPasswordByLinkCommand command, CancellationToken cancellationToken);
}

public partial class AccountService
{
    public async Task<SendUserAccountPasswordResetLinkResponse> SendUserAccountPasswordResetLinkAsync(
        SendUserAccountPasswordResetLinkCommand command, CancellationToken cancellationToken)
    {
        var user = await _dataProvider.GetUserAccountAsync(command.UserId, string.Empty, cancellationToken).ConfigureAwait(false);
        if (user is null)
            return new SendUserAccountPasswordResetLinkResponse { Code = HttpStatusCode.NotFound, Message = "account_not_found" };
        if (string.IsNullOrWhiteSpace(user.Email) || !user.EmailVerifiedAt.HasValue)
            return new SendUserAccountPasswordResetLinkResponse { Code = HttpStatusCode.Conflict, Message = "verified_email_required" };
        if (!Uri.TryCreate(_accountEmailSetting.PasswordResetFrontendUrl, UriKind.Absolute, out var frontendUrl) ||
            frontendUrl.Scheme is not ("http" or "https"))
            return new SendUserAccountPasswordResetLinkResponse { Code = HttpStatusCode.ServiceUnavailable, Message = "password_reset_url_not_configured" };

        var created = await _passwordResetLinkStore.CreateAsync(
            user.Id, user.Email, TimeSpan.FromMinutes(_accountEmailSetting.PasswordResetLinkTtlMinutes), cancellationToken).ConfigureAwait(false);
        if (created is not CreatePasswordResetLinkResult.Created link)
            return new SendUserAccountPasswordResetLinkResponse { Code = HttpStatusCode.ServiceUnavailable, Message = "password_reset_link_unavailable" };

        var url = $"{frontendUrl.GetLeftPart(UriPartial.Path)}#token={Uri.EscapeDataString(link.Token)}";
        try
        {
            var subject = _localizationService.GetValue<BuiltInEmailContextTemplate>(
                x => x.PasswordResetLinkSubject);
            var content = _localizationService.GetValue<BuiltInEmailContextTemplate>(
                x => x.PasswordResetLinkContent,
                _currentUser.Language,
                url,
                link.ExpiresAt);
            await _emailService.SendAsync(new EmailMessageDto
            {
                To = user.Email,
                Subject = subject,
                TextContent = content
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await _passwordResetLinkStore.RemoveAsync(link.Token, CancellationToken.None).ConfigureAwait(false);
            throw;
        }
        catch (Exception exception)
        {
            await _passwordResetLinkStore.RemoveAsync(link.Token, CancellationToken.None).ConfigureAwait(false);
            _logger.Warning(exception, "Password reset link email failed for account {UserId}", user.Id);
            return new SendUserAccountPasswordResetLinkResponse { Code = HttpStatusCode.BadGateway, Message = "email_send_failed" };
        }
        return new SendUserAccountPasswordResetLinkResponse { Message = "password_reset_link_sent" };
    }

    public async Task<ResetUserAccountPasswordByLinkResponse> ResetUserAccountPasswordByLinkAsync(
        ResetUserAccountPasswordByLinkCommand command, CancellationToken cancellationToken)
    {
        var taken = await _passwordResetLinkStore.TakeAsync(command.Token, cancellationToken).ConfigureAwait(false);
        if (taken is TakePasswordResetLinkResult.Unavailable)
            return new ResetUserAccountPasswordByLinkResponse { Code = HttpStatusCode.ServiceUnavailable, Message = "password_reset_link_unavailable" };
        if (taken is not TakePasswordResetLinkResult.Found found)
            return new ResetUserAccountPasswordByLinkResponse { Code = HttpStatusCode.BadRequest, Message = "invalid_or_expired_password_reset_link" };

        var user = await _dataProvider.GetUserAccountAsync(found.Record.UserId, string.Empty, cancellationToken).ConfigureAwait(false);
        if (user is null || !string.Equals(user.Email, found.Record.Email, StringComparison.OrdinalIgnoreCase) || !user.EmailVerifiedAt.HasValue)
            return new ResetUserAccountPasswordByLinkResponse { Code = HttpStatusCode.BadRequest, Message = "invalid_or_expired_password_reset_link" };
        user.Password = command.NewPassword.SHAEncrypt();
        user.TokenVersion++;
        await _dataProvider.UpdateUserAccountAsync(user, cancellationToken).ConfigureAwait(false);
        _logger.Information("Account password reset through management link for {UserId}", user.Id);
        return new ResetUserAccountPasswordByLinkResponse();
    }

    public async Task<SendRegistrationEmailCodeResponse> SendRegistrationEmailCodeAsync(
        SendRegistrationEmailCodeCommand command,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(command.Email);
        if (!await _dataProvider.IsEmailAvailableAsync(
                email, null, cancellationToken).ConfigureAwait(false))
        {
            return new SendRegistrationEmailCodeResponse
            {
                Code = HttpStatusCode.Conflict,
                Message = "email_already_bound"
            };
        }

        var result = await CreateAndSendVerificationCodeAsync(
            email,
            EmailVerificationPurposeEnum.Registration,
            null,
            cancellationToken).ConfigureAwait(false);
        return new SendRegistrationEmailCodeResponse
        {
            Code = result.Code,
            Message = result.Message,
            Data = result.Data
        };
    }

    public async Task<SendEmailBindingCodeResponse> SendEmailBindingCodeAsync(
        SendEmailBindingCodeCommand command,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue)
        {
            return new SendEmailBindingCodeResponse
            {
                Code = HttpStatusCode.Unauthorized,
                Message = "account_not_authenticated"
            };
        }

        var userId = _currentUser.Id.Value;
        var email = NormalizeEmail(command.Email);
        var user = await _dataProvider.GetUserAccountAsync(
            userId, string.Empty, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return new SendEmailBindingCodeResponse
            {
                Code = HttpStatusCode.BadRequest,
                Message = "account_not_found"
            };
        }

        if (!string.IsNullOrEmpty(user.Email))
        {
            return new SendEmailBindingCodeResponse
            {
                Code = HttpStatusCode.Conflict,
                Message = "account_email_already_bound"
            };
        }

        if (!await _dataProvider.IsEmailAvailableAsync(
                email, userId, cancellationToken).ConfigureAwait(false))
        {
            return new SendEmailBindingCodeResponse
            {
                Code = HttpStatusCode.Conflict,
                Message = "email_already_bound"
            };
        }

        var result = await CreateAndSendVerificationCodeAsync(
            email,
            EmailVerificationPurposeEnum.Binding,
            userId,
            cancellationToken).ConfigureAwait(false);
        return new SendEmailBindingCodeResponse
        {
            Code = result.Code,
            Message = result.Message,
            Data = result.Data
        };
    }

    public async Task<BindUserEmailResponse> BindUserEmailAsync(
        BindUserEmailCommand command,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue)
        {
            return new BindUserEmailResponse
            {
                Code = HttpStatusCode.Unauthorized,
                Message = "account_not_authenticated"
            };
        }

        var userId = _currentUser.Id.Value;
        var user = await _dataProvider.GetUserAccountAsync(
            userId, string.Empty, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return new BindUserEmailResponse
            {
                Code = HttpStatusCode.BadRequest,
                Message = "account_not_found"
            };
        }

        if (!string.IsNullOrEmpty(user.Email))
        {
            return new BindUserEmailResponse
            {
                Code = HttpStatusCode.Conflict,
                Message = "account_email_already_bound"
            };
        }

        var verification = NormalizeEmailVerification(command.EmailVerification);
        if (!await _dataProvider.IsEmailAvailableAsync(
                verification.Email, userId, cancellationToken).ConfigureAwait(false))
        {
            return new BindUserEmailResponse
            {
                Code = HttpStatusCode.Conflict,
                Message = "email_already_bound"
            };
        }

        var verifyResult = await _emailVerificationStore.VerifyAndTakeAsync(
            verification,
            EmailVerificationPurposeEnum.Binding,
            userId,
            cancellationToken).ConfigureAwait(false);
        if (verifyResult is VerifyEmailVerificationResult.Unavailable)
        {
            return new BindUserEmailResponse
            {
                Code = HttpStatusCode.ServiceUnavailable,
                Message = "email_verification_unavailable"
            };
        }

        if (verifyResult is not VerifyEmailVerificationResult.Verified verified)
        {
            _logger.Warning(
                "Email binding verification challenge {ChallengeId} was rejected for account {UserId}",
                verification.ChallengeId,
                userId);
            return new BindUserEmailResponse
            {
                Code = HttpStatusCode.BadRequest,
                Message = "invalid_or_expired_email_verification"
            };
        }

        user.Email = verification.Email;
        user.EmailVerifiedAt = DateTimeOffset.UtcNow;
        try
        {
            await _dataProvider.UpdateUserAccountAsync(user, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await _emailVerificationStore.RestoreAsync(
                verified.Record, CancellationToken.None).ConfigureAwait(false);
            throw;
        }

        _logger.Information("Email bound to account {UserId}: {Email}", userId, MaskEmail(verification.Email));
        return new BindUserEmailResponse
        {
            Data = CreateEmailStatus(user)
        };
    }

    public async Task<GetUserEmailStatusResponse> GetUserEmailStatusAsync(
        GetUserEmailStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue)
        {
            return new GetUserEmailStatusResponse
            {
                Code = HttpStatusCode.Unauthorized,
                Message = "account_not_authenticated"
            };
        }

        var user = await _dataProvider.GetUserAccountAsync(
            _currentUser.Id.Value, string.Empty, cancellationToken).ConfigureAwait(false);
        return user is null
            ? new GetUserEmailStatusResponse
            {
                Code = HttpStatusCode.BadRequest,
                Message = "account_not_found"
            }
            : new GetUserEmailStatusResponse { Data = CreateEmailStatus(user) };
    }

    public async Task<SendEmailUnbindingCodeResponse> SendEmailUnbindingCodeAsync(
        SendEmailUnbindingCodeCommand command,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue)
        {
            return new SendEmailUnbindingCodeResponse
            {
                Code = HttpStatusCode.Unauthorized,
                Message = "account_not_authenticated"
            };
        }

        var userId = _currentUser.Id.Value;
        var user = await _dataProvider.GetUserAccountAsync(
            userId, string.Empty, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return new SendEmailUnbindingCodeResponse
            {
                Code = HttpStatusCode.BadRequest,
                Message = "account_not_found"
            };
        }

        if (string.IsNullOrWhiteSpace(user.Email) || !user.EmailVerifiedAt.HasValue)
        {
            return new SendEmailUnbindingCodeResponse
            {
                Code = HttpStatusCode.Conflict,
                Message = "account_email_not_bound"
            };
        }

        var result = await CreateAndSendVerificationCodeAsync(
            user.Email,
            EmailVerificationPurposeEnum.Unbinding,
            userId,
            cancellationToken).ConfigureAwait(false);
        return new SendEmailUnbindingCodeResponse
        {
            Code = result.Code,
            Message = result.Message,
            Data = result.Data
        };
    }

    public async Task<UnbindUserEmailResponse> UnbindUserEmailAsync(
        UnbindUserEmailCommand command,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue)
        {
            return new UnbindUserEmailResponse
            {
                Code = HttpStatusCode.Unauthorized,
                Message = "account_not_authenticated"
            };
        }

        var userId = _currentUser.Id.Value;
        var user = await _dataProvider.GetUserAccountAsync(
            userId, string.Empty, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return new UnbindUserEmailResponse
            {
                Code = HttpStatusCode.BadRequest,
                Message = "account_not_found"
            };
        }

        if (string.IsNullOrEmpty(user.Email))
        {
            _logger.Information("Email unbind skipped because account {UserId} has no bound email", userId);
            return new UnbindUserEmailResponse { Data = CreateEmailStatus(user) };
        }

        var verifyResult = await _emailVerificationStore.VerifyAndTakeAsync(
            new EmailVerificationDto
            {
                Email = NormalizeEmail(user.Email),
                ChallengeId = command.ChallengeId,
                Code = command.Code.Trim()
            },
            EmailVerificationPurposeEnum.Unbinding,
            userId,
            cancellationToken).ConfigureAwait(false);
        if (verifyResult is VerifyEmailVerificationResult.Unavailable)
        {
            return new UnbindUserEmailResponse
            {
                Code = HttpStatusCode.ServiceUnavailable,
                Message = "email_verification_unavailable"
            };
        }

        if (verifyResult is not VerifyEmailVerificationResult.Verified verified)
        {
            _logger.Warning(
                "Email unbinding verification challenge {ChallengeId} was rejected for account {UserId}",
                command.ChallengeId,
                userId);
            return new UnbindUserEmailResponse
            {
                Code = HttpStatusCode.BadRequest,
                Message = "invalid_or_expired_email_verification"
            };
        }

        var maskedEmail = MaskEmail(user.Email);
        user.Email = null;
        user.EmailVerifiedAt = null;
        try
        {
            await _dataProvider.UpdateUserAccountAsync(user, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await _emailVerificationStore.RestoreAsync(
                verified.Record, CancellationToken.None).ConfigureAwait(false);
            throw;
        }

        _logger.Information("Email {Email} unbound from account {UserId}", maskedEmail, userId);
        return new UnbindUserEmailResponse
        {
            Data = CreateEmailStatus(user)
        };
    }

    public async Task<SendPasswordResetCodeResponse> SendPasswordResetCodeAsync(
        SendPasswordResetCodeCommand command,
        CancellationToken cancellationToken)
    {
        const string message = "If the email is registered, a verification code has been sent.";
        var email = NormalizeEmail(command.Email);
        var user = await _dataProvider.GetUserAccountByEmailAsync(
            email, cancellationToken).ConfigureAwait(false);
        if (user?.EmailVerifiedAt is null)
        {
            var decoy = await _emailVerificationStore.CreateAsync(
                email,
                EmailVerificationPurposeEnum.PasswordReset,
                null,
                cancellationToken).ConfigureAwait(false);
            if (decoy is CreateEmailVerificationResult.Created created)
            {
                await _emailVerificationStore.RemoveAsync(
                    created.Record.ChallengeId, cancellationToken).ConfigureAwait(false);
            }

            return new SendPasswordResetCodeResponse
            {
                Message = message,
                Data = CreateDecoyChallenge()
            };
        }

        var result = await CreateAndSendVerificationCodeAsync(
            email,
            EmailVerificationPurposeEnum.PasswordReset,
            user.Id,
            cancellationToken).ConfigureAwait(false);
        return new SendPasswordResetCodeResponse
        {
            Message = message,
            Data = result.Data ?? CreateDecoyChallenge()
        };
    }

    public async Task<ResetUserAccountPasswordResponse> ResetUserAccountPasswordAsync(
        ResetUserAccountPasswordCommand command,
        CancellationToken cancellationToken)
    {
        if (command.NewPassword != command.NewPasswordConfirm)
        {
            return new ResetUserAccountPasswordResponse
            {
                Code = HttpStatusCode.BadRequest,
                Message = "new_password_confirmation_mismatch"
            };
        }

        var email = NormalizeEmail(command.Email);
        var user = await _dataProvider.GetUserAccountByEmailAsync(
            email, cancellationToken).ConfigureAwait(false);
        if (user?.EmailVerifiedAt is null)
        {
            return InvalidPasswordResetResponse();
        }

        var verifyResult = await _emailVerificationStore.VerifyAndTakeAsync(
            new EmailVerificationDto
            {
                Email = email,
                ChallengeId = command.ChallengeId,
                Code = command.Code
            },
            EmailVerificationPurposeEnum.PasswordReset,
            user.Id,
            cancellationToken).ConfigureAwait(false);
        if (verifyResult is VerifyEmailVerificationResult.Unavailable)
        {
            return new ResetUserAccountPasswordResponse
            {
                Code = HttpStatusCode.ServiceUnavailable,
                Message = "email_verification_unavailable"
            };
        }

        if (verifyResult is not VerifyEmailVerificationResult.Verified verified)
        {
            _logger.Warning(
                "Password reset verification challenge {ChallengeId} was rejected",
                command.ChallengeId);
            return InvalidPasswordResetResponse();
        }

        user.Password = command.NewPassword.SHAEncrypt();
        user.TokenVersion++;
        try
        {
            await _dataProvider.UpdateUserAccountAsync(user, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await _emailVerificationStore.RestoreAsync(
                verified.Record, CancellationToken.None).ConfigureAwait(false);
            throw;
        }

        _logger.Information(
            "Account password reset completed for {UserId}; token version advanced to {TokenVersion}",
            user.Id,
            user.TokenVersion);
        return new ResetUserAccountPasswordResponse();
    }

    private async Task<EmailChallengeCreationResult> CreateAndSendVerificationCodeAsync(
        string email,
        EmailVerificationPurposeEnum purpose,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var createResult = await _emailVerificationStore.CreateAsync(
            email, purpose, userId, cancellationToken).ConfigureAwait(false);
        if (createResult is CreateEmailVerificationResult.RateLimited)
        {
            return new EmailChallengeCreationResult(
                HttpStatusCode.TooManyRequests, "email_verification_rate_limited", null);
        }

        if (createResult is not CreateEmailVerificationResult.Created created)
        {
            return new EmailChallengeCreationResult(
                HttpStatusCode.ServiceUnavailable, "email_verification_unavailable", null);
        }

        try
        {
            var subject = GetVerificationEmailSubject(purpose);
            var introduction = _localizationService.GetValue<BuiltInEmailContextTemplate>(
                x => x.VerificationIntroduction);
            var expirationNotice = _localizationService.GetValue<BuiltInEmailContextTemplate>(
                x => x.VerificationExpirationNotice,
                _currentUser.Language,
                _accountEmailSetting.CodeTtlMinutes);
            var textContent = $"{introduction} {created.Code}\n{expirationNotice}";
            await _emailService.SendAsync(
                new EmailMessageDto
                {
                    To = email,
                    Subject = subject,
                    TextContent = textContent,
                    HtmlContent = EmailHtmlTemplate.RenderVerificationCode(
                        subject,
                        introduction,
                        created.Code,
                        expirationNotice,
                        GetEmailTemplateText())
                },
                cancellationToken).ConfigureAwait(false);
        }
        catch (EmailServiceException exception)
        {
            await _emailVerificationStore.RemoveAsync(
                created.Record.ChallengeId, CancellationToken.None).ConfigureAwait(false);
            _logger.Warning(
                exception,
                "Account email verification delivery failed for {Purpose}: {ErrorCode}",
                purpose,
                exception.ProviderErrorCode ?? "email_send_failed");
            return new EmailChallengeCreationResult(
                HttpStatusCode.BadGateway,
                exception.ProviderErrorCode ?? "email_send_failed",
                null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            await _emailVerificationStore.RemoveAsync(
                created.Record.ChallengeId, CancellationToken.None).ConfigureAwait(false);
            _logger.Error(
                exception,
                "Unexpected account email verification delivery failure for {Purpose}",
                purpose);
            return new EmailChallengeCreationResult(
                HttpStatusCode.BadGateway,
                "email_send_failed",
                null);
        }

        _logger.Information(
            "Account email verification sent for {Purpose} to {Email}",
            purpose,
            MaskEmail(email));
        return new EmailChallengeCreationResult(
            default,
            string.Empty,
            new EmailVerificationChallengeDto
            {
                ChallengeId = created.Record.ChallengeId,
                ExpiresAt = created.Record.ExpiresAt
            });
    }

    private static EmailVerificationDto NormalizeEmailVerification(EmailVerificationDto verification)
    {
        return new EmailVerificationDto
        {
            Email = NormalizeEmail(verification.Email),
            ChallengeId = verification.ChallengeId,
            Code = verification.Code.Trim()
        };
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static UserEmailStatusDto CreateEmailStatus(UserAccountDto user)
    {
        return new UserEmailStatusDto
        {
            IsBound = !string.IsNullOrEmpty(user.Email) && user.EmailVerifiedAt.HasValue,
            MaskedEmail = string.IsNullOrEmpty(user.Email) ? null : MaskEmail(user.Email),
            VerifiedAt = user.EmailVerifiedAt
        };
    }

    private static string MaskEmail(string email)
    {
        var separator = email.IndexOf('@');
        return separator <= 0 ? "***" : $"{email[0]}***{email[separator..]}";
    }

    private string GetVerificationEmailSubject(EmailVerificationPurposeEnum purpose)
    {
        return purpose switch
        {
            EmailVerificationPurposeEnum.PasswordReset =>
                _localizationService.GetValue<BuiltInEmailContextTemplate>(x => x.PasswordResetCodeSubject),
            EmailVerificationPurposeEnum.Unbinding =>
                _localizationService.GetValue<BuiltInEmailContextTemplate>(x => x.EmailUnbindingCodeSubject),
            _ => _localizationService.GetValue<BuiltInEmailContextTemplate>(x => x.VerificationSubject)
        };
    }

    private EmailTemplateText GetEmailTemplateText()
    {
        return new EmailTemplateText(
            _currentUser.Language.ToI18nCode(),
            _localizationService.GetValue<EmailLayoutContextTemplate>(x => x.TeamSignature),
            _localizationService.GetValue<EmailLayoutContextTemplate>(x => x.AutomatedNotice));
    }

    private static ResetUserAccountPasswordResponse InvalidPasswordResetResponse()
    {
        return new ResetUserAccountPasswordResponse
        {
            Code = HttpStatusCode.BadRequest,
            Message = "invalid_or_expired_email_verification"
        };
    }

    private EmailVerificationChallengeDto CreateDecoyChallenge()
    {
        return new EmailVerificationChallengeDto
        {
            ChallengeId = Guid.NewGuid(),
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_accountEmailSetting.CodeTtlMinutes)
        };
    }

    private sealed record EmailChallengeCreationResult(
        HttpStatusCode Code,
        string Message,
        EmailVerificationChallengeDto? Data);
}
