using Void.Bot.Core.Settings;
using Void.Bot.Message.Constants.Discord;
using Void.Bot.Message.Dto.Discord.OAuth;
using Void.Bot.Message.Dto.HttpClient;
using Void.Bot.Message.Request.Discord.OAuth;

namespace Void.Bot.Core.Services.Discord;

public partial interface IDiscordService
{
    string GenerateOAuthAuthorizationUrl(string state);

    Task<AuthenticateDiscordOAuthResponse> AuthenticateOAuthAsync(
        AuthenticateDiscordOAuthRequest request,
        CancellationToken cancellationToken = default);

    Task<RefreshDiscordOAuthTokenResponse> RefreshOAuthTokenAsync(
        RefreshDiscordOAuthTokenRequest request,
        CancellationToken cancellationToken = default);
}

public partial class DiscordService
{
    private enum OAuthFailure
    {
        TokenExchangeFailed,
        RefreshTokenFailed,
        InvalidTokenResponse,
        IdentityFetchFailed,
        InvalidIdentityResponse
    }

    private const string Scope = "identify";
    private const string CallbackPath = "/api/Identity/oauth/discord/callback";

    public string GenerateOAuthAuthorizationUrl(string state)
    {
        var redirectUri = AppSetting.DomainUrl + CallbackPath;
        return $"{DiscordConstants.AuthorizeUrl}?response_type=code&client_id={Uri.EscapeDataString(_setting.ClientId)}" +
               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={Scope}" +
               $"&state={Uri.EscapeDataString(state)}&prompt=consent";
    }

    public async Task<AuthenticateDiscordOAuthResponse> AuthenticateOAuthAsync(
        AuthenticateDiscordOAuthRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.Information("Starting Discord OAuth authorization-code exchange");
        var redirectUri = AppSetting.DomainUrl + CallbackPath;
        var tokenResult = await RequestTokenAsync(
            DiscordConstants.GenerateTokenUrl,
            new Dictionary<string, string>
            {
                ["client_id"] = _setting.ClientId,
                ["client_secret"] = _setting.ClientSecret,
                ["grant_type"] = "authorization_code",
                ["code"] = request.Code,
                ["redirect_uri"] = redirectUri
            },
            OAuthFailure.TokenExchangeFailed,
            null,
            cancellationToken).ConfigureAwait(false);
        if (tokenResult.AccessToken is null || tokenResult.ExpiresAt is null)
        {
            return new AuthenticateDiscordOAuthResponse.Failed();
        }

        return await GetIdentityAsync(
            tokenResult.AccessToken,
            tokenResult.RefreshToken,
            tokenResult.ExpiresAt.Value,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<RefreshDiscordOAuthTokenResponse> RefreshOAuthTokenAsync(
        RefreshDiscordOAuthTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.Information("Starting Discord OAuth token refresh");
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            _logger.Warning("Discord OAuth request failed: {Failure}", OAuthFailure.RefreshTokenFailed);
            return new RefreshDiscordOAuthTokenResponse.Failed();
        }

        var result = await RequestTokenAsync(
            DiscordConstants.RefreshTokenUrl,
            new Dictionary<string, string>
            {
                ["client_id"] = _setting.ClientId,
                ["client_secret"] = _setting.ClientSecret,
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = request.RefreshToken
            },
            OAuthFailure.RefreshTokenFailed,
            request.RefreshToken,
            cancellationToken).ConfigureAwait(false);

        return result.AccessToken is not null && result.ExpiresAt is not null
            ? new RefreshDiscordOAuthTokenResponse.Succeeded(
                result.AccessToken,
                result.RefreshToken,
                result.ExpiresAt.Value)
            : new RefreshDiscordOAuthTokenResponse.Failed();
    }

    private async Task<(
        string? AccessToken,
        string? RefreshToken,
        DateTimeOffset? ExpiresAt)> RequestTokenAsync(
        string url,
        Dictionary<string, string> parameters,
        OAuthFailure requestFailure,
        string? fallbackRefreshToken,
        CancellationToken cancellationToken)
    {
        try
        {
            using var content = new FormUrlEncodedContent(parameters);
            var response = await _httpClientFactory
                .PostAsync<DiscordOAuthTokenResponse>(
                    url,
                    content,
                    cancellationToken)
                .ConfigureAwait(false);

            if ((int)response.Code is < 200 or >= 300)
            {
                _logger.Warning(
                    "Discord OAuth request failed: {Failure}, HTTP status: {StatusCode}",
                    requestFailure,
                    response.Code);
                return (null, null, null);
            }

            var token = response.Data;
            if (token is null || string.IsNullOrWhiteSpace(token.AccessToken) || token.ExpiresIn is null or <= 0)
            {
                _logger.Warning("Discord OAuth request failed: {Failure}", OAuthFailure.InvalidTokenResponse);
                return (null, null, null);
            }

            try
            {
                var result = (
                    token.AccessToken,
                    string.IsNullOrWhiteSpace(token.RefreshToken) ? fallbackRefreshToken : token.RefreshToken,
                    DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn.Value));
                _logger.Information(
                    "Discord OAuth token request succeeded: {Operation}, expires at {ExpiresAt}, has refresh token {HasRefreshToken}",
                    requestFailure,
                    result.Item3,
                    !string.IsNullOrWhiteSpace(result.Item2));
                return result;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                _logger.Warning(exception, "Discord OAuth request failed: {Failure}", OAuthFailure.InvalidTokenResponse);
                return (null, null, null);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.Warning(exception, "Discord OAuth request failed: {Failure}", requestFailure);
            return (null, null, null);
        }
    }

    private async Task<AuthenticateDiscordOAuthResponse> GetIdentityAsync(
        string accessToken,
        string? refreshToken,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClientFactory
                .GetAsync<DiscordOAuthUserResponse>(
                    DiscordConstants.CurrentUserUrl,
                    cancellationToken,
                    tokenModel: new TokenModel { Parameter = accessToken })
                .ConfigureAwait(false);

            if ((int)response.Code is < 200 or >= 300)
            {
                _logger.Warning(
                    "Discord OAuth request failed: {Failure}, HTTP status: {StatusCode}",
                    OAuthFailure.IdentityFetchFailed,
                    response.Code);
                return new AuthenticateDiscordOAuthResponse.Failed();
            }

            var user = response.Data;
            var displayName = user?.GlobalName ?? user?.Username;
            if (string.IsNullOrWhiteSpace(user?.Id) || string.IsNullOrWhiteSpace(displayName))
            {
                _logger.Warning("Discord OAuth request failed: {Failure}", OAuthFailure.InvalidIdentityResponse);
                return new AuthenticateDiscordOAuthResponse.Failed();
            }

            _logger.Information(
                "Discord OAuth identity fetch succeeded: provider user {ProviderUserId}",
                user.Id);
            return new AuthenticateDiscordOAuthResponse.Succeeded(new DiscordOAuthIdentity(
                user.Id,
                displayName,
                accessToken,
                refreshToken,
                expiresAt));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.Warning(exception, "Discord OAuth request failed: {Failure}", OAuthFailure.IdentityFetchFailed);
            return new AuthenticateDiscordOAuthResponse.Failed();
        }
    }

}
