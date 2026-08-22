using System.Net;
using System.Security.Claims;
using Serilog;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Identity.OAuth;
using Void.Bot.Core.Settings.OAuth;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Commands.Bungie;
using Void.Bot.Message.Commands.Identity;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Dto.Bungie;
using Void.Bot.Message.Dto.Identity;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Bungie;
using Void.Bot.Message.Enum.Identity;
using Void.Bot.Message.Request.Bungie.Destiny2;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Core.Services.Bungie.OAuth;

public sealed class BungieOAuthProvider : IOAuthProvider
{
    private const string BungieProvider = nameof(AppPlatformEnum.Bungie);

    private readonly IOAuthStateStore _stateStore;
    private readonly IBungieService _bungieService;
    private readonly OAuthSetting _oauthSetting;
    private readonly IAccountService _accountService;
    private readonly IAccountDataProvider _accountDataProvider;
    private readonly ITokenClaimService _tokenClaimService;
    private readonly ILogger _logger;

    public BungieOAuthProvider(
        IOAuthStateStore stateStore,
        IBungieService bungieService,
        OAuthSetting oauthSetting,
        IAccountService accountService,
        IAccountDataProvider accountDataProvider,
        ITokenClaimService tokenClaimService,
        ILogger logger)
    {
        _stateStore = stateStore;
        _bungieService = bungieService;
        _oauthSetting = oauthSetting;
        _accountService = accountService;
        _accountDataProvider = accountDataProvider;
        _tokenClaimService = tokenClaimService;
        _logger = logger;
    }

    public AppPlatformEnum Platform => AppPlatformEnum.Bungie;

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

        if (result is CreateOAuthStateResponse.Created created)
        {
            return new OAuthAuthorizeResponse
            {
                Data = new OAuthAuthorizeData
                {
                    Platform = BungieProvider,
                    RedirectUrl = _bungieService.GenerateOAuthAuthorizationUrl(created.Record.State)
                }
            };
        }

        return result is CreateOAuthStateResponse.Unavailable
            ? Error<OAuthAuthorizeResponse>(OAuthError.StateStoreUnavailable)
            : Error<OAuthAuthorizeResponse>(OAuthError.InvalidState);
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

        if (stateResult is not TakeOAuthStateResponse.Found found || found.Record.Platform != Platform)
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

        var tokenResponse = await _bungieService.GenerateTokenAsync(
            new GenerateBungieTokenCommand { OAuthCode = context.Code },
            cancellationToken).ConfigureAwait(false);
        var token = tokenResponse.Data;
        if (tokenResponse.Code != HttpStatusCode.OK || token is null ||
            string.IsNullOrWhiteSpace(token.MembershipId) ||
            !long.TryParse(token.MembershipId, out var membershipId))
        {
            _logger.Warning("Bungie OAuth token exchange failed");
            return Error<OAuthCallbackResponse>(OAuthError.ProviderFailure);
        }

        var profileResponse = await _bungieService.GetDestiny2LinkedProfileAsync(
            new GetDestiny2LinkedProfileRequest
            {
                MembershipType = BungieMembershipTypeEnum.BungieNext,
                MembershipId = membershipId
            },
            cancellationToken).ConfigureAwait(false);
        if (profileResponse.Data is null || profileResponse.Data.Response.Profiles.Count == 0)
        {
            _logger.Warning("Bungie OAuth identity lookup failed for membership {MembershipId}", token.MembershipId);
            return Error<OAuthCallbackResponse>(OAuthError.ProviderFailure);
        }

        var providerUserName = profileResponse.Data.Response.BungieNetMembershipDto.SupplementalDisplayName;
        if (string.IsNullOrWhiteSpace(providerUserName))
        {
            providerUserName = profileResponse.Data.Response.BungieNetMembershipDto.DisplayName;
        }

        return found.Record.Flow switch
        {
            OAuthStateFlow.Login => await LoginAsync(token, providerUserName, cancellationToken).ConfigureAwait(false),
            OAuthStateFlow.Link => await LinkAsync(
                found.Record.InitiatingAccountId,
                token,
                providerUserName,
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
            BungieProvider,
            cancellationToken).ConfigureAwait(false);
        if (binding is null)
        {
            return Error<OAuthUnlinkResponse>(OAuthError.BindingNotFound);
        }

        await _accountService.UnlinkAccountFromThirdPartyAsync(
            new UnlinkUserAccountFromThirdPartyCommand
            {
                UserId = authenticatedAccountId,
                Platform = Platform
            },
            cancellationToken).ConfigureAwait(false);

        return new OAuthUnlinkResponse();
    }

    private async Task<OAuthCallbackResponse> LoginAsync(
        BungieApiGenerateTokenResponse token,
        string providerUserName,
        CancellationToken cancellationToken)
    {
        var binding = await _accountDataProvider.GetThirdPartyBindingAsync(
            BungieProvider,
            token.MembershipId,
            cancellationToken).ConfigureAwait(false);
        if (binding is not null)
        {
            var account = await GetAccountAsync(binding.UserId, cancellationToken).ConfigureAwait(false);
            UpdateBindingTokens(binding, token);
            await _accountDataProvider.UpdateThirdPartyAsync(binding, cancellationToken).ConfigureAwait(false);
            return GenerateJwt(
                account.Id,
                account.DisplayName,
                token.MembershipId,
                providerUserName,
                OAuthStateFlow.Login);
        }

        return RegistrationRequired(
            token,
            providerUserName);
    }

    private async Task<OAuthCallbackResponse> LinkAsync(
        Guid? initiatingAccountId,
        BungieApiGenerateTokenResponse token,
        string providerUserName,
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
            BungieProvider,
            token.MembershipId,
            cancellationToken).ConfigureAwait(false);
        if (identityBinding is not null && identityBinding.UserId != initiatingAccountId.Value)
        {
            return Error<OAuthCallbackResponse>(OAuthError.BindingConflict);
        }

        var accountBinding = await _accountDataProvider.GetThirdPartyByUserIdAsync(
            initiatingAccountId.Value,
            BungieProvider,
            cancellationToken).ConfigureAwait(false);
        if (accountBinding is not null && accountBinding.ThirdPartyUserId != token.MembershipId)
        {
            return Error<OAuthCallbackResponse>(OAuthError.BindingConflict);
        }

        var binding = identityBinding ?? accountBinding;
        if (binding is null)
        {
            await _accountDataProvider.LinkThirdPartyAsync(
                CreateBinding(initiatingAccountId.Value, token),
                cancellationToken).ConfigureAwait(false);
        }
        else
        {
            UpdateBindingTokens(binding, token);
            await _accountDataProvider.UpdateThirdPartyAsync(binding, cancellationToken).ConfigureAwait(false);
        }

        return GenerateJwt(
            account.Id,
            account.DisplayName,
            token.MembershipId,
            providerUserName,
            OAuthStateFlow.Link);
    }

    private async Task<UserAccountDto> GetAccountAsync(Guid accountId, CancellationToken cancellationToken)
    {
        return await _accountDataProvider.GetUserAccountAsync(
                   accountId,
                   string.Empty,
                   cancellationToken).ConfigureAwait(false) ??
               throw new InvalidOperationException("The Bungie binding owner account was not found.");
    }

    private OAuthCallbackResponse GenerateJwt(
        Guid accountId,
        string displayName,
        string providerUserId,
        string providerUserName,
        OAuthStateFlow flow)
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
                Platform = BungieProvider,
                Flow = flow,
                ProviderUserId = providerUserId,
                ProviderUserName = providerUserName,
                AccessToken = flow == OAuthStateFlow.Login
                    ? _tokenClaimService.GenerateAccessToken(claims)
                    : string.Empty,
                RefreshToken = flow == OAuthStateFlow.Login
                    ? _tokenClaimService.GenerateRefreshToken(claims)
                    : string.Empty
            }
        };
    }

    private static UserThirdPartyDto CreateBinding(Guid accountId, BungieApiGenerateTokenResponse token)
    {
        var now = DateTimeOffset.Now;
        return new UserThirdPartyDto
        {
            Id = Guid.NewGuid(),
            UserId = accountId,
            Provider = BungieProvider,
            ThirdPartyUserId = token.MembershipId,
            AccessToken = token.AccessToken,
            AccessTokenExpiresAt = string.IsNullOrEmpty(token.AccessToken)
                ? null
                : now.AddSeconds(token.AccessTokenExpiresIn),
            RefreshToken = token.RefreshToken,
            RefreshTokenExpiresAt = string.IsNullOrEmpty(token.RefreshToken)
                ? null
                : now.AddSeconds(token.RefreshExpiresIn)
        };
    }

    private static void UpdateBindingTokens(
        UserThirdPartyDto binding,
        BungieApiGenerateTokenResponse token)
    {
        var now = DateTimeOffset.Now;
        binding.AccessToken = token.AccessToken;
        binding.AccessTokenExpiresAt = string.IsNullOrEmpty(token.AccessToken)
            ? null
            : now.AddSeconds(token.AccessTokenExpiresIn);
        binding.RefreshToken = token.RefreshToken;
        binding.RefreshTokenExpiresAt = string.IsNullOrEmpty(token.RefreshToken)
            ? null
            : now.AddSeconds(token.RefreshExpiresIn);
    }

    private static OAuthCallbackResponse RegistrationRequired(
        BungieApiGenerateTokenResponse token,
        string providerUserName)
    {
        var now = DateTimeOffset.Now;
        return new OAuthCallbackResponse
        {
            Data = new OAuthCallbackData
            {
                Platform = BungieProvider,
                Flow = OAuthStateFlow.Register,
                ProviderUserId = token.MembershipId,
                ProviderUserName = providerUserName,
                AccessToken = token.AccessToken,
                AccessTokenExpiresAt = string.IsNullOrEmpty(token.AccessToken)
                    ? null
                    : now.AddSeconds(token.AccessTokenExpiresIn),
                RefreshToken = token.RefreshToken,
                RefreshTokenExpiresAt = string.IsNullOrEmpty(token.RefreshToken)
                    ? null
                    : now.AddSeconds(token.RefreshExpiresIn)
            }
        };
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
