using System.Net;
using System.Security.Claims;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Settings.OAuth;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Commands.Identity;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Dto.Identity;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Account;
using Void.Bot.Message.Enum.Identity;
using Void.Bot.Message.Request.Account;

namespace Void.Bot.Core.Services.Identity.OAuth;

public interface IOAuthService : IScope
{
    Task<OAuthAuthorizeResponse> AuthorizeAsync(
        AppPlatformEnum platform,
        CancellationToken cancellationToken);

    Task<OAuthCallbackResponse> CallbackAsync(
        AppPlatformEnum platform,
        OAuthCallbackContext context,
        CancellationToken cancellationToken);

    Task<OAuthUnlinkResponse> UnlinkAsync(
        AppPlatformEnum platform,
        CancellationToken cancellationToken);

    Task<OAuthExchangeResponse> ExchangeAsync(
        string code,
        CancellationToken cancellationToken);

    Task<OAuthRegisterResponse> RegisterAsync(
        OAuthRegisterCommand command,
        CancellationToken cancellationToken);

    Task<OAuthBindResponse> BindAsync(
        OAuthBindCommand command,
        CancellationToken cancellationToken);
}

public sealed class OAuthService : IOAuthService
{
    private readonly IReadOnlyDictionary<AppPlatformEnum, IOAuthProvider> _providers;
    private readonly ICurrentUser _currentUser;
    private readonly IOAuthExchangeStore _exchangeStore;
    private readonly IOAuthRegistrationStore _registrationStore;
    private readonly IAccountService _accountService;
    private readonly IAccountTwoFactorService _accountTwoFactorService;
    private readonly IAccountDataProvider _accountDataProvider;
    private readonly ITokenClaimService _tokenClaimService;
    private readonly OAuthSetting _oauthSetting;

    public OAuthService(
        IEnumerable<IOAuthProvider> providers,
        ICurrentUser currentUser,
        IOAuthExchangeStore exchangeStore,
        IOAuthRegistrationStore registrationStore,
        IAccountService accountService,
        IAccountTwoFactorService accountTwoFactorService,
        IAccountDataProvider accountDataProvider,
        ITokenClaimService tokenClaimService,
        OAuthSetting oauthSetting)
    {
        _providers = providers.ToDictionary(provider => provider.Platform);
        _currentUser = currentUser;
        _exchangeStore = exchangeStore;
        _registrationStore = registrationStore;
        _accountService = accountService;
        _accountTwoFactorService = accountTwoFactorService;
        _accountDataProvider = accountDataProvider;
        _tokenClaimService = tokenClaimService;
        _oauthSetting = oauthSetting;
    }

    public Task<OAuthAuthorizeResponse> AuthorizeAsync(
        AppPlatformEnum platform,
        CancellationToken cancellationToken)
    {
        return GetProvider(platform).AuthorizeAsync(_currentUser.Id, cancellationToken);
    }

    public async Task<OAuthCallbackResponse> CallbackAsync(
        AppPlatformEnum platform,
        OAuthCallbackContext context,
        CancellationToken cancellationToken)
    {
        var response = await GetProvider(platform)
            .CallbackAsync(context, cancellationToken)
            .ConfigureAwait(false);
        if (response.Data is not { } data)
        {
            return response;
        }

        if (data.Flow == OAuthStateFlow.Register)
        {
            return await CreateRegistrationAsync(response, data, platform, cancellationToken).ConfigureAwait(false);
        }

        if (data.Flow != OAuthStateFlow.Login)
        {
            return response;
        }

        var result = await _exchangeStore.CreateAsync(
            new JwtToken
            {
                AccessToken = data.AccessToken,
                RefreshToken = data.RefreshToken
            },
            TimeSpan.FromSeconds(_oauthSetting.ExchangeTtlSeconds),
            cancellationToken).ConfigureAwait(false);
        if (result is not CreateOAuthExchangeResult.Created created)
        {
            var error = result is CreateOAuthExchangeResult.Unavailable
                ? OAuthError.ExchangeStoreUnavailable
                : OAuthError.ProviderFailure;
            return new OAuthCallbackResponse
            {
                Code = error.StatusCode,
                Message = error.Code
            };
        }

        data.AccessToken = string.Empty;
        data.RefreshToken = string.Empty;
        data.ExchangeCode = created.Code;
        return response;
    }

    public Task<OAuthUnlinkResponse> UnlinkAsync(
        AppPlatformEnum platform,
        CancellationToken cancellationToken)
    {
        return GetProvider(platform).UnlinkAsync(_currentUser.Id ?? Guid.Empty, cancellationToken);
    }

    public async Task<OAuthExchangeResponse> ExchangeAsync(
        string code,
        CancellationToken cancellationToken)
    {
        var result = await _exchangeStore.TakeAsync(code, cancellationToken).ConfigureAwait(false);
        if (result is TakeOAuthExchangeResult.Found found)
        {
            return new OAuthExchangeResponse { Data = found.Token };
        }

        var error = result is TakeOAuthExchangeResult.Unavailable
            ? OAuthError.ExchangeStoreUnavailable
            : OAuthError.InvalidExchangeCode;
        return new OAuthExchangeResponse
        {
            Code = error.StatusCode,
            Message = error.Code
        };
    }

    public async Task<OAuthRegisterResponse> RegisterAsync(
        OAuthRegisterCommand command,
        CancellationToken cancellationToken)
    {
        var takeResult = await _registrationStore.TakeAsync(command.Code, cancellationToken).ConfigureAwait(false);
        if (takeResult is not TakeOAuthRegistrationResult.Found found)
        {
            return Error<OAuthRegisterResponse>(takeResult is TakeOAuthRegistrationResult.Unavailable
                ? OAuthError.RegistrationStoreUnavailable
                : OAuthError.InvalidRegistrationCode);
        }

        var record = found.Record;
        try
        {
            var existingBinding = await _accountDataProvider.GetThirdPartyBindingAsync(
                record.Platform.ToString(),
                record.ProviderUserId,
                cancellationToken).ConfigureAwait(false);
            if (existingBinding is not null)
            {
                return Error<OAuthRegisterResponse>(OAuthError.BindingConflict);
            }

            var registration = await _accountService.RegisterUserAccountAsync(
                new RegisterUserAccountCommand
                {
                    Name = command.Name,
                    Password = command.Password,
                    EmailVerification = command.EmailVerification
                },
                cancellationToken).ConfigureAwait(false);
            if (registration.Data is not { } account)
            {
                await _registrationStore.RestoreAsync(record, cancellationToken).ConfigureAwait(false);
                return Error<OAuthRegisterResponse>(OAuthError.RegistrationFailed);
            }

            await _accountDataProvider.LinkThirdPartyAsync(
                CreateBinding(account.Id, record),
                cancellationToken).ConfigureAwait(false);
            return new OAuthRegisterResponse
            {
                Data = GenerateJwt(account.Id, account.DisplayName, account.TokenVersion),
                RegisteredAccountId = account.Id
            };
        }
        catch
        {
            await _registrationStore.RestoreAsync(record, cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public async Task<OAuthBindResponse> BindAsync(
        OAuthBindCommand command,
        CancellationToken cancellationToken)
    {
        var getResult = await _registrationStore.GetAsync(command.Code, cancellationToken).ConfigureAwait(false);
        if (getResult is not GetOAuthRegistrationResult.Found)
        {
            return Error<OAuthBindResponse>(getResult is GetOAuthRegistrationResult.Unavailable
                ? OAuthError.RegistrationStoreUnavailable
                : OAuthError.InvalidRegistrationCode);
        }

        var accountResponse = await _accountService.GetUserAccountAsync(
            new GetUserAccountRequest { DisplayName = $"{command.Name}#{command.Suffix}" },
            cancellationToken).ConfigureAwait(false);
        if (accountResponse?.Data is not { } account ||
            !await _accountService.VerifyPassword(account.Id, command.Password, cancellationToken).ConfigureAwait(false))
        {
            return Error<OAuthBindResponse>(OAuthError.InvalidCredentials);
        }

        var twoFactorResult = await _accountTwoFactorService.VerifyLoginAsync(
            account.Id, command.TwoFactorCode, cancellationToken).ConfigureAwait(false);
        if (twoFactorResult is TwoFactorLoginVerificationResult.CodeRequired or
            TwoFactorLoginVerificationResult.InvalidCode)
        {
            return new OAuthBindResponse
            {
                Code = HttpStatusCode.Unauthorized,
                Message = twoFactorResult == TwoFactorLoginVerificationResult.CodeRequired
                    ? "two_factor_code_required"
                    : "invalid_two_factor_code",
                RequiresTwoFactor = true
            };
        }

        var takeResult = await _registrationStore.TakeAsync(command.Code, cancellationToken).ConfigureAwait(false);
        if (takeResult is not TakeOAuthRegistrationResult.Found found)
        {
            return Error<OAuthBindResponse>(takeResult is TakeOAuthRegistrationResult.Unavailable
                ? OAuthError.RegistrationStoreUnavailable
                : OAuthError.InvalidRegistrationCode);
        }

        var record = found.Record;
        try
        {
            var identityBinding = await _accountDataProvider.GetThirdPartyBindingAsync(
                record.Platform.ToString(),
                record.ProviderUserId,
                cancellationToken).ConfigureAwait(false);
            if (identityBinding is not null)
            {
                return Error<OAuthBindResponse>(OAuthError.BindingConflict);
            }

            var accountBinding = await _accountDataProvider.GetThirdPartyByUserIdAsync(
                account.Id,
                record.Platform.ToString(),
                cancellationToken).ConfigureAwait(false);
            if (accountBinding is not null)
            {
                await _registrationStore.RestoreAsync(record, cancellationToken).ConfigureAwait(false);
                return Error<OAuthBindResponse>(OAuthError.BindingConflict);
            }

            await _accountDataProvider.LinkThirdPartyAsync(
                CreateBinding(account.Id, record),
                cancellationToken).ConfigureAwait(false);
            return new OAuthBindResponse
            {
                Data = GenerateJwt(
                    account.Id,
                    account.DisplayName,
                    account.TokenVersion,
                    twoFactorResult == TwoFactorLoginVerificationResult.Valid)
            };
        }
        catch
        {
            await _registrationStore.RestoreAsync(record, cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    private async Task<OAuthCallbackResponse> CreateRegistrationAsync(
        OAuthCallbackResponse response,
        OAuthCallbackData data,
        AppPlatformEnum platform,
        CancellationToken cancellationToken)
    {
        var result = await _registrationStore.CreateAsync(
            new OAuthRegistrationIdentity
            {
                Platform = platform,
                ProviderUserId = data.ProviderUserId,
                ProviderUserName = data.ProviderUserName,
                AccessToken = data.AccessToken,
                AccessTokenExpiresAt = data.AccessTokenExpiresAt,
                RefreshToken = data.RefreshToken,
                RefreshTokenExpiresAt = data.RefreshTokenExpiresAt,
                ExtraData = data.ExtraData
            },
            TimeSpan.FromSeconds(_oauthSetting.RegistrationTtlSeconds),
            cancellationToken).ConfigureAwait(false);
        if (result is not CreateOAuthRegistrationResult.Created created)
        {
            var error = result is CreateOAuthRegistrationResult.Unavailable
                ? OAuthError.RegistrationStoreUnavailable
                : OAuthError.ProviderFailure;
            return Error<OAuthCallbackResponse>(error);
        }

        data.AccessToken = string.Empty;
        data.RefreshToken = string.Empty;
        data.RegistrationCode = created.Code;
        return response;
    }

    private JwtToken GenerateJwt(
        Guid accountId,
        string displayName,
        int tokenVersion,
        bool isTwoFactorAuthenticated = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, displayName),
            new(ClaimTypes.NameIdentifier, accountId.ToString()),
            new("token_version", tokenVersion.ToString())
        };
        if (isTwoFactorAuthenticated)
        {
            claims.Add(new Claim(
                TokenClaimService.TwoFactorAuthenticationClaim,
                TokenClaimService.TwoFactorAuthenticationValue));
        }
        return new JwtToken
        {
            AccessToken = _tokenClaimService.GenerateAccessToken(claims),
            RefreshToken = _tokenClaimService.GenerateRefreshToken(claims)
        };
    }

    private static UserThirdPartyDto CreateBinding(Guid accountId, OAuthRegistrationRecord record)
    {
        return new UserThirdPartyDto
        {
            Id = Guid.NewGuid(),
            UserId = accountId,
            Provider = record.Platform.ToString(),
            ThirdPartyUserId = record.ProviderUserId,
            AccessToken = record.AccessToken,
            AccessTokenExpiresAt = record.AccessTokenExpiresAt,
            RefreshToken = record.RefreshToken,
            RefreshTokenExpiresAt = record.RefreshTokenExpiresAt,
            ExtraData = record.ExtraData
        };
    }

    private static TResponse Error<TResponse>(OAuthError error)
        where TResponse : VoidBotResponse, new()
    {
        return new TResponse
        {
            Code = error.StatusCode,
            Message = error.Code
        };
    }

    private IOAuthProvider GetProvider(AppPlatformEnum platform)
    {
        return _providers.TryGetValue(platform, out var provider)
            ? provider
            : throw new NotSupportedException($"OAuth provider '{platform}' is not supported.");
    }
}
