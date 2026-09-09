using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shouldly;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Settings.System;

namespace Void.Bot.UnitTest.Identity;

public class TokenClaimServiceFixture
{
    private static readonly Claim[] Claims =
    [
        new(ClaimTypes.Name, "test#0001"),
        new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
    ];

    [Fact]
    public void ValidateRefreshToken_WithRefreshToken_ReturnsPrincipal()
    {
        var service = CreateService();

        var refreshToken = service.GenerateRefreshToken(Claims);
        var principal = service.ValidateRefreshToken(refreshToken);

        principal.FindFirstValue(ClaimTypes.Name).ShouldBe(Claims[0].Value);
        principal.FindFirstValue(ClaimTypes.NameIdentifier).ShouldBe(Claims[1].Value);
        principal.FindFirstValue(TokenClaimService.TokenUseClaim)
            .ShouldBe(TokenClaimService.RefreshTokenUse);
    }

    [Fact]
    public void ValidateRefreshToken_PreservesTwoFactorAuthenticationClaim()
    {
        var service = CreateService();
        var claims = Claims.Append(new Claim(
            TokenClaimService.TwoFactorAuthenticationClaim,
            TokenClaimService.TwoFactorAuthenticationValue));

        var refreshToken = service.GenerateRefreshToken(claims);
        var principal = service.ValidateRefreshToken(refreshToken);

        principal.HasClaim(
            TokenClaimService.TwoFactorAuthenticationClaim,
            TokenClaimService.TwoFactorAuthenticationValue).ShouldBeTrue();
    }

    [Fact]
    public void ValidateRefreshToken_WithAccessToken_ThrowsValidationException()
    {
        var service = CreateService();
        var accessToken = service.GenerateAccessToken(Claims);

        Should.Throw<SecurityTokenValidationException>(() =>
            service.ValidateRefreshToken(accessToken));
    }

    [Fact]
    public void ValidateRefreshToken_WithExpiredToken_ThrowsExpiredException()
    {
        var service = CreateService(refreshExpireDays: -1);
        var refreshToken = service.GenerateRefreshToken(Claims);

        Should.Throw<SecurityTokenExpiredException>(() =>
            service.ValidateRefreshToken(refreshToken));
    }

    private static TokenClaimService CreateService(int refreshExpireDays = 7)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "0123456789abcdef0123456789abcdef",
                ["Jwt:Issuer"] = "VoidBot",
                ["Jwt:Audience"] = "VoidBotClient",
                ["Jwt:ExpireMinutes"] = "60",
                ["Jwt:RefreshExpireDays"] = refreshExpireDays.ToString()
            })
            .Build();

        return new TokenClaimService(new JwtSetting(configuration));
    }
}
