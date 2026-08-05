using Void.Bot.Message.Commands.Identity;
using Void.Bot.Message.Dto.Identity;
using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Services.Identity.OAuth;

public interface IOAuthProvider : IScope
{
    AppPlatformEnum Platform { get; }

    OAuthErrorResponse CreateProviderFailure();

    Task<OAuthAuthorizeResponse> AuthorizeAsync(
        Guid? authenticatedAccountId,
        CancellationToken cancellationToken);

    Task<OAuthCallbackResponse> CallbackAsync(
        OAuthCallbackContext context,
        CancellationToken cancellationToken);

    Task<OAuthUnlinkResponse> UnlinkAsync(
        Guid authenticatedAccountId,
        CancellationToken cancellationToken);
}
