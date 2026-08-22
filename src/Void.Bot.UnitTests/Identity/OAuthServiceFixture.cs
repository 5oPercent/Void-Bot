using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Identity.OAuth;
using Void.Bot.Core.Settings.OAuth;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Commands.Identity;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Dto.Identity;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Identity;
using Void.Bot.Message.Request.Account;

namespace Void.Bot.UnitTest.Identity;

public class OAuthServiceFixture
{
    private const string ExchangeCode = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string RegistrationCode = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private readonly IOAuthProvider _provider = Substitute.For<IOAuthProvider>();
    private readonly IOAuthExchangeStore _exchangeStore = Substitute.For<IOAuthExchangeStore>();
    private readonly IOAuthRegistrationStore _registrationStore = Substitute.For<IOAuthRegistrationStore>();
    private readonly IAccountService _accountService = Substitute.For<IAccountService>();
    private readonly IAccountDataProvider _accountDataProvider = Substitute.For<IAccountDataProvider>();
    private readonly ITokenClaimService _tokenClaimService = Substitute.For<ITokenClaimService>();
    private readonly OAuthService _service;

    public OAuthServiceFixture()
    {
        _provider.Platform.Returns(AppPlatformEnum.Discord);
        var setting = new OAuthSetting(new ConfigurationBuilder().Build())
        {
            ExchangeTtlSeconds = OAuthSetting.DefaultExchangeTtlSeconds,
            RegistrationTtlSeconds = OAuthSetting.DefaultRegistrationTtlSeconds
        };
        _service = new OAuthService(
            [_provider],
            Substitute.For<ICurrentUser>(),
            _exchangeStore,
            _registrationStore,
            _accountService,
            _accountDataProvider,
            _tokenClaimService,
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
    public async Task RegisterCallbackStoresPendingIdentityAsync()
    {
        _provider.CallbackAsync(
                Arg.Any<OAuthCallbackContext>(),
                Arg.Any<CancellationToken>())
            .Returns(CreateCallbackResponse(OAuthStateFlow.Register));
        _registrationStore.CreateAsync(
                Arg.Any<OAuthRegistrationIdentity>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>())
            .Returns(new CreateOAuthRegistrationResult.Created(RegistrationCode));

        var response = await _service.CallbackAsync(
            AppPlatformEnum.Discord,
            new OAuthCallbackContext("code", "state", null),
            CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.Flow.ShouldBe(OAuthStateFlow.Register);
        response.Data.RegistrationCode.ShouldBe(RegistrationCode);
        response.Data.AccessToken.ShouldBeEmpty();
        response.Data.RefreshToken.ShouldBeEmpty();
        await _registrationStore.Received(1).CreateAsync(
            Arg.Is<OAuthRegistrationIdentity>(identity =>
                identity.Platform == AppPlatformEnum.Discord &&
                identity.ProviderUserId == "provider-user" &&
                identity.AccessToken == "access-token" &&
                identity.RefreshToken == "refresh-token"),
            TimeSpan.FromSeconds(OAuthSetting.DefaultRegistrationTtlSeconds),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterCreatesAccountBindingAndJwtAsync()
    {
        var accountId = Guid.NewGuid();
        _registrationStore.TakeAsync(RegistrationCode, Arg.Any<CancellationToken>())
            .Returns(new TakeOAuthRegistrationResult.Found(CreateRegistrationRecord()));
        _accountService.RegisterUserAccountAsync(
                Arg.Any<RegisterUserAccountCommand>(),
                Arg.Any<CancellationToken>())
            .Returns(new RegisterUserAccountResponse
            {
                Data = new UserAccountInfoDto
                {
                    Id = accountId,
                    Name = "VoidUser",
                    Suffix = "0001"
                }
            });
        _tokenClaimService.GenerateAccessToken(Arg.Any<IEnumerable<System.Security.Claims.Claim>>())
            .Returns("void-access-token");
        _tokenClaimService.GenerateRefreshToken(Arg.Any<IEnumerable<System.Security.Claims.Claim>>())
            .Returns("void-refresh-token");

        var response = await _service.RegisterAsync(
            new OAuthRegisterCommand
            {
                Code = RegistrationCode,
                Name = "VoidUser",
                Password = "Password123"
            },
            CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.AccessToken.ShouldBe("void-access-token");
        response.Data.RefreshToken.ShouldBe("void-refresh-token");
        await _accountDataProvider.Received(1).LinkThirdPartyAsync(
            Arg.Is<UserThirdPartyDto>(binding =>
                binding.UserId == accountId &&
                binding.Provider == nameof(AppPlatformEnum.Discord) &&
                binding.ThirdPartyUserId == "provider-user" &&
                binding.AccessToken == "access-token"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BindInvalidCredentialsDoesNotConsumeRegistrationCodeAsync()
    {
        var accountId = Guid.NewGuid();
        _registrationStore.GetAsync(RegistrationCode, Arg.Any<CancellationToken>())
            .Returns(new GetOAuthRegistrationResult.Found(CreateRegistrationRecord()));
        _accountService.GetUserAccountAsync(
                Arg.Any<GetUserAccountRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(new GetUserAccountResponse
            {
                Data = new UserAccountInfoDto
                {
                    Id = accountId,
                    Name = "VoidUser",
                    Suffix = "0001"
                }
            });
        _accountService.VerifyPassword(accountId, "wrong-password", Arg.Any<CancellationToken>())
            .Returns(false);

        var response = await _service.BindAsync(
            new OAuthBindCommand
            {
                Code = RegistrationCode,
                Name = "VoidUser",
                Suffix = "0001",
                Password = "wrong-password"
            },
            CancellationToken.None);

        response.Message.ShouldBe(OAuthError.InvalidCredentials.Code);
        await _registrationStore.DidNotReceive().TakeAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BindExistingAccountCreatesBindingAndJwtAsync()
    {
        var accountId = Guid.NewGuid();
        var record = CreateRegistrationRecord();
        _registrationStore.GetAsync(RegistrationCode, Arg.Any<CancellationToken>())
            .Returns(new GetOAuthRegistrationResult.Found(record));
        _registrationStore.TakeAsync(RegistrationCode, Arg.Any<CancellationToken>())
            .Returns(new TakeOAuthRegistrationResult.Found(record));
        _accountService.GetUserAccountAsync(
                Arg.Any<GetUserAccountRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(new GetUserAccountResponse
            {
                Data = new UserAccountInfoDto
                {
                    Id = accountId,
                    Name = "VoidUser",
                    Suffix = "0001"
                }
            });
        _accountService.VerifyPassword(accountId, "Password123", Arg.Any<CancellationToken>())
            .Returns(true);
        _tokenClaimService.GenerateAccessToken(Arg.Any<IEnumerable<System.Security.Claims.Claim>>())
            .Returns("void-access-token");
        _tokenClaimService.GenerateRefreshToken(Arg.Any<IEnumerable<System.Security.Claims.Claim>>())
            .Returns("void-refresh-token");

        var response = await _service.BindAsync(
            new OAuthBindCommand
            {
                Code = RegistrationCode,
                Name = "VoidUser",
                Suffix = "0001",
                Password = "Password123"
            },
            CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.AccessToken.ShouldBe("void-access-token");
        await _accountDataProvider.Received(1).LinkThirdPartyAsync(
            Arg.Is<UserThirdPartyDto>(binding =>
                binding.UserId == accountId &&
                binding.Provider == nameof(AppPlatformEnum.Discord) &&
                binding.ThirdPartyUserId == "provider-user"),
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
                ProviderUserId = "provider-user",
                AccessToken = "access-token",
                RefreshToken = "refresh-token"
            }
        };
    }

    private static OAuthRegistrationRecord CreateRegistrationRecord()
    {
        return new OAuthRegistrationRecord
        {
            Code = RegistrationCode,
            Platform = AppPlatformEnum.Discord,
            ProviderUserId = "provider-user",
            ProviderUserName = "Provider User",
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            IssuedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
        };
    }
}
