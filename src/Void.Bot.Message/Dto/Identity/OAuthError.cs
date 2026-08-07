using System.Net;

namespace Void.Bot.Message.Dto.Identity;

public sealed record OAuthError(string Code, HttpStatusCode StatusCode)
{
    public static OAuthError InvalidState { get; } = new("invalid_oauth_state", HttpStatusCode.BadRequest);
    public static OAuthError ConsentDenied { get; } = new("consent_denied", HttpStatusCode.BadRequest);
    public static OAuthError AuthenticationRequired { get; } = new("authentication_required", HttpStatusCode.Unauthorized);
    public static OAuthError AuthorizationForbidden { get; } = new("authorization_forbidden", HttpStatusCode.Forbidden);
    public static OAuthError BindingConflict { get; } = new("binding_conflict", HttpStatusCode.Conflict);
    public static OAuthError BindingNotFound { get; } = new("binding_not_found", HttpStatusCode.NotFound);
    public static OAuthError ProviderFailure { get; } = new("provider_failure", HttpStatusCode.BadGateway);
    public static OAuthError StateStoreUnavailable { get; } = new("oauth_state_store_unavailable", HttpStatusCode.ServiceUnavailable);
}
