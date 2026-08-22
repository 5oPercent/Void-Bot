using Void.Bot.Core.Caching;
using Void.Bot.Core.Settings.OAuth;
using Void.Bot.Message.Commands.Identity;
using Void.Bot.Message.Dto.Identity;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Identity;

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
}

public sealed class OAuthService : IOAuthService
{
    private readonly IReadOnlyDictionary<AppPlatformEnum, IOAuthProvider> _providers;
    private readonly ICurrentUser _currentUser;
    private readonly IOAuthExchangeStore _exchangeStore;
    private readonly OAuthSetting _oauthSetting;

    public OAuthService(
        IEnumerable<IOAuthProvider> providers,
        ICurrentUser currentUser,
        IOAuthExchangeStore exchangeStore,
        OAuthSetting oauthSetting)
    {
        _providers = providers.ToDictionary(provider => provider.Platform);
        _currentUser = currentUser;
        _exchangeStore = exchangeStore;
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
        if (response.Data is not { Flow: OAuthStateFlow.Login } data)
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

    private IOAuthProvider GetProvider(AppPlatformEnum platform)
    {
        return _providers.TryGetValue(platform, out var provider)
            ? provider
            : throw new NotSupportedException($"OAuth provider '{platform}' is not supported.");
    }
}
