using System.Security.Claims;
using System.Net;
using Serilog;
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

    private readonly IOAuthStateStore _stateStore;
    private readonly IDiscordService _discordService;
    private readonly OAuthSetting _oauthSetting;
    private readonly IAccountService _accountService;
    private readonly IAccountDataProvider _accountDataProvider;
    private readonly ITokenClaimService _tokenClaimService;
    private readonly ILogger _logger;

    public DiscordOAuthProvider(
        IOAuthStateStore stateStore,
        IDiscordService discordService,
        OAuthSetting setting,
        IAccountService accountService,
        IAccountDataProvider accountDataProvider,
        ITokenClaimService tokenClaimService,
        ILogger logger)
    {
        _stateStore = stateStore;
        _discordService = discordService;
        _oauthSetting = setting;
        _accountService = accountService;
        _accountDataProvider = accountDataProvider;
        _tokenClaimService = tokenClaimService;
        _logger = logger;
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
        _logger.Information(
            "Starting OAuth authorization: {Platform}, flow {Flow}, account {AccountId}",
            Platform,
            flow,
            authenticatedAccountId);

        var result = await _stateStore.CreateAsync(
            new CreateOAuthStateRequest(
                Platform,
                flow,
                TimeSpan.FromSeconds(_oauthSetting.StateTtlSeconds),
                authenticatedAccountId),
            cancellationToken).ConfigureAwait(false);

        if (result is CreateOAuthStateResponse.Created created)
        {
            _logger.Information(
                "OAuth authorization created: {Platform}, flow {Flow}, expires at {ExpiresAt}",
                Platform,
                flow,
                created.Record.ExpiresAt);
            return new OAuthAuthorizeResponse
            {
                Data = new OAuthAuthorizeData
                {
                    Platform = DiscordProvider,
                    RedirectUrl = _discordService.GenerateOAuthAuthorizationUrl(created.Record.State)
                }
            };
        }

        if (result is CreateOAuthStateResponse.Unavailable)
        {
            _logger.Warning(
                "OAuth authorization failed because the state store is unavailable: {Platform}, flow {Flow}",
                Platform,
                flow);
            return Error<OAuthAuthorizeResponse>(OAuthError.StateStoreUnavailable);
        }

        _logger.Warning(
            "OAuth authorization rejected an invalid state request: {Platform}, flow {Flow}, account {AccountId}",
            Platform,
            flow,
            authenticatedAccountId);
        return Error<OAuthAuthorizeResponse>(OAuthError.InvalidState);
    }

    public async Task<OAuthCallbackResponse> CallbackAsync(
        OAuthCallbackContext context,
        CancellationToken cancellationToken)
    {
        _logger.Information(
            "Processing OAuth callback: {Platform}, has code {HasCode}, has provider error {HasProviderError}",
            Platform,
            !string.IsNullOrWhiteSpace(context.Code),
            !string.IsNullOrWhiteSpace(context.Error));

        var stateResult = await _stateStore
            .TakeAsync(new TakeOAuthStateRequest(context.State), cancellationToken)
            .ConfigureAwait(false);
        if (stateResult is TakeOAuthStateResponse.Unavailable)
        {
            _logger.Warning("OAuth callback failed because the state store is unavailable: {Platform}", Platform);
            return Error<OAuthCallbackResponse>(OAuthError.StateStoreUnavailable);
        }

        if (stateResult is not TakeOAuthStateResponse.Found found)
        {
            _logger.Warning("OAuth callback rejected an invalid or expired state: {Platform}", Platform);
            return Error<OAuthCallbackResponse>(OAuthError.InvalidState);
        }

        if (found.Record.Platform != Platform)
        {
            _logger.Warning(
                "OAuth callback state platform mismatch: expected {ExpectedPlatform}, actual {ActualPlatform}",
                Platform,
                found.Record.Platform);
            return Error<OAuthCallbackResponse>(OAuthError.InvalidState);
        }

        if (!string.IsNullOrWhiteSpace(context.Error))
        {
            _logger.Information(
                "OAuth callback reported denied consent: {Platform}, flow {Flow}",
                Platform,
                found.Record.Flow);
            return Error<OAuthCallbackResponse>(OAuthError.ConsentDenied);
        }

        if (string.IsNullOrWhiteSpace(context.Code))
        {
            _logger.Warning(
                "OAuth callback did not include an authorization code: {Platform}, flow {Flow}",
                Platform,
                found.Record.Flow);
            return Error<OAuthCallbackResponse>(OAuthError.ProviderFailure);
        }

        var identityResult = await _discordService
            .AuthenticateOAuthAsync(new AuthenticateDiscordOAuthRequest(context.Code), cancellationToken)
            .ConfigureAwait(false);
        if (identityResult is not AuthenticateDiscordOAuthResponse.Succeeded succeeded)
        {
            _logger.Warning(
                "OAuth provider authentication failed: {Platform}, flow {Flow}",
                Platform,
                found.Record.Flow);
            return Error<OAuthCallbackResponse>(OAuthError.ProviderFailure);
        }

        _logger.Information(
            "OAuth provider identity authenticated: {Platform}, flow {Flow}, provider user {ProviderUserId}",
            Platform,
            found.Record.Flow,
            succeeded.Identity.UserId);

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
        _logger.Information(
            "Starting OAuth unlink: {Platform}, account {AccountId}",
            Platform,
            authenticatedAccountId);

        if (authenticatedAccountId == Guid.Empty)
        {
            _logger.Warning("OAuth unlink requires authentication: {Platform}", Platform);
            return Error<OAuthUnlinkResponse>(OAuthError.AuthenticationRequired);
        }

        var binding = await _accountDataProvider.GetThirdPartyByUserIdAsync(
            authenticatedAccountId,
            DiscordProvider,
            cancellationToken).ConfigureAwait(false);
        if (binding is null)
        {
            _logger.Information(
                "OAuth unlink found no binding: {Platform}, account {AccountId}",
                Platform,
                authenticatedAccountId);
            return Error<OAuthUnlinkResponse>(OAuthError.BindingNotFound);
        }

        await _accountService.UnlinkAccountFromThirdPartyAsync(
            new UnlinkUserAccountFromThirdPartyCommand
            {
                UserId = authenticatedAccountId,
                Platform = AppPlatformEnum.Discord
            },
            cancellationToken).ConfigureAwait(false);

        _logger.Information(
            "OAuth unlink succeeded: {Platform}, account {AccountId}, provider user {ProviderUserId}",
            Platform,
            authenticatedAccountId,
            binding.ThirdPartyUserId);
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
            _logger.Information(
                "OAuth login succeeded with an existing binding: {Platform}, account {AccountId}, provider user {ProviderUserId}",
                Platform,
                account.Id,
                identity.UserId);
            return GenerateJwt(account.Id, account.DisplayName, identity, OAuthStateFlow.Login);
        }

        _logger.Information(
            "OAuth login requires Void account registration or binding: {Platform}, provider user {ProviderUserId}",
            Platform,
            identity.UserId);
        return RegistrationRequired(identity);
    }

    private async Task<OAuthCallbackResponse> LinkAsync(
        Guid? initiatingAccountId,
        DiscordOAuthIdentity identity,
        CancellationToken cancellationToken)
    {
        if (!initiatingAccountId.HasValue || initiatingAccountId.Value == Guid.Empty)
        {
            _logger.Warning(
                "OAuth link rejected a missing initiating account: {Platform}, provider user {ProviderUserId}",
                Platform,
                identity.UserId);
            return Error<OAuthCallbackResponse>(OAuthError.InvalidState);
        }

        var account = await _accountDataProvider.GetUserAccountAsync(
            initiatingAccountId.Value,
            string.Empty,
            cancellationToken).ConfigureAwait(false);
        if (account is null)
        {
            _logger.Warning(
                "OAuth link account was not found: {Platform}, account {AccountId}, provider user {ProviderUserId}",
                Platform,
                initiatingAccountId.Value,
                identity.UserId);
            return Error<OAuthCallbackResponse>(OAuthError.AuthorizationForbidden);
        }

        var identityBinding = await _accountDataProvider.GetThirdPartyBindingAsync(
            DiscordProvider,
            identity.UserId,
            cancellationToken).ConfigureAwait(false);
        if (identityBinding is not null && identityBinding.UserId != initiatingAccountId.Value)
        {
            _logger.Warning(
                "OAuth link conflict because the provider identity belongs to another account: {Platform}, account {AccountId}, provider user {ProviderUserId}",
                Platform,
                initiatingAccountId.Value,
                identity.UserId);
            return Error<OAuthCallbackResponse>(OAuthError.BindingConflict);
        }

        var accountBinding = await _accountDataProvider.GetThirdPartyByUserIdAsync(
            initiatingAccountId.Value,
            DiscordProvider,
            cancellationToken).ConfigureAwait(false);
        if (accountBinding is not null && accountBinding.ThirdPartyUserId != identity.UserId)
        {
            _logger.Warning(
                "OAuth link conflict because the account has another provider identity: {Platform}, account {AccountId}, provider user {ProviderUserId}",
                Platform,
                initiatingAccountId.Value,
                identity.UserId);
            return Error<OAuthCallbackResponse>(OAuthError.BindingConflict);
        }

        var binding = identityBinding ?? accountBinding;
        if (binding is null)
        {
            await _accountDataProvider.LinkThirdPartyAsync(
                CreateBinding(initiatingAccountId.Value, identity),
                cancellationToken).ConfigureAwait(false);
            _logger.Information(
                "OAuth link created a new binding: {Platform}, account {AccountId}, provider user {ProviderUserId}",
                Platform,
                initiatingAccountId.Value,
                identity.UserId);
        }
        else
        {
            UpdateBindingTokens(binding, identity);
            await _accountDataProvider.UpdateThirdPartyAsync(binding, cancellationToken).ConfigureAwait(false);
            _logger.Information(
                "OAuth link refreshed an existing binding: {Platform}, account {AccountId}, provider user {ProviderUserId}",
                Platform,
                initiatingAccountId.Value,
                identity.UserId);
        }

        return GenerateJwt(account.Id, account.DisplayName, identity, OAuthStateFlow.Link);
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
        DiscordOAuthIdentity identity,
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
                Platform = DiscordProvider,
                Flow = flow,
                ProviderUserId = identity.UserId,
                ProviderUserName = identity.DisplayName,
                AccessToken = flow == OAuthStateFlow.Login
                    ? _tokenClaimService.GenerateAccessToken(claims)
                    : string.Empty,
                RefreshToken = flow == OAuthStateFlow.Login
                    ? _tokenClaimService.GenerateRefreshToken(claims)
                    : string.Empty
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

    private static OAuthCallbackResponse RegistrationRequired(DiscordOAuthIdentity identity)
    {
        return new OAuthCallbackResponse
        {
            Data = new OAuthCallbackData
            {
                Platform = DiscordProvider,
                Flow = OAuthStateFlow.Register,
                ProviderUserId = identity.UserId,
                ProviderUserName = identity.DisplayName,
                AccessToken = identity.AccessToken,
                AccessTokenExpiresAt = identity.ExpiresAt,
                RefreshToken = identity.RefreshToken ?? string.Empty
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
