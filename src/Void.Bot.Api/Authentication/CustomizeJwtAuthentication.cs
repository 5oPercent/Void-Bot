using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Void.Bot.Core.Settings.System;

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
        IOptionsMonitor<CustomizeJwtAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock,
        JwtSetting jwtSetting)
            : base(options, logger, encoder, clock)
    {
        _jwtSetting = jwtSetting;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(CustomizeJwtAuthenticationOptions.SchemeHeader, out var authHeader))
            return AuthenticateResult.NoResult();
        
        var authHeaderValue = authHeader.ToString();
        if (!authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.Fail("Invalid Authorization header format");
        
        var token = authHeaderValue.Substring("Bearer ".Length).Trim();
        if (string.IsNullOrEmpty(token))
            return AuthenticateResult.Fail("Token is missing");
        
        try
        {
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtKey = _jwtSetting.Secret;
        
            if (string.IsNullOrEmpty(jwtKey))
                return AuthenticateResult.Fail("JWT configuration is missing");
            
            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtKey));

            var validationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
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
            
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
        catch (Microsoft.IdentityModel.Tokens.SecurityTokenExpiredException)
        {
            return AuthenticateResult.Fail("Token has expired");
        }
        catch (Microsoft.IdentityModel.Tokens.SecurityTokenValidationException ex)
        {
            return AuthenticateResult.Fail($"Token validation failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Authentication failed");
            return AuthenticateResult.Fail("Authentication failed");
        }
    }
}