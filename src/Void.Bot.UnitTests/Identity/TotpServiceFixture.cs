using NSubstitute;
using Shouldly;
using Void.Bot.Core.Services.Identity;

namespace Void.Bot.UnitTest.Identity;

public sealed class TotpServiceFixture
{
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    [Fact]
    public void VerifyCodeAcceptsRfcVectorAndAdjacentTimeStep()
    {
        _timeProvider.GetUtcNow().Returns(DateTimeOffset.FromUnixTimeSeconds(59));
        var service = new TotpService(_timeProvider);

        service.VerifyCode("GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ", "287082").ShouldBeTrue();

        _timeProvider.GetUtcNow().Returns(DateTimeOffset.FromUnixTimeSeconds(89));
        service.VerifyCode("GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ", "287082").ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("12345a")]
    public void VerifyCodeRejectsMalformedCodes(string? code)
    {
        _timeProvider.GetUtcNow().Returns(DateTimeOffset.FromUnixTimeSeconds(59));
        var service = new TotpService(_timeProvider);

        service.VerifyCode("GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ", code).ShouldBeFalse();
    }

    [Fact]
    public void SetupUriContainsEncodedIssuerAccountAndSecret()
    {
        var service = new TotpService(_timeProvider);

        var uri = service.CreateOtpAuthUri("Void Bot", "测试 User#0001", "ABC234");

        uri.ShouldBe("otpauth://totp/Void%20Bot%3A%E6%B5%8B%E8%AF%95%20User%230001?secret=ABC234&issuer=Void%20Bot&algorithm=SHA1&digits=6&period=30");
    }

    [Fact]
    public void GeneratedSecretHasExpectedBase32Format()
    {
        var service = new TotpService(_timeProvider);

        var secret = service.GenerateSecret();

        secret.Length.ShouldBe(32);
        secret.ShouldAllBe(character => "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".Contains(character));
    }
}
