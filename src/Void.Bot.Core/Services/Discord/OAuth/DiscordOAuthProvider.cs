using System.Security.Claims;
using System.Security.Cryptography;
using System.Net;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Identity.OAuth;
using Void.Bot.Core.Settings.OAuth;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Commands.Identity;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Dto.Discord.OAuth;
using Void.Bot.Message.Dto.Identity;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Identity;
using Void.Bot.Message.Request.Discord.OAuth;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Core.Services.Discord.OAuth;

public sealed class DiscordOAuthProvider : IOAuthProvider
{
    private const string DiscordProvider = nameof(AppPlatformEnum.Discord);
    private const int AccountSecretByteLength = 32;

    private readonly IOAuthStateStore _stateStore;
    private readonly IDiscordService _discordService;
    private readonly OAuthSetting _oauthSetting;
    private readonly IAccountService _accountService;
    private readonly IAccountDataProvider _accountDataProvider;
    private readonly ITokenClaimService _tokenClaimService;

    public DiscordOAuthProvider(
        IOAuthStateStore stateStore,
        IDiscordService discordService,
        OAuthSetting setting,
        IAccountService accountService,
        IAccountDataProvider accountDataProvider,
        ITokenClaimService tokenClaimService)
    {
        _stateStore = stateStore;
        _discordService = discordService;
        _oauthSetting = setting;
        _accountService = accountService;
        _accountDataProvider = accountDataProvider;
        _tokenClaimService = tokenClaimService;
    }

    public AppPlatformEnum Platform => AppPlatformEnum.Discord;

    public OAuthErrorResponse CreateProviderFailure()
    {
        return Error<OAuthErrorResponse>(OAuthError.ProviderFailure);
    }

    public async Task<OAuthAuthorizeResponse> AuthorizeAsync(
        Guid? authenticatedAccountId,
        CancellationToken cancellationToken)
    {
        var flow = authenticatedAccountId.HasValue ? OAuthStateFlow.Link : OAuthStateFlow.Login;
        var result = await _stateStore.CreateAsync(
            new CreateOAuthStateRequest(
                Platform,
                flow,
                TimeSpan.FromSeconds(_oauthSetting.StateTtlSeconds),
                authenticatedAccountId),
            cancellationToken).ConfigureAwait(false);

        return result switch
        {
            CreateOAuthStateResponse.Created created => new OAuthAuthorizeResponse
            {
                Data = new OAuthAuthorizeData
                {
                    Platform = DiscordProvider,
                    RedirectUrl = _discordService.GenerateOAuthAuthorizationUrl(created.Record.State)
                }
            },
            CreateOAuthStateResponse.Unavailable => Error<OAuthAuthorizeResponse>(OAuthError.StateStoreUnavailable),
            _ => Error<OAuthAuthorizeResponse>(OAuthError.InvalidState)
        };
    }

    public async Task<OAuthCallbackResponse> CallbackAsync(
        OAuthCallbackContext context,
        CancellationToken cancellationToken)
    {
        var stateResult = await _stateStore
            .TakeAsync(new TakeOAuthStateRequest(context.State), cancellationToken)
            .ConfigureAwait(false);
        if (stateResult is TakeOAuthStateResponse.Unavailable)
        {
            return Error<OAuthCallbackResponse>(OAuthError.StateStoreUnavailable);
        }

        if (stateResult is not TakeOAuthStateResponse.Found found)
        {
            return Error<OAuthCallbackResponse>(OAuthError.InvalidState);
        }

        if (found.Record.Platform != Platform)
        {
            return Error<OAuthCallbackResponse>(OAuthError.InvalidState);
        }

        if (!string.IsNullOrWhiteSpace(context.Error))
        {
            return Error<OAuthCallbackResponse>(OAuthError.ConsentDenied);
        }

        if (string.IsNullOrWhiteSpace(context.Code))
        {
            return Error<OAuthCallbackResponse>(OAuthError.ProviderFailure);
        }

        var identityResult = await _discordService
            .AuthenticateOAuthAsync(new AuthenticateDiscordOAuthRequest(context.Code), cancellationToken)
            .ConfigureAwait(false);
        if (identityResult is not AuthenticateDiscordOAuthResponse.Succeeded succeeded)
        {
            return Error<OAuthCallbackResponse>(OAuthError.ProviderFailure);
        }

        return found.Record.Flow switch
        {
            OAuthStateFlow.Login => await LoginAsync(succeeded.Identity, cancellationToken).ConfigureAwait(false),
            OAuthStateFlow.Link => await LinkAsync(
                found.Record.InitiatingAccountId,
                succeeded.Identity,
                cancellationToken).ConfigureAwait(false),
            _ => Error<OAuthCallbackResponse>(OAuthError.InvalidState)
        };
    }

    public async Task<OAuthUnlinkResponse> UnlinkAsync(
        Guid authenticatedAccountId,
        CancellationToken cancellationToken)
    {
        if (authenticatedAccountId == Guid.Empty)
        {
            return Error<OAuthUnlinkResponse>(OAuthError.AuthenticationRequired);
        }

        var binding = await _accountDataProvider.GetThirdPartyByUserIdAsync(
            authenticatedAccountId,
            DiscordProvider,
            cancellationToken).ConfigureAwait(false);
        if (binding is null)
        {
            return Error<OAuthUnlinkResponse>(OAuthError.BindingNotFound);
        }

        await _accountService.UnlinkAccountFromThirdPartyAsync(
            new UnlinkUserAccountFromThirdPartyCommand
            {
                UserId = authenticatedAccountId,
                Platform = AppPlatformEnum.Discord
            },
            cancellationToken).ConfigureAwait(false);

        return new OAuthUnlinkResponse();
    }

    private async Task<OAuthCallbackResponse> LoginAsync(
        DiscordOAuthIdentity identity,
        CancellationToken cancellationToken)
    {
        var binding = await _accountDataProvider.GetThirdPartyBindingAsync(
            DiscordProvider,
            identity.UserId,
            cancellationToken).ConfigureAwait(false);
        if (binding is not null)
        {
            var account = await GetAccountAsync(binding.UserId, cancellationToken).ConfigureAwait(false);
            UpdateBindingTokens(binding, identity);
            await _accountDataProvider.UpdateThirdPartyAsync(binding, cancellationToken).ConfigureAwait(false);
            return GenerateJwt(account.Id, account.DisplayName, identity);
        }

        var registration = await _accountService.RegisterUserAccountAsync(
            new RegisterUserAccountCommand
            {
                Name = identity.DisplayName,
                Password = GenerateAccountSecret()
            },
            cancellationToken).ConfigureAwait(false);
        var createdAccount = registration.Data ??
            throw new InvalidOperationException("Unable to create the account for the Discord identity.");

        await _accountDataProvider.LinkThirdPartyAsync(
            CreateBinding(createdAccount.Id, identity),
            cancellationToken).ConfigureAwait(false);

        return GenerateJwt(createdAccount.Id, createdAccount.DisplayName, identity);
    }

    private async Task<OAuthCallbackResponse> LinkAsync(
        Guid? initiatingAccountId,
        DiscordOAuthIdentity identity,
        CancellationToken cancellationToken)
    {
        if (!initiatingAccountId.HasValue || initiatingAccountId.Value == Guid.Empty)
        {
            return Error<OAuthCallbackResponse>(OAuthError.InvalidState);
        }

        var account = await _accountDataProvider.GetUserAccountAsync(
            initiatingAccountId.Value,
            string.Empty,
            cancellationToken).ConfigureAwait(false);
        if (account is null)
        {
            return Error<OAuthCallbackResponse>(OAuthError.AuthorizationForbidden);
        }

        var identityBinding = await _accountDataProvider.GetThirdPartyBindingAsync(
            DiscordProvider,
            identity.UserId,
            cancellationToken).ConfigureAwait(false);
        if (identityBinding is not null && identityBinding.UserId != initiatingAccountId.Value)
        {
            return Error<OAuthCallbackResponse>(OAuthError.BindingConflict);
        }

        var accountBinding = await _accountDataProvider.GetThirdPartyByUserIdAsync(
            initiatingAccountId.Value,
            DiscordProvider,
            cancellationToken).ConfigureAwait(false);
        if (accountBinding is not null && accountBinding.ThirdPartyUserId != identity.UserId)
        {
            return Error<OAuthCallbackResponse>(OAuthError.BindingConflict);
        }

        var binding = identityBinding ?? accountBinding;
        if (binding is null)
        {
            await _accountDataProvider.LinkThirdPartyAsync(
                CreateBinding(initiatingAccountId.Value, identity),
                cancellationToken).ConfigureAwait(false);
        }
        else
        {
            UpdateBindingTokens(binding, identity);
            await _accountDataProvider.UpdateThirdPartyAsync(binding, cancellationToken).ConfigureAwait(false);
        }

        return GenerateJwt(account.Id, account.DisplayName, identity);
    }

    private async Task<UserAccountDto> GetAccountAsync(Guid accountId, CancellationToken cancellationToken)
    {
        return await _accountDataProvider.GetUserAccountAsync(
                   accountId,
                   string.Empty,
                   cancellationToken).ConfigureAwait(false) ??
               throw new InvalidOperationException("The Discord binding owner account was not found.");
    }

    private OAuthCallbackResponse GenerateJwt(
        Guid accountId,
        string displayName,
        DiscordOAuthIdentity identity)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, displayName),
            new(ClaimTypes.NameIdentifier, accountId.ToString())
        };

        return new OAuthCallbackResponse
        {
            Data = new OAuthCallbackData
            {
                Platform = DiscordProvider,
                ProviderUserId = identity.UserId,
                ProviderUserName = identity.DisplayName,
                AccessToken = _tokenClaimService.GenerateAccessToken(claims),
                RefreshToken = _tokenClaimService.GenerateRefreshToken()
            }
        };
    }

    private static UserThirdPartyDto CreateBinding(Guid accountId, DiscordOAuthIdentity identity)
    {
        return new UserThirdPartyDto
        {
            Id = Guid.NewGuid(),
            UserId = accountId,
            Provider = DiscordProvider,
            ThirdPartyUserId = identity.UserId,
            AccessToken = identity.AccessToken,
            AccessTokenExpiresAt = identity.ExpiresAt,
            RefreshToken = identity.RefreshToken ?? string.Empty
        };
    }

    private static void UpdateBindingTokens(
        UserThirdPartyDto binding,
        DiscordOAuthIdentity identity)
    {
        binding.AccessToken = identity.AccessToken;
        binding.AccessTokenExpiresAt = identity.ExpiresAt;
        binding.RefreshToken = identity.RefreshToken ?? string.Empty;
        binding.RefreshTokenExpiresAt = null;
    }

    private static string GenerateAccountSecret()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(AccountSecretByteLength))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static TResponse Error<TResponse>(OAuthError error)
        where TResponse : VoidBotResponse, IThirdPartyOAuthResponse, new()
    {
        return new TResponse
        {
            Code = error.StatusCode,
            Message = error.Code
        };
    }
}
