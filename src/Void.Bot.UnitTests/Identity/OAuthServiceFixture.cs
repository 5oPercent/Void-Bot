using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Identity.OAuth;
using Void.Bot.Core.Settings.OAuth;
using Void.Bot.Message.Commands.Identity;
using Void.Bot.Message.Dto.Identity;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Identity;

namespace Void.Bot.UnitTest.Identity;

public class OAuthServiceFixture
{
    private const string ExchangeCode = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private readonly IOAuthProvider _provider = Substitute.For<IOAuthProvider>();
    private readonly IOAuthExchangeStore _exchangeStore = Substitute.For<IOAuthExchangeStore>();
    private readonly OAuthService _service;

    public OAuthServiceFixture()
    {
        _provider.Platform.Returns(AppPlatformEnum.Discord);
        var setting = new OAuthSetting(new ConfigurationBuilder().Build())
        {
            ExchangeTtlSeconds = OAuthSetting.DefaultExchangeTtlSeconds
        };
        _service = new OAuthService(
            [_provider],
            Substitute.For<ICurrentUser>(),
            _exchangeStore,
            setting);
    }

    [Fact]
    public async Task LoginCallbackReplacesTokensWithExchangeCodeAsync()
    {
        _provider.CallbackAsync(
                Arg.Any<OAuthCallbackContext>(),
                Arg.Any<CancellationToken>())
            .Returns(CreateCallbackResponse(OAuthStateFlow.Login));
        _exchangeStore.CreateAsync(
                Arg.Any<JwtToken>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>())
            .Returns(new CreateOAuthExchangeResult.Created(ExchangeCode));

        var response = await _service.CallbackAsync(
            AppPlatformEnum.Discord,
            new OAuthCallbackContext("code", "state", null),
            CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.ExchangeCode.ShouldBe(ExchangeCode);
        response.Data.AccessToken.ShouldBeEmpty();
        response.Data.RefreshToken.ShouldBeEmpty();
        await _exchangeStore.Received(1).CreateAsync(
            Arg.Is<JwtToken>(token =>
                token.AccessToken == "access-token" &&
                token.RefreshToken == "refresh-token"),
            TimeSpan.FromSeconds(OAuthSetting.DefaultExchangeTtlSeconds),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LinkCallbackDoesNotCreateExchangeCodeAsync()
    {
        _provider.CallbackAsync(
                Arg.Any<OAuthCallbackContext>(),
                Arg.Any<CancellationToken>())
            .Returns(CreateCallbackResponse(OAuthStateFlow.Link));

        var response = await _service.CallbackAsync(
            AppPlatformEnum.Discord,
            new OAuthCallbackContext("code", "state", null),
            CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.Flow.ShouldBe(OAuthStateFlow.Link);
        response.Data.ExchangeCode.ShouldBeEmpty();
        await _exchangeStore.DidNotReceive().CreateAsync(
            Arg.Any<JwtToken>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExchangeReturnsTokensOnlyOnceAsync()
    {
        _exchangeStore.TakeAsync(ExchangeCode, Arg.Any<CancellationToken>())
            .Returns(
                new TakeOAuthExchangeResult.Found(
                    new JwtToken
                    {
                        AccessToken = "access-token",
                        RefreshToken = "refresh-token"
                    }),
                new TakeOAuthExchangeResult.Missing());

        var first = await _service.ExchangeAsync(ExchangeCode, CancellationToken.None);
        var replay = await _service.ExchangeAsync(ExchangeCode, CancellationToken.None);

        first.Data.ShouldNotBeNull();
        first.Data.AccessToken.ShouldBe("access-token");
        first.Data.RefreshToken.ShouldBe("refresh-token");
        replay.Data.ShouldBeNull();
        replay.Message.ShouldBe(OAuthError.InvalidExchangeCode.Code);
    }

    private static OAuthCallbackResponse CreateCallbackResponse(OAuthStateFlow flow)
    {
        return new OAuthCallbackResponse
        {
            Data = new OAuthCallbackData
            {
                Platform = nameof(AppPlatformEnum.Discord),
                Flow = flow,
                AccessToken = "access-token",
                RefreshToken = "refresh-token"
            }
        };
    }
}
