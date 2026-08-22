using NSubstitute;
using Shouldly;
using Void.Bot.Core.Caching;
using Void.Bot.Message.Enum;
using ILogger = Serilog.ILogger;

namespace Void.Bot.UnitTest.Identity;

public sealed class OAuthRegistrationStoreFixture
{
    private static readonly DateTimeOffset Now = new(2026, 8, 22, 1, 0, 0, TimeSpan.Zero);
    private readonly ICacheManager _cacheManager = Substitute.For<ICacheManager>();
    private readonly OAuthRegistrationStore _store;

    public OAuthRegistrationStoreFixture()
    {
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(Now);
        _store = new OAuthRegistrationStore(
            _cacheManager,
            timeProvider,
            Substitute.For<ILogger>());
    }

    [Fact]
    public async Task CreateStoresProviderTokensBehindShortLivedCodeAsync()
    {
        var result = await _store.CreateAsync(
            new OAuthRegistrationIdentity
            {
                Platform = AppPlatformEnum.Discord,
                ProviderUserId = "provider-user",
                ProviderUserName = "Provider User",
                AccessToken = "access-token",
                RefreshToken = "refresh-token"
            },
            TimeSpan.FromMinutes(10));

        var created = result.ShouldBeOfType<CreateOAuthRegistrationResult.Created>();
        created.Code.Length.ShouldBe(43);
        await _cacheManager.Received(1).Set(
            $"oauth:registration:{created.Code}",
            Arg.Is<OAuthRegistrationRecord>(record =>
                record.Code == created.Code &&
                record.Platform == AppPlatformEnum.Discord &&
                record.ProviderUserId == "provider-user" &&
                record.AccessToken == "access-token" &&
                record.RefreshToken == "refresh-token" &&
                record.IssuedAt == Now &&
                record.ExpiresAt == Now.AddMinutes(10)),
            TimeSpan.FromMinutes(10),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TakeConsumesValidCodeAtomicallyAsync()
    {
        const string code = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var record = CreateRecord(code);
        _cacheManager.Take<OAuthRegistrationRecord>(
                $"oauth:registration:{code}",
                Arg.Any<CancellationToken>())
            .Returns(record);

        var result = await _store.TakeAsync(code);

        result.ShouldBeOfType<TakeOAuthRegistrationResult.Found>().Record.ShouldBe(record);
        await _cacheManager.Received(1).Take<OAuthRegistrationRecord>(
            $"oauth:registration:{code}",
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("too-short")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa!")]
    public async Task GetRejectsMalformedCodeWithoutReadingCacheAsync(string code)
    {
        var result = await _store.GetAsync(code);

        result.ShouldBeOfType<GetOAuthRegistrationResult.Missing>();
        await _cacheManager.DidNotReceive().Get<OAuthRegistrationRecord>(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    private static OAuthRegistrationRecord CreateRecord(string code)
    {
        return new OAuthRegistrationRecord
        {
            Code = code,
            Platform = AppPlatformEnum.Discord,
            ProviderUserId = "provider-user",
            AccessToken = "access-token",
            IssuedAt = Now.AddMinutes(-1),
            ExpiresAt = Now.AddMinutes(1)
        };
    }
}
