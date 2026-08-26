using System.Net;
using Autofac;
using Mediator.Net;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Account;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Discord;
using Void.Bot.Message.Commands.Identity;
using Void.Bot.Message.Dto.Discord.OAuth;
using Void.Bot.Message.Dto.Identity;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Identity;
using Void.Bot.Message.Request.Discord.OAuth;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.IntegrationTest.Services.Identity;

public class DiscordOAuthFixture(IntegrationTestBase testBase) : IntegrationFixture(testBase)
{
    private const string Provider = nameof(AppPlatformEnum.Discord);
    private static readonly Guid CurrentUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public async Task LoginWithNewDiscordIdentityRequiresRegistrationWithoutCreatingAccountAsync()
    {
        var identity = CreateIdentity("discord-new-user");
        var response = await CallbackAsync(CreateState(OAuthStateFlow.Login), identity);

        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data.ShouldNotBeNull();
        response.Data.Platform.ShouldBe(Provider);
        response.Data.ProviderUserId.ShouldBe(identity.UserId);
        response.Data.ProviderUserName.ShouldBe(identity.DisplayName);
        response.Data.Flow.ShouldBe(OAuthStateFlow.Register);
        response.Data.RegistrationCode.ShouldBe("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
        response.Data.AccessToken.ShouldBeEmpty();
        response.Data.RefreshToken.ShouldBeEmpty();

        await Run<IRepository<UserAccount>, IRepository<UserThirdParty>>(async (accounts, bindings) =>
        {
            (await accounts.GetAllAsync()).ShouldBeEmpty();
            (await bindings.GetAllAsync()).ShouldBeEmpty();
        });
    }

    [Fact]
    public async Task LoginWithExistingDiscordIdentityUpdatesTokensWithoutCreatingAccountAsync()
    {
        var accountId = Guid.NewGuid();
        var oldIdentity = CreateIdentity("discord-existing-user", "old-access", "old-refresh");
        await AddAccountAsync(accountId, "Existing User");
        await AddBindingAsync(accountId, oldIdentity);

        var newIdentity = CreateIdentity("discord-existing-user", "new-access", "new-refresh");
        var response = await CallbackAsync(CreateState(OAuthStateFlow.Login), newIdentity);

        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data.ShouldNotBeNull();

        await Run<IRepository<UserAccount>, IAccountDataProvider>(async (accounts, provider) =>
        {
            (await accounts.GetAllAsync()).Count.ShouldBe(1);
            var binding = await provider.GetThirdPartyBindingAsync(
                Provider,
                newIdentity.UserId,
                CancellationToken.None);
            AssertBinding(binding, accountId, newIdentity);
        });
    }

    [Fact]
    public async Task LinkDiscordIdentityBindsItToInitiatingAccountAsync()
    {
        await AddAccountAsync(CurrentUserId, "Current User");
        var identity = CreateIdentity("discord-link-user");

        var response = await CallbackAsync(
            CreateState(OAuthStateFlow.Link, CurrentUserId),
            identity);

        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data.ShouldNotBeNull();

        await Run<IRepository<UserAccount>, IAccountDataProvider>(async (accounts, provider) =>
        {
            (await accounts.GetAllAsync()).Count.ShouldBe(1);
            var binding = await provider.GetThirdPartyBindingAsync(
                Provider,
                identity.UserId,
                CancellationToken.None);
            AssertBinding(binding, CurrentUserId, identity);
        });
    }

    [Fact]
    public async Task UnlinkDiscordIdentityDeletesOnlyTheBindingAsync()
    {
        await AddAccountAsync(CurrentUserId, "Current User");
        var identity = CreateIdentity("discord-unlink-user");
        await AddBindingAsync(CurrentUserId, identity);

        var stateStore = Substitute.For<IOAuthStateStore>();
        var discordService = Substitute.For<IDiscordService>();
        OAuthUnlinkResponse? response = null;
        await Run<IMediator>(async mediator =>
        {
            response = await mediator.SendAsync<OAuthUnlinkCommand, OAuthUnlinkResponse>(
                new OAuthUnlinkCommand { Platform = AppPlatformEnum.Discord });
        }, RegisterOAuthDoubles(stateStore, discordService));

        response.ShouldNotBeNull();
        response.Code.ShouldBe(HttpStatusCode.OK);

        await Run<IRepository<UserAccount>, IAccountDataProvider>(async (accounts, provider) =>
        {
            (await accounts.GetAllAsync()).Single().Id.ShouldBe(CurrentUserId);
            (await provider.GetThirdPartyBindingAsync(
                Provider,
                identity.UserId,
                CancellationToken.None)).ShouldBeNull();
        });
    }

    [Fact]
    public async Task AuthorizeCreatesSingleUseStateThatRejectsReplayAsync()
    {
        await AddAccountAsync(CurrentUserId, "Current User");
        var identity = CreateIdentity("discord-replay-user");
        var discordService = CreateDiscordService(identity);
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);

        OAuthAuthorizeResponse? authorizeResponse = null;
        OAuthCallbackResponse? firstResponse = null;
        OAuthCallbackResponse? replayResponse = null;
        await Run<IMediator>(async mediator =>
        {
            authorizeResponse = await mediator.SendAsync<OAuthAuthorizeCommand, OAuthAuthorizeResponse>(
                new OAuthAuthorizeCommand { Platform = AppPlatformEnum.Discord });
        }, RegisterOAuthDoubles(discordService, timeProvider));

        authorizeResponse.ShouldNotBeNull();
        authorizeResponse.Code.ShouldBe(HttpStatusCode.OK);
        authorizeResponse.Data.ShouldNotBeNull();
        authorizeResponse.Data.Platform.ShouldBe(Provider);
        var redirectUrl = authorizeResponse.Data.RedirectUrl;
        redirectUrl.ShouldNotBeNull();
        var state = ExtractQueryParameter(redirectUrl, "state");
        await Run<IMediator>(async mediator =>
        {
            firstResponse = await SendCallbackAsync(mediator, state);
        }, RegisterOAuthDoubles(discordService, timeProvider));
        await Run<IMediator>(async mediator =>
        {
            replayResponse = await SendCallbackAsync(mediator, state);
        }, RegisterOAuthDoubles(discordService, timeProvider));

        firstResponse.ShouldNotBeNull();
        firstResponse.Code.ShouldBe(HttpStatusCode.OK);
        replayResponse.ShouldNotBeNull();
        AssertError(replayResponse, HttpStatusCode.BadRequest, "invalid_oauth_state");
        await discordService.Received(1).AuthenticateOAuthAsync(
            Arg.Any<AuthenticateDiscordOAuthRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConsentDeniedConsumesStateWithoutCallingDiscordAsync()
    {
        var state = CreateState(OAuthStateFlow.Login);
        var stateStore = Substitute.For<IOAuthStateStore>();
        stateStore.TakeAsync(Arg.Any<TakeOAuthStateRequest>(), Arg.Any<CancellationToken>())
            .Returns(
                new TakeOAuthStateResponse.Found(state),
                new TakeOAuthStateResponse.Missing());
        var discordService = Substitute.For<IDiscordService>();

        OAuthCallbackResponse? deniedResponse = null;
        OAuthCallbackResponse? replayResponse = null;
        await Run<IMediator>(async mediator =>
        {
            deniedResponse = await SendCallbackAsync(mediator, state.State, error: "access_denied");
        }, RegisterOAuthDoubles(stateStore, discordService));
        await Run<IMediator>(async mediator =>
        {
            replayResponse = await SendCallbackAsync(mediator, state.State, error: "access_denied");
        }, RegisterOAuthDoubles(stateStore, discordService));

        deniedResponse.ShouldNotBeNull();
        AssertError(deniedResponse, HttpStatusCode.BadRequest, "consent_denied");
        replayResponse.ShouldNotBeNull();
        AssertError(replayResponse, HttpStatusCode.BadRequest, "invalid_oauth_state");
        await discordService.DidNotReceive().AuthenticateOAuthAsync(
            Arg.Any<AuthenticateDiscordOAuthRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LinkingDiscordIdentityOwnedByAnotherAccountReturnsConflictAsync()
    {
        var otherAccountId = Guid.NewGuid();
        await AddAccountAsync(CurrentUserId, "Current User");
        await AddAccountAsync(otherAccountId, "Other User");
        var identity = CreateIdentity("discord-owned-user");
        await AddBindingAsync(otherAccountId, identity);

        var response = await CallbackAsync(
            CreateState(OAuthStateFlow.Link, CurrentUserId),
            identity);

        AssertError(response, HttpStatusCode.Conflict, "binding_conflict");
        await Run<IAccountDataProvider>(async provider =>
        {
            var binding = await provider.GetThirdPartyBindingAsync(
                Provider,
                identity.UserId,
                CancellationToken.None);
            AssertBinding(binding, otherAccountId, identity);
        });
    }

    [Fact]
    public async Task LinkingAnotherDiscordIdentityToBoundAccountReturnsConflictAsync()
    {
        await AddAccountAsync(CurrentUserId, "Current User");
        var existingIdentity = CreateIdentity("discord-current-binding");
        await AddBindingAsync(CurrentUserId, existingIdentity);
        var anotherIdentity = CreateIdentity("discord-another-user");

        var response = await CallbackAsync(
            CreateState(OAuthStateFlow.Link, CurrentUserId),
            anotherIdentity);

        AssertError(response, HttpStatusCode.Conflict, "binding_conflict");
        await Run<IAccountDataProvider>(async provider =>
        {
            var existingBinding = await provider.GetThirdPartyByUserIdAsync(
                CurrentUserId,
                Provider,
                CancellationToken.None);
            AssertBinding(existingBinding, CurrentUserId, existingIdentity);
            (await provider.GetThirdPartyBindingAsync(
                Provider,
                anotherIdentity.UserId,
                CancellationToken.None)).ShouldBeNull();
        });
    }

    [Fact]
    public async Task DiscordProviderFailureDoesNotCreateAccountOrBindingAsync()
    {
        var state = CreateState(OAuthStateFlow.Login);
        var stateStore = CreateStateStore(state);
        var discordService = Substitute.For<IDiscordService>();
        discordService.AuthenticateOAuthAsync(
                Arg.Any<AuthenticateDiscordOAuthRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(new AuthenticateDiscordOAuthResponse.Failed());

        OAuthCallbackResponse? response = null;
        await Run<IMediator>(async mediator =>
        {
            response = await SendCallbackAsync(mediator, state.State);
        }, RegisterOAuthDoubles(stateStore, discordService));

        response.ShouldNotBeNull();
        AssertError(response, HttpStatusCode.BadGateway, "provider_failure");
        await Run<IRepository<UserAccount>, IRepository<UserThirdParty>>(async (accounts, bindings) =>
        {
            (await accounts.GetAllAsync()).ShouldBeEmpty();
            (await bindings.GetAllAsync()).ShouldBeEmpty();
        });
    }

    private async Task<OAuthCallbackResponse> CallbackAsync(
        OAuthStateRecord state,
        DiscordOAuthIdentity identity)
    {
        var stateStore = CreateStateStore(state);
        var discordService = CreateDiscordService(identity);
        OAuthCallbackResponse? response = null;

        await Run<IMediator>(async mediator =>
        {
            response = await SendCallbackAsync(mediator, state.State);
        }, RegisterOAuthDoubles(stateStore, discordService));

        return response ?? throw new InvalidOperationException("OAuth callback did not return a response.");
    }

    private static Task<OAuthCallbackResponse> SendCallbackAsync(
        IMediator mediator,
        string state,
        string? code = "authorization-code",
        string? error = null)
    {
        return mediator.SendAsync<OAuthCallbackCommand, OAuthCallbackResponse>(
            new OAuthCallbackCommand
            {
                Platform = AppPlatformEnum.Discord,
                Code = code,
                State = state,
                Error = error
            });
    }

    private static IOAuthStateStore CreateStateStore(OAuthStateRecord state)
    {
        var stateStore = Substitute.For<IOAuthStateStore>();
        stateStore.TakeAsync(Arg.Any<TakeOAuthStateRequest>(), Arg.Any<CancellationToken>())
            .Returns(new TakeOAuthStateResponse.Found(state));
        return stateStore;
    }

    private static IDiscordService CreateDiscordService(DiscordOAuthIdentity identity)
    {
        var discordService = Substitute.For<IDiscordService>();
        discordService.GenerateOAuthAuthorizationUrl(Arg.Any<string>())
            .Returns(call => $"https://discord.test/oauth?state={Uri.EscapeDataString(call.Arg<string>())}");
        discordService.AuthenticateOAuthAsync(
                Arg.Any<AuthenticateDiscordOAuthRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(new AuthenticateDiscordOAuthResponse.Succeeded(identity));
        return discordService;
    }

    private static Action<ContainerBuilder> RegisterOAuthDoubles(
        IOAuthStateStore stateStore,
        IDiscordService discordService)
    {
        return builder =>
        {
            builder.RegisterInstance(stateStore).As<IOAuthStateStore>();
            builder.RegisterInstance(discordService).As<IDiscordService>();
            builder.RegisterInstance(CreateExchangeStore()).As<IOAuthExchangeStore>();
            builder.RegisterInstance(CreateRegistrationStore()).As<IOAuthRegistrationStore>();
        };
    }

    private static Action<ContainerBuilder> RegisterOAuthDoubles(
        IDiscordService discordService,
        TimeProvider timeProvider)
    {
        return builder =>
        {
            builder.RegisterInstance(discordService).As<IDiscordService>();
            builder.RegisterInstance(timeProvider).As<TimeProvider>();
            builder.RegisterInstance(CreateExchangeStore()).As<IOAuthExchangeStore>();
            builder.RegisterInstance(CreateRegistrationStore()).As<IOAuthRegistrationStore>();
        };
    }

    private static IOAuthExchangeStore CreateExchangeStore()
    {
        var exchangeStore = Substitute.For<IOAuthExchangeStore>();
        exchangeStore.CreateAsync(
                Arg.Any<JwtToken>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>())
            .Returns(new CreateOAuthExchangeResult.Created(
                "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"));
        return exchangeStore;
    }

    private static IOAuthRegistrationStore CreateRegistrationStore()
    {
        var registrationStore = Substitute.For<IOAuthRegistrationStore>();
        registrationStore.CreateAsync(
                Arg.Any<OAuthRegistrationIdentity>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>())
            .Returns(new CreateOAuthRegistrationResult.Created(
                "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"));
        return registrationStore;
    }

    private async Task AddAccountAsync(Guid id, string name)
    {
        await RunWithUnitOfWork<IRepository<UserAccount>>(repository =>
            repository.InsertAsync(new UserAccount
            {
                Id = id,
                Name = name,
                Suffix = "0001",
                Password = "integration-test-password"
            }));
    }

    private async Task AddBindingAsync(Guid accountId, DiscordOAuthIdentity identity)
    {
        await RunWithUnitOfWork<IRepository<UserThirdParty>>(repository =>
            repository.InsertAsync(new UserThirdParty
            {
                Id = Guid.NewGuid(),
                UserId = accountId,
                Provider = Provider,
                ThirdPartyUserId = identity.UserId,
                AccessToken = identity.AccessToken,
                AccessTokenExpiresAt = identity.ExpiresAt,
                RefreshToken = identity.RefreshToken ?? string.Empty
            }));
    }

    private static OAuthStateRecord CreateState(
        OAuthStateFlow flow,
        Guid? initiatingAccountId = null)
    {
        var issuedAt = DateTimeOffset.UtcNow;
        return new OAuthStateRecord
        {
            State = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_')
                .PadRight(43, 'a'),
            Platform = AppPlatformEnum.Discord,
            Flow = flow,
            IssuedAt = issuedAt,
            ExpiresAt = issuedAt.AddMinutes(5),
            InitiatingAccountId = initiatingAccountId
        };
    }

    private static DiscordOAuthIdentity CreateIdentity(
        string userId,
        string accessToken = "discord-access-token",
        string refreshToken = "discord-refresh-token")
    {
        var expiresAt = DateTimeOffset.FromUnixTimeMilliseconds(
            DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeMilliseconds());

        return new DiscordOAuthIdentity(
            userId,
            "Discord User",
            accessToken,
            refreshToken,
            expiresAt);
    }

    private static string ExtractQueryParameter(string url, string parameterName)
    {
        var query = new Uri(url).Query.TrimStart('?');
        var parameter = query
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(item => item.Split('=', 2))
            .Single(item => Uri.UnescapeDataString(item[0]) == parameterName);

        return Uri.UnescapeDataString(parameter[1]);
    }

    private static void AssertBinding(
        Void.Bot.Message.Dto.Account.UserThirdPartyDto? binding,
        Guid accountId,
        DiscordOAuthIdentity identity)
    {
        binding.ShouldNotBeNull();
        binding.UserId.ShouldBe(accountId);
        binding.Provider.ShouldBe(Provider);
        binding.ThirdPartyUserId.ShouldBe(identity.UserId);
        binding.AccessToken.ShouldBe(identity.AccessToken);
        binding.RefreshToken.ShouldBe(identity.RefreshToken);
        binding.AccessTokenExpiresAt.ShouldBe(identity.ExpiresAt);
    }

    private static void AssertError(
        OAuthCallbackResponse response,
        HttpStatusCode statusCode,
        string errorCode)
    {
        response.Code.ShouldBe(statusCode);
        response.Message.ShouldBe(errorCode);
        response.Data.ShouldBeNull();
    }
}
