using System.Net;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Settings.Email;
using Void.Bot.Message.Dto.Email;

namespace Void.Bot.Core.Services.Email;

internal interface IMicrosoftGraphClient : IScope
{
    string CreateAuthorizationUrl(EmailAuthorizationStateRecord state);

    Task<EmailTokenResult> ExchangeAuthorizationCodeAsync(
        string code,
        string codeVerifier,
        CancellationToken cancellationToken);

    Task<EmailTokenResult> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken);

    Task<EmailIdentityResult> ValidateIdentityAsync(
        string idToken,
        string expectedNonce,
        CancellationToken cancellationToken);

    Task SendAsync(
        string accessToken,
        EmailMessageDto message,
        CancellationToken cancellationToken);
}

internal sealed class MicrosoftGraphClient(
    IHttpClientFactory httpClientFactory,
    EmailSetting setting) : IMicrosoftGraphClient
{
    private const string Scope = "openid profile email offline_access Mail.Send";
    private const string GraphSendMailUrl = "https://graph.microsoft.com/v1.0/me/sendMail";
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _openidConfigurationManager = new(
        $"https://login.microsoftonline.com/{Uri.EscapeDataString(setting.Tenant)}/v2.0/.well-known/openid-configuration",
        new OpenIdConnectConfigurationRetriever());

    public string CreateAuthorizationUrl(EmailAuthorizationStateRecord state)
    {
        ValidateConfiguration();

        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = setting.ClientId,
            ["response_type"] = "code",
            ["redirect_uri"] = setting.RedirectUri,
            ["response_mode"] = "query",
            ["scope"] = Scope,
            ["state"] = state.State,
            ["nonce"] = state.Nonce,
            ["code_challenge"] = EmailAuthorizationStateStore.CreateCodeChallenge(state.CodeVerifier),
            ["code_challenge_method"] = "S256"
        };
        if (state.ForceReauthorization)
        {
            parameters["prompt"] = "select_account";
        }

        var query = string.Join("&", parameters.Select(pair =>
            $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));
        return $"https://login.microsoftonline.com/{Uri.EscapeDataString(setting.Tenant)}/oauth2/v2.0/authorize?{query}";
    }

    public Task<EmailTokenResult> ExchangeAuthorizationCodeAsync(
        string code,
        string codeVerifier,
        CancellationToken cancellationToken)
    {
        return RequestTokenAsync(
            new Dictionary<string, string>
            {
                ["client_id"] = setting.ClientId,
                ["client_secret"] = setting.ClientSecret,
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = setting.RedirectUri,
                ["code_verifier"] = codeVerifier,
                ["scope"] = Scope
            },
            null,
            cancellationToken);
    }

    public Task<EmailTokenResult> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        return RequestTokenAsync(
            new Dictionary<string, string>
            {
                ["client_id"] = setting.ClientId,
                ["client_secret"] = setting.ClientSecret,
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["scope"] = Scope
            },
            refreshToken,
            cancellationToken);
    }

    public async Task<EmailIdentityResult> ValidateIdentityAsync(
        string idToken,
        string expectedNonce,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new EmailServiceException("The email provider did not return an identity token.");
        }

        try
        {
            var configuration = await _openidConfigurationManager
                .GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
            var principal = new JwtSecurityTokenHandler { MapInboundClaims = false }.ValidateToken(
                idToken,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = configuration.SigningKeys,
                    ValidateIssuer = true,
                    ValidIssuer = configuration.Issuer,
                    ValidateAudience = true,
                    ValidAudience = setting.ClientId,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
                },
                out _);

            var accountId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                            principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var nonce = principal.FindFirstValue("nonce");
            var email = principal.FindFirstValue("preferred_username") ??
                        principal.FindFirstValue(ClaimTypes.Email) ??
                        principal.FindFirstValue(JwtRegisteredClaimNames.Email);
            if (string.IsNullOrWhiteSpace(accountId) || string.IsNullOrWhiteSpace(email) ||
                !string.Equals(nonce, expectedNonce, StringComparison.Ordinal))
            {
                throw new EmailServiceException("The authorized email account identity is incomplete.");
            }

            return new EmailIdentityResult { AccountId = accountId, Email = email };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException exception)
        {
            throw CreateTransportException(exception);
        }
        catch (HttpRequestException exception)
        {
            throw CreateTransportException(exception);
        }
        catch (InvalidOperationException exception)
            when (exception.GetBaseException() is HttpRequestException or IOException)
        {
            throw CreateTransportException(exception);
        }
        catch (EmailServiceException)
        {
            throw;
        }
        catch (Exception exception) when (exception is SecurityTokenException or InvalidOperationException)
        {
            throw new EmailServiceException("The email provider returned an invalid identity token.", exception);
        }
    }

    public async Task SendAsync(
        string accessToken,
        EmailMessageDto message,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message.To);
        ArgumentException.ThrowIfNullOrWhiteSpace(message.Subject);
        if (string.IsNullOrWhiteSpace(message.TextContent) && string.IsNullOrWhiteSpace(message.HtmlContent))
        {
            throw new ArgumentException("Email content cannot be empty.", nameof(message));
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, GraphSendMailUrl);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(CreateSendMailRequest(message));

        HttpResponseMessage response;
        try
        {
            response = await httpClientFactory.CreateClient()
                .SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException exception)
        {
            throw CreateTransportException(exception);
        }
        catch (HttpRequestException exception)
        {
            throw CreateTransportException(exception);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                return;
            }

            throw await CreateProviderExceptionAsync(response, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<EmailTokenResult> RequestTokenAsync(
        Dictionary<string, string> parameters,
        string? fallbackRefreshToken,
        CancellationToken cancellationToken)
    {
        ValidateConfiguration();

        using var content = new FormUrlEncodedContent(parameters);
        var url = $"https://login.microsoftonline.com/{Uri.EscapeDataString(setting.Tenant)}/oauth2/v2.0/token";
        HttpResponseMessage response;
        try
        {
            response = await httpClientFactory.CreateClient()
                .PostAsync(url, content, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException exception)
        {
            throw CreateTransportException(exception);
        }
        catch (HttpRequestException exception)
        {
            throw CreateTransportException(exception);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw await CreateProviderExceptionAsync(response, cancellationToken).ConfigureAwait(false);
            }

            var token = await response.Content.ReadFromJsonAsync<GraphTokenResponse>(
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (token is null || string.IsNullOrWhiteSpace(token.AccessToken) ||
                string.IsNullOrWhiteSpace(token.RefreshToken ?? fallbackRefreshToken))
            {
                throw new EmailServiceException("The email provider returned an invalid token response.");
            }

            return new EmailTokenResult
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken ?? fallbackRefreshToken!,
                IdToken = token.IdToken ?? string.Empty,
                AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(token.ExpiresIn, 60))
            };
        }
    }

    private static EmailProviderUnavailableException CreateTransportException(Exception exception)
    {
        return new EmailProviderUnavailableException(
            exception is OperationCanceledException
                ? "email_provider_timeout"
                : "email_provider_unavailable",
            exception);
    }

    private static object CreateSendMailRequest(EmailMessageDto message)
    {
        var bodyType = string.IsNullOrWhiteSpace(message.HtmlContent) ? "Text" : "HTML";
        var bodyContent = bodyType == "HTML" ? message.HtmlContent : message.TextContent;
        var graphMessage = new Dictionary<string, object?>
        {
            ["subject"] = message.Subject,
            ["body"] = new { contentType = bodyType, content = bodyContent },
            ["toRecipients"] = new[]
            {
                new { emailAddress = new { address = message.To } }
            }
        };
        if (!string.IsNullOrWhiteSpace(message.ReplyTo))
        {
            graphMessage["replyTo"] = new[]
            {
                new { emailAddress = new { address = message.ReplyTo } }
            };
        }

        return new { message = graphMessage, saveToSentItems = true };
    }

    private static async Task<EmailServiceException> CreateProviderExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string? errorCode = null;
        try
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (root.TryGetProperty("error", out var error))
            {
                errorCode = error.ValueKind == JsonValueKind.String
                    ? error.GetString()
                    : error.TryGetProperty("code", out var code) ? code.GetString() : null;
            }
        }
        catch (JsonException)
        {
        }

        if (string.Equals(errorCode, "invalid_grant", StringComparison.OrdinalIgnoreCase))
        {
            return new EmailReauthorizationRequiredException(errorCode, response.StatusCode);
        }

        return new EmailServiceException(
            $"The email provider rejected the request with status {(int)response.StatusCode}.",
            providerErrorCode: errorCode,
            providerStatusCode: response.StatusCode);
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(setting.ClientId) ||
            string.IsNullOrWhiteSpace(setting.ClientSecret) ||
            string.IsNullOrWhiteSpace(setting.RedirectUri))
        {
            throw new EmailServiceException("Email OAuth configuration is incomplete.");
        }
    }

    private sealed class GraphTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; init; }

        [JsonPropertyName("id_token")]
        public string? IdToken { get; init; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }
    }
}
