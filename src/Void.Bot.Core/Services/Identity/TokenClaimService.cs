using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Void.Bot.Core.Settings.System;

namespace Void.Bot.Core.Services.Identity;

public interface ITokenClaimService : IScope
{
    string GenerateAccessToken(IEnumerable<Claim> claims);
    
    string GenerateRefreshToken(IEnumerable<Claim> claims);

    ClaimsPrincipal ValidateRefreshToken(string refreshToken);
}

public class TokenClaimService : ITokenClaimService
{
    public const string TokenUseClaim = "token_use";
    public const string AccessTokenUse = "access";
    public const string RefreshTokenUse = "refresh";
    public const string TwoFactorAuthenticationClaim = "two_factor_authenticated";
    public const string TwoFactorAuthenticationValue = "true";

    private readonly JwtSetting _jwtSetting;

    public TokenClaimService(JwtSetting jwtSetting)
    {
        _jwtSetting = jwtSetting;
    }
    
    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        return GenerateToken(
            claims,
            AccessTokenUse,
            DateTime.UtcNow.AddMinutes(_jwtSetting.ExpireMinutes));
    }

    public string GenerateRefreshToken(IEnumerable<Claim> claims)
    {
        return GenerateToken(
            claims,
            RefreshTokenUse,
            DateTime.UtcNow.AddDays(_jwtSetting.RefreshExpireDays));
    }

    public ClaimsPrincipal ValidateRefreshToken(string refreshToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(
            refreshToken,
            CreateValidationParameters(),
            out var validatedToken);

        if (validatedToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.Ordinal) ||
            principal.FindFirstValue(TokenUseClaim) != RefreshTokenUse)
        {
            throw new SecurityTokenValidationException("Invalid refresh token");
        }

        return principal;
    }

    private string GenerateToken(IEnumerable<Claim> claims, string tokenUse, DateTime expires)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var tokenClaims = claims
            .Where(claim => claim.Type != TokenUseClaim)
            .Append(new Claim(TokenUseClaim, tokenUse));

        var token = new JwtSecurityToken(
            issuer: _jwtSetting.Issuer,
            audience: _jwtSetting.Audience,
            claims: tokenClaims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private TokenValidationParameters CreateValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Secret)),
            ValidateIssuer = true,
            ValidIssuer = _jwtSetting.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSetting.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }
}
