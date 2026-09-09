using NSubstitute;
using Shouldly;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Commands.Identity;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Enum.Account;
using Void.Bot.Message.Request.Account;
using Void.Bot.Message.Request.Identity;
using ILogger = Serilog.ILogger;

namespace Void.Bot.UnitTest.Identity;

public sealed class IdentityTwoFactorFixture
{
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private readonly IAccountService _accountService = Substitute.For<IAccountService>();
    private readonly IAccountTwoFactorService _twoFactorService = Substitute.For<IAccountTwoFactorService>();
    private readonly ITokenClaimService _tokenClaimService = Substitute.For<ITokenClaimService>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IdentityService _service;

    public IdentityTwoFactorFixture()
    {
        _accountService.GetUserAccountAsync(
                Arg.Any<GetUserAccountRequest>(), Arg.Any<CancellationToken>())
            .Returns(new GetUserAccountResponse
            {
                Data = new UserAccountInfoDto
                {
                    Id = UserId,
                    Name = "VoidUser",
                    Suffix = "0001"
                }
            });
        _accountService.VerifyPassword(UserId, "Password1", Arg.Any<CancellationToken>())
            .Returns(true);
        _currentUser.Id.Returns(UserId);
        _service = new IdentityService(
            Substitute.For<ILogger>(),
            _tokenClaimService,
            _accountService,
            _twoFactorService,
            _currentUser);
    }

    [Fact]
    public async Task LoginDoesNotIssueTokensWhenTwoFactorCodeIsRequiredAsync()
    {
        _twoFactorService.VerifyLoginAsync(UserId, null, Arg.Any<CancellationToken>())
            .Returns(TwoFactorLoginVerificationResult.CodeRequired);

        var response = await _service.AuthenticateAsync(CreateRequest(), CancellationToken.None);

        response.RequiresTwoFactor.ShouldBeTrue();
        response.Message.ShouldBe("two_factor_code_required");
        response.Data.ShouldBeNull();
        _tokenClaimService.DidNotReceive().GenerateAccessToken(Arg.Any<IEnumerable<System.Security.Claims.Claim>>());
        _tokenClaimService.DidNotReceive().GenerateRefreshToken(Arg.Any<IEnumerable<System.Security.Claims.Claim>>());
    }

    [Fact]
    public async Task LoginIssuesTokensAfterValidTwoFactorCodeAsync()
    {
        _twoFactorService.VerifyLoginAsync(UserId, "123456", Arg.Any<CancellationToken>())
            .Returns(TwoFactorLoginVerificationResult.Valid);
        _tokenClaimService.GenerateAccessToken(Arg.Any<IEnumerable<System.Security.Claims.Claim>>())
            .Returns("access-token");
        _tokenClaimService.GenerateRefreshToken(Arg.Any<IEnumerable<System.Security.Claims.Claim>>())
            .Returns("refresh-token");

        var response = await _service.AuthenticateAsync(CreateRequest("123456"), CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.AccessToken.ShouldBe("access-token");
        response.Data.RefreshToken.ShouldBe("refresh-token");
        _tokenClaimService.Received(1).GenerateAccessToken(
            Arg.Is<IEnumerable<System.Security.Claims.Claim>>(claims => claims.Any(claim =>
                claim.Type == TokenClaimService.TwoFactorAuthenticationClaim &&
                claim.Value == TokenClaimService.TwoFactorAuthenticationValue)));
    }

    [Fact]
    public async Task VerifyTwoFactorUpgradesCurrentJwtAsync()
    {
        _accountService.GetUserAccountAsync(
                Arg.Is<GetUserAccountRequest>(request => request.UserId == UserId),
                Arg.Any<CancellationToken>())
            .Returns(new GetUserAccountResponse
            {
                Data = new UserAccountInfoDto
                {
                    Id = UserId,
                    Name = "VoidUser",
                    Suffix = "0001",
                    IsTwoFactorEnabled = true
                }
            });
        _twoFactorService.VerifyLoginAsync(UserId, "123456", Arg.Any<CancellationToken>())
            .Returns(TwoFactorLoginVerificationResult.Valid);
        _tokenClaimService.GenerateAccessToken(Arg.Any<IEnumerable<System.Security.Claims.Claim>>())
            .Returns("upgraded-access-token");
        _tokenClaimService.GenerateRefreshToken(Arg.Any<IEnumerable<System.Security.Claims.Claim>>())
            .Returns("upgraded-refresh-token");

        var response = await _service.VerifyTwoFactorAsync(
            new VerifyTwoFactorCommand { Code = "123456" },
            CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.AccessToken.ShouldBe("upgraded-access-token");
        response.Data.RefreshToken.ShouldBe("upgraded-refresh-token");
        _tokenClaimService.Received(1).GenerateAccessToken(
            Arg.Is<IEnumerable<System.Security.Claims.Claim>>(claims => claims.Any(claim =>
                claim.Type == TokenClaimService.TwoFactorAuthenticationClaim &&
                claim.Value == TokenClaimService.TwoFactorAuthenticationValue)));
    }

    private static AuthenticateRequest CreateRequest(string? code = null)
    {
        return new AuthenticateRequest
        {
            Name = "VoidUser",
            Suffix = "0001",
            Password = "Password1",
            TwoFactorCode = code
        };
    }
}
