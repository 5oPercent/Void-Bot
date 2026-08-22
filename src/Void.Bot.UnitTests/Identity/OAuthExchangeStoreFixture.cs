using NSubstitute;
using Shouldly;
using Void.Bot.Core.Caching;
using Void.Bot.Message.Dto.Identity;
using ILogger = Serilog.ILogger;

namespace Void.Bot.UnitTest.Identity;

public class OAuthExchangeStoreFixture
{
    private static readonly DateTimeOffset Now = new(2026, 8, 22, 1, 0, 0, TimeSpan.Zero);
    private readonly ICacheManager _cacheManager = Substitute.For<ICacheManager>();
    private readonly OAuthExchangeStore _store;

    public OAuthExchangeStoreFixture()
    {
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(Now);
        _store = new OAuthExchangeStore(
            _cacheManager,
            timeProvider,
            Substitute.For<ILogger>());
    }

    [Fact]
    public async Task CreateStoresShortLivedSingleUseCodeAsync()
    {
        var result = await _store.CreateAsync(
            new JwtToken
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token"
            },
            TimeSpan.FromMinutes(2));

        var created = result.ShouldBeOfType<CreateOAuthExchangeResult.Created>();
        created.Code.Length.ShouldBe(43);
        await _cacheManager.Received(1).Set(
            $"oauth:exchange:{created.Code}",
            Arg.Is<OAuthExchangeRecord>(record =>
                record.Code == created.Code &&
                record.AccessToken == "access-token" &&
                record.RefreshToken == "refresh-token" &&
                record.IssuedAt == Now &&
                record.ExpiresAt == Now.AddMinutes(2)),
            TimeSpan.FromMinutes(2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TakeReturnsTokensFromValidCodeAsync()
    {
        const string code = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        _cacheManager.Take<OAuthExchangeRecord>(
                $"oauth:exchange:{code}",
                Arg.Any<CancellationToken>())
            .Returns(new OAuthExchangeRecord
            {
                Code = code,
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                IssuedAt = Now.AddMinutes(-1),
                ExpiresAt = Now.AddMinutes(1)
            });

        var result = await _store.TakeAsync(code);

        var found = result.ShouldBeOfType<TakeOAuthExchangeResult.Found>();
        found.Token.AccessToken.ShouldBe("access-token");
        found.Token.RefreshToken.ShouldBe("refresh-token");
    }

    [Theory]
    [InlineData("")]
    [InlineData("too-short")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa!")]
    public async Task TakeRejectsMalformedCodeWithoutReadingCacheAsync(string code)
    {
        var result = await _store.TakeAsync(code);

        result.ShouldBeOfType<TakeOAuthExchangeResult.Missing>();
        await _cacheManager.DidNotReceive().Take<OAuthExchangeRecord>(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }
}
