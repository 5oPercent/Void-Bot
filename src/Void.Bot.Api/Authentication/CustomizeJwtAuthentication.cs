using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Void.Bot.Core.Settings.System;
using Void.Bot.Core.Services.Identity;

namespace Void.Bot.Api.Authentication;

public class CustomizeJwtAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string AuthenticationScheme = "Bearer";
    
    public const string SchemeHeader = "Authorization";
}

public class CustomizeJwtAuthenticationHandler : AuthenticationHandler<CustomizeJwtAuthenticationOptions>
{
    private readonly JwtSetting _jwtSetting;
    
    public CustomizeJwtAuthenticationHandler(
        IOptionsMonitor<CustomizeJwtAuthenticationOptions> options, ILoggerFactory logger,
        UrlEncoder encoder, JwtSetting jwtSetting) : base(options, logger, encoder)
    {
        _jwtSetting = jwtSetting;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(CustomizeJwtAuthenticationOptions.SchemeHeader, out var authHeader))
        {
            var socialHub = Request.Path.StartsWithSegments("/hubs/web/social") ||
                Request.Path.StartsWithSegments("/hubs/era/social");
            var queryToken = Request.Query["access_token"].ToString();
            if (!socialHub || string.IsNullOrEmpty(queryToken))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }
            authHeader = "Bearer " + queryToken;
        }
        
        var authHeaderValue = authHeader.ToString();
        if (!authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header format"));
        
        var token = authHeaderValue.Substring("Bearer ".Length).Trim();
        if (string.IsNullOrEmpty(token))
            return Task.FromResult(AuthenticateResult.Fail("Token is missing"));
        
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKey = _jwtSetting.Secret;
        
            if (string.IsNullOrEmpty(jwtKey))
                return Task.FromResult(AuthenticateResult.Fail("JWT configuration is missing"));
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _jwtSetting.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSetting.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (principal.FindFirst(TokenClaimService.TokenUseClaim)?.Value == TokenClaimService.RefreshTokenUse)
                return Task.FromResult(AuthenticateResult.Fail("Invalid access token"));
            
            var properties = new AuthenticationProperties
            {
                ExpiresUtc = new DateTimeOffset(validatedToken.ValidTo, TimeSpan.Zero)
            };
            var ticket = new AuthenticationTicket(principal, properties, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (SecurityTokenExpiredException)
        {
            return Task.FromResult(AuthenticateResult.Fail("Token has expired"));
        }
        catch (SecurityTokenValidationException ex)
        {
            return Task.FromResult(AuthenticateResult.Fail($"Token validation failed: {ex.Message}"));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Authentication failed");
            return Task.FromResult(AuthenticateResult.Fail("Authentication failed"));
        }
    }
}