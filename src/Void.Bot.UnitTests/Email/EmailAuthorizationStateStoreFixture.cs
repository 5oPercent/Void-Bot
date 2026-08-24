using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Settings.Email;
using ILogger = Serilog.ILogger;

namespace Void.Bot.UnitTest.Email;

public class EmailAuthorizationStateStoreFixture
{
    private readonly ICacheManager _cacheManager = Substitute.For<ICacheManager>();
    private readonly EmailAuthorizationStateStore _store;

    public EmailAuthorizationStateStoreFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Email:StateTtlSeconds"] = "600"
            })
            .Build();
        _store = new EmailAuthorizationStateStore(
            _cacheManager,
            new EmailSetting(configuration),
            Substitute.For<ILogger>());
    }

    [Fact]
    public async Task CreateStoresPkceAndNonceStateAsync()
    {
        var record = await _store.CreateAsync(true, CancellationToken.None);

        record.State.Length.ShouldBe(43);
        record.Nonce.Length.ShouldBe(43);
        record.CodeVerifier.Length.ShouldBe(86);
        record.ForceReauthorization.ShouldBeTrue();
        EmailAuthorizationStateStore.CreateCodeChallenge(record.CodeVerifier).Length.ShouldBe(43);
        await _cacheManager.Received(1).Set(
            $"email:oauth:state:{record.State}",
            record,
            TimeSpan.FromMinutes(10),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TakeConsumesValidStateAsync()
    {
        const string state = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var record = new EmailAuthorizationStateRecord
        {
            State = state,
            Nonce = "nonce",
            CodeVerifier = "verifier",
            IssuedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(1)
        };
        _cacheManager.Take<EmailAuthorizationStateRecord>(
                $"email:oauth:state:{state}", Arg.Any<CancellationToken>())
            .Returns(record);

        var result = await _store.TakeAsync(state, CancellationToken.None);

        result.ShouldBeSameAs(record);
    }
}
