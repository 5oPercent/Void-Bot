using Void.Bot.Message.Commands.Identity;
using Void.Bot.Message.Dto.Identity;
using Void.Bot.Message.Enum;

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
}

public sealed class OAuthService : IOAuthService
{
    private readonly IReadOnlyDictionary<AppPlatformEnum, IOAuthProvider> _providers;
    private readonly ICurrentUser _currentUser;

    public OAuthService(IEnumerable<IOAuthProvider> providers, ICurrentUser currentUser)
    {
        _providers = providers.ToDictionary(provider => provider.Platform);
        _currentUser = currentUser;
    }

    public Task<OAuthAuthorizeResponse> AuthorizeAsync(
        AppPlatformEnum platform,
        CancellationToken cancellationToken)
    {
        return GetProvider(platform).AuthorizeAsync(_currentUser.Id, cancellationToken);
    }

    public Task<OAuthCallbackResponse> CallbackAsync(
        AppPlatformEnum platform,
        OAuthCallbackContext context,
        CancellationToken cancellationToken)
    {
        return GetProvider(platform).CallbackAsync(context, cancellationToken);
    }

    public Task<OAuthUnlinkResponse> UnlinkAsync(
        AppPlatformEnum platform,
        CancellationToken cancellationToken)
    {
        return GetProvider(platform).UnlinkAsync(_currentUser.Id ?? Guid.Empty, cancellationToken);
    }

    private IOAuthProvider GetProvider(AppPlatformEnum platform)
    {
        return _providers.TryGetValue(platform, out var provider)
            ? provider
            : throw new NotSupportedException($"OAuth provider '{platform}' is not supported.");
    }
}
