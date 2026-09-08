using System.Net;
using AutoMapper;
using Serilog;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Services.Email;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Settings.Account;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Account;
using Void.Bot.Message.Request.Account;

namespace Void.Bot.Core.Services.Account;

public partial interface IAccountService : IScope
{
    Task<GetUserAccountResponse> GetUserAccountAsync(
        GetUserAccountRequest request, CancellationToken cancellationToken);
    
    Task<GetUserAccountPageResponse> GetUserAccountPageAsync(
        GetUserAccountPageRequest request, CancellationToken cancellationToken);

    Task<GetUserAccountByThirdPartyResponse> GetUserAccountByThirdPartyAsync(
        GetUserAccountByThirdPartyRequest request, CancellationToken cancellationToken);

    Task<GetThirdPartyByThirdPartyResponse> GetThirdPartyByThirdPartyAsync(
        GetThirdPartyByThirdPartyRequest request, CancellationToken cancellationToken);

    Task<RegisterUserAccountResponse> RegisterUserAccountAsync(
        RegisterUserAccountCommand command, CancellationToken cancellationToken);

    Task<CanChangeUserAccountNameResponse> CanChangeUserAccountNameAsync(
        CanChangeUserAccountNameRequest request, CancellationToken cancellationToken);

    Task<ChangeUserAccountNameResponse> ChangeUserAccountNameAsync(
        ChangeUserAccountNameCommand command, CancellationToken cancellationToken);

    Task<LinkUserAccountFromThirdPartyResponse> LinkAccountFromThirdPartyAsync(
        LinkUserAccountFromThirdPartyCommand command, CancellationToken cancellationToken);

    Task<UnlinkUserAccountFromThirdPartyResponse> UnlinkAccountFromThirdPartyAsync(
        UnlinkUserAccountFromThirdPartyCommand command, CancellationToken cancellationToken);

    Task<ChangeUserAccountPasswordResponse> ChangeUserAccountPasswordAsync(
        ChangeUserAccountPasswordCommand command, CancellationToken cancellationToken);

    Task<ForceResetUserAccountPasswordResponse> ForceResetUserAccountPasswordAsync(
        ForceResetUserAccountPasswordCommand command, CancellationToken cancellationToken);

    Task<ForceUnlinkUserAccountThirdPartyResponse> ForceUnlinkUserAccountThirdPartyAsync(
        ForceUnlinkUserAccountThirdPartyCommand command, CancellationToken cancellationToken);
    
    Task<bool> VerifyPassword(Guid userId, string password, CancellationToken cancellationToken);
}

public partial class AccountService : IAccountService
{
    private readonly IMapper _mapper;
    private readonly IAccountDataProvider _dataProvider;
    private readonly IAccountEmailVerificationStore _emailVerificationStore;
    private readonly IAccountPasswordResetLinkStore _passwordResetLinkStore;
    private readonly IEmailService _emailService;
    private readonly ILocalizationService _localizationService;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger _logger;
    private readonly AccountEmailSetting _accountEmailSetting;

    public AccountService(
        IMapper mapper,
        IAccountDataProvider dataProvider,
        IAccountEmailVerificationStore emailVerificationStore,
        IAccountPasswordResetLinkStore passwordResetLinkStore,
        IEmailService emailService,
        ILocalizationService localizationService,
        ICurrentUser currentUser,
        ILogger logger,
        AccountEmailSetting accountEmailSetting)
    {
        _mapper = mapper;
        _dataProvider = dataProvider;
        _emailVerificationStore = emailVerificationStore;
        _passwordResetLinkStore = passwordResetLinkStore;
        _emailService = emailService;
        _localizationService = localizationService;
        _currentUser = currentUser;
        _logger = logger;
        _accountEmailSetting = accountEmailSetting;
    }

    public async Task<GetUserAccountResponse> GetUserAccountAsync(
        GetUserAccountRequest request, CancellationToken cancellationToken)
    {
        var user = await _dataProvider.GetUserAccountAsync(
            request.UserId, request.DisplayName, cancellationToken).ConfigureAwait(false);

        return new GetUserAccountResponse
        {
            Data = _mapper.Map<UserAccountInfoDto>(user)
        };
    }

    public async Task<GetUserAccountPageResponse> GetUserAccountPageAsync(
        GetUserAccountPageRequest request, CancellationToken cancellationToken)
    {
        var list = await _dataProvider.GetUserAccountPageAsync(
            request.UserIds, request.PageIndex, request.PageSize, cancellationToken).ConfigureAwait(false);

        return new GetUserAccountPageResponse
        {
            Data = _mapper.Map<PagedListDto<UserAccountInfoDto>>(list)
        };
    }

    public async Task<GetUserAccountByThirdPartyResponse> GetUserAccountByThirdPartyAsync(
        GetUserAccountByThirdPartyRequest request, CancellationToken cancellationToken)
    {
        var user = await _dataProvider.GetUserAccountByThirdPartyAsync(
            request.Platform.ToString(), request.ThirdPartyUserId, cancellationToken).ConfigureAwait(false);

        return new GetUserAccountByThirdPartyResponse
        {
            Data = _mapper.Map<UserAccountInfoDto>(user)
        };
    }

    public async Task<GetThirdPartyByThirdPartyResponse> GetThirdPartyByThirdPartyAsync(
        GetThirdPartyByThirdPartyRequest request, CancellationToken cancellationToken)
    {
        var thirdParty = await _dataProvider.GetThirdPartyByThirdPartyAsync(
            request.CurrentPlatform.ToString(), request.ThirdPartyUserId,
            request.TargetPlatform.ToString(), cancellationToken).ConfigureAwait(false);

        return new GetThirdPartyByThirdPartyResponse
        {
            Data = thirdParty
        };
    }

    public async Task<RegisterUserAccountResponse> RegisterUserAccountAsync(
        RegisterUserAccountCommand command, CancellationToken cancellationToken)
    {
        if (command.Name is null || command.Name.Length is < 1 or > 64)
        {
            return new RegisterUserAccountResponse
            {
                Code = HttpStatusCode.BadRequest,
                Message = "Name must be between 1 and 64 characters long."
            };
        }

        AccountEmailVerificationRecord? verifiedEmail = null;
        if (command.EmailVerification is not null)
        {
            var verification = NormalizeEmailVerification(command.EmailVerification);
            if (!await _dataProvider.IsEmailAvailableAsync(
                    verification.Email, null, cancellationToken).ConfigureAwait(false))
            {
                return new RegisterUserAccountResponse
                {
                    Code = HttpStatusCode.Conflict,
                    Message = "email_already_bound"
                };
            }

            var verifyResult = await _emailVerificationStore.VerifyAndTakeAsync(
                verification,
                EmailVerificationPurposeEnum.Registration,
                null,
                cancellationToken).ConfigureAwait(false);
            if (verifyResult is VerifyEmailVerificationResult.Unavailable)
            {
                return new RegisterUserAccountResponse
                {
                    Code = HttpStatusCode.ServiceUnavailable,
                    Message = "email_verification_unavailable"
                };
            }

            if (verifyResult is not VerifyEmailVerificationResult.Verified verified)
            {
                _logger.Warning(
                    "Registration email verification challenge {ChallengeId} was rejected",
                    verification.ChallengeId);
                return new RegisterUserAccountResponse
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "invalid_or_expired_email_verification"
                };
            }

            verifiedEmail = verified.Record;
        }

        UserAccountDto? user;
        try
        {
            user = await _dataProvider.RegisterUserAccountAsync(
                command.Name,
                command.Password.SHAEncrypt(),
                verifiedEmail?.Email,
                verifiedEmail is null ? null : DateTimeOffset.UtcNow,
                cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            if (verifiedEmail is not null)
            {
                await _emailVerificationStore.RestoreAsync(
                    verifiedEmail, CancellationToken.None).ConfigureAwait(false);
            }

            throw;
        }

        if (user is null && verifiedEmail is not null)
        {
            await _emailVerificationStore.RestoreAsync(
                verifiedEmail, CancellationToken.None).ConfigureAwait(false);
        }

        if (user is not null)
        {
            _logger.Information(
                "Account {UserId} registered with email verification: {HasVerifiedEmail}",
                user.Id,
                verifiedEmail is not null);
        }

        return new RegisterUserAccountResponse
        {
            Data = _mapper.Map<UserAccountInfoDto>(user)
        };
    }

    public async Task<CanChangeUserAccountNameResponse> CanChangeUserAccountNameAsync(
        CanChangeUserAccountNameRequest request, CancellationToken cancellationToken)
    {
        var canChangeName = request.UserId.HasValue &&
                            request.Name is { Length: >= 1 and <= 64 } &&
                            request.Name.All(character => !char.IsWhiteSpace(character)) &&
                            await _dataProvider.CanChangeUserAccountNameAsync(
                                request.UserId.Value, request.Name, cancellationToken).ConfigureAwait(false);

        return new CanChangeUserAccountNameResponse
        {
            Data = canChangeName
        };
    }

    public async Task<ChangeUserAccountNameResponse> ChangeUserAccountNameAsync(
        ChangeUserAccountNameCommand command, CancellationToken cancellationToken)
    {
        if (command.Name is null || command.Name.Length is < 1 or > 64 ||
            command.Name.Any(char.IsWhiteSpace))
        {
            return new ChangeUserAccountNameResponse
            {
                Code = HttpStatusCode.BadRequest,
                Message = "Name must be between 1 and 64 characters long and cannot contain whitespace characters."
            };
        }

        if (!command.UserId.HasValue)
        {
            return new ChangeUserAccountNameResponse
            {
                Code = HttpStatusCode.BadRequest,
                Message = "Account not found."
            };
        }

        var user = await _dataProvider.ChangeUserAccountNameAsync(
            command.UserId.Value, command.Name, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return new ChangeUserAccountNameResponse
            {
                Code = HttpStatusCode.Conflict,
                Message = "Name cannot be changed."
            };
        }

        var account = await _dataProvider.GetUserAccountAsync(
            command.UserId.Value, string.Empty, cancellationToken).ConfigureAwait(false);

        return new ChangeUserAccountNameResponse
        {
            Data = _mapper.Map<UserAccountInfoDto>(account)
        };
    }

    public async Task<LinkUserAccountFromThirdPartyResponse> LinkAccountFromThirdPartyAsync(
        LinkUserAccountFromThirdPartyCommand command, CancellationToken cancellationToken)
    {
        Guid? registeredAccountId = null;
        var getUserByThirdPartyResponse = await GetUserAccountByThirdPartyAsync(
            new GetUserAccountByThirdPartyRequest
            {
                Platform = command.Authorization.Platform,
                ThirdPartyUserId = command.Authorization.AccountId
            }, cancellationToken).ConfigureAwait(false);

        if (getUserByThirdPartyResponse.Data == null)
        {
            UserAccountInfoDto userInfo;

            if (string.IsNullOrEmpty(command.MasterDisplayName))
            {
                var registerResponse = await RegisterUserAccountAsync(new RegisterUserAccountCommand
                {
                    Name = command.Authorization.AccountName,
                    Password = "".GenerateRandomString()
                }, cancellationToken).ConfigureAwait(false);

                userInfo = registerResponse.Data ?? throw new Exception($"创建{AppPlatformEnum.Void}账号失败.");
                registeredAccountId = userInfo.Id;
            }
            else
            {
                var getUserAccountResponse = await GetUserAccountAsync(
                    new GetUserAccountRequest
                    {
                        DisplayName = command.MasterDisplayName
                    }, cancellationToken).ConfigureAwait(false);

                userInfo = getUserAccountResponse.Data ?? throw new Exception($"未找到对应{AppPlatformEnum.Void}账号.");
            }

            var thirdParty = new UserThirdPartyDto
            {
                Id = Guid.NewGuid(),
                UserId = userInfo.Id,
                Provider = command.Authorization.Platform.ToString(),
                ThirdPartyUserId = command.Authorization.AccountId,
                AccessToken = command.AccessToken,
                AccessTokenExpiresAt = command.AccessTokenExpiresAt,
                RefreshToken = command.RefreshToken,
                RefreshTokenExpiresAt = command.RefreshTokenExpiresAt,
                ExtraData = command.ExtraData
            };

            await _dataProvider.LinkThirdPartyAsync(thirdParty, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var thirdParty = await _dataProvider.GetThirdPartyByUserIdAsync(
                getUserByThirdPartyResponse.Data.Id,
                command.Authorization.Platform.ToString(), cancellationToken).ConfigureAwait(false);

            if (thirdParty != null)
            {
                thirdParty.AccessToken = command.AccessToken;
                thirdParty.AccessTokenExpiresAt = command.AccessTokenExpiresAt;
                thirdParty.RefreshToken = command.RefreshToken;
                thirdParty.RefreshTokenExpiresAt = command.RefreshTokenExpiresAt;
                thirdParty.ExtraData = command.ExtraData;

                await _dataProvider.UpdateThirdPartyAsync(thirdParty, cancellationToken).ConfigureAwait(false);
            }
        }

        return new LinkUserAccountFromThirdPartyResponse
        {
            RegisteredAccountId = registeredAccountId
        };
    }

    public async Task<UnlinkUserAccountFromThirdPartyResponse> UnlinkAccountFromThirdPartyAsync(
        UnlinkUserAccountFromThirdPartyCommand command, CancellationToken cancellationToken)
    {
        await _dataProvider.DeleteThirdPartyAsync(
            command.UserId, command.Platform.ToString(), cancellationToken).ConfigureAwait(false);

        return new UnlinkUserAccountFromThirdPartyResponse();
    }

    public async Task<ChangeUserAccountPasswordResponse> ChangeUserAccountPasswordAsync(
        ChangeUserAccountPasswordCommand command, CancellationToken cancellationToken)
    {
        var response = new ChangeUserAccountPasswordResponse();
        var user = await _dataProvider.GetUserAccountAsync(
            command.UserId, string.Empty, cancellationToken).ConfigureAwait(false);

        if (user == null)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = "Account not found.";
            return response;
        }

        if (command.OldPassword.SHAEncrypt() != user.Password)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = "Old password is incorrect.";
            return response;
        }

        if (command.NewPassword != command.NewPasswordConfirm)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = "New password confirmation does not match.";
            return response;
        }

        user.Password = command.NewPassword.SHAEncrypt();
        user.TokenVersion++;

        await _dataProvider.UpdateUserAccountAsync(user, cancellationToken).ConfigureAwait(false);
        _logger.Information(
            "Account password changed for {UserId}; token version advanced to {TokenVersion}",
            user.Id,
            user.TokenVersion);

        return response;
    }

    public async Task<ForceResetUserAccountPasswordResponse> ForceResetUserAccountPasswordAsync(
        ForceResetUserAccountPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _dataProvider.GetUserAccountAsync(
            command.UserId, string.Empty, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return new ForceResetUserAccountPasswordResponse
            {
                Code = HttpStatusCode.NotFound,
                Message = "Account not found."
            };
        }

        user.Password = command.NewPassword.SHAEncrypt();
        user.TokenVersion++;

        await _dataProvider.UpdateUserAccountAsync(user, cancellationToken).ConfigureAwait(false);
        _logger.Information(
            "Account password forcibly reset for {UserId}; token version advanced to {TokenVersion}",
            user.Id,
            user.TokenVersion);

        return new ForceResetUserAccountPasswordResponse();
    }

    public async Task<ForceUnlinkUserAccountThirdPartyResponse> ForceUnlinkUserAccountThirdPartyAsync(
        ForceUnlinkUserAccountThirdPartyCommand command, CancellationToken cancellationToken)
    {
        var account = await _dataProvider.GetUserAccountAsync(
            command.UserId, string.Empty, cancellationToken).ConfigureAwait(false);
        if (account is null)
        {
            return new ForceUnlinkUserAccountThirdPartyResponse
            {
                Code = HttpStatusCode.NotFound,
                Message = "Account not found."
            };
        }

        var binding = await _dataProvider.GetThirdPartyByUserIdAsync(
            command.UserId, command.Platform.ToString(), cancellationToken).ConfigureAwait(false);
        if (binding is null)
        {
            return new ForceUnlinkUserAccountThirdPartyResponse
            {
                Code = HttpStatusCode.NotFound,
                Message = "Third-party binding not found."
            };
        }

        await _dataProvider.DeleteThirdPartyAsync(
            command.UserId, command.Platform.ToString(), cancellationToken).ConfigureAwait(false);
        _logger.Information(
            "Third-party binding forcibly unlinked for account {UserId}, platform {Platform}, provider user {ProviderUserId}",
            command.UserId,
            command.Platform,
            binding.ThirdPartyUserId);

        return new ForceUnlinkUserAccountThirdPartyResponse();
    }

    public async Task<bool> VerifyPassword(Guid userId, string password, CancellationToken cancellationToken)
    {
        var user = await _dataProvider.GetUserAccountAsync(userId, string.Empty, cancellationToken).ConfigureAwait(false);

        return user != null && user.Password == password.SHAEncrypt();
    }
}
