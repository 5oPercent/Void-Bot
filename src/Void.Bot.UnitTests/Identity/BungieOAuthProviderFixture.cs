using System.Net;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Core.Services.Bungie.OAuth;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Settings.OAuth;
using Void.Bot.Message.Commands.Bungie;
using Void.Bot.Message.Commands.Identity;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Dto.Bungie;
using Void.Bot.Message.Dto.Bungie.Destiny2;
using Void.Bot.Message.Dto.Identity;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Identity;
using Void.Bot.Message.Request.Bungie.Destiny2;
using Void.Bot.Message.Request.Identity;
using ILogger = Serilog.ILogger;

namespace Void.Bot.UnitTest.Identity;

public class BungieOAuthProviderFixture
{
    private static readonly Guid CurrentUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public async Task AuthorizeAuthenticatedAccountCreatesLinkStateAsync()
    {
        var stateStore = Substitute.For<IOAuthStateStore>();
        var state = CreateState(OAuthStateFlow.Link, CurrentUserId);
        stateStore.CreateAsync(Arg.Any<CreateOAuthStateRequest>(), Arg.Any<CancellationToken>())
            .Returns(new CreateOAuthStateResponse.Created(state));
        var bungieService = Substitute.For<IBungieService>();
        bungieService.GenerateOAuthAuthorizationUrl(state.State).Returns("https://bungie.example/authorize");
        var provider = CreateProvider(stateStore, bungieService);

        var response = await provider.AuthorizeAsync(CurrentUserId, CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.Platform.ShouldBe(nameof(AppPlatformEnum.Bungie));
        response.Data.RedirectUrl.ShouldBe("https://bungie.example/authorize");
        await stateStore.Received(1).CreateAsync(
            Arg.Is<CreateOAuthStateRequest>(request =>
                request.Platform == AppPlatformEnum.Bungie &&
                request.Flow == OAuthStateFlow.Link &&
                request.InitiatingAccountId == CurrentUserId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CallbackExistingIdentityRefreshesBindingAndReturnsJwtAsync()
    {
        var stateStore = Substitute.For<IOAuthStateStore>();
        var state = CreateState(OAuthStateFlow.Login);
        stateStore.TakeAsync(Arg.Any<TakeOAuthStateRequest>(), Arg.Any<CancellationToken>())
            .Returns(new TakeOAuthStateResponse.Found(state));
        var bungieService = CreateAuthenticatedBungieService();
        var accountDataProvider = Substitute.For<IAccountDataProvider>();
        var binding = new UserThirdPartyDto
        {
            Id = Guid.NewGuid(),
            UserId = CurrentUserId,
            Provider = nameof(AppPlatformEnum.Bungie),
            ThirdPartyUserId = "123456"
        };
        accountDataProvider.GetThirdPartyBindingAsync(
                nameof(AppPlatformEnum.Bungie),
                "123456",
                Arg.Any<CancellationToken>())
            .Returns(binding);
        accountDataProvider.GetUserAccountAsync(CurrentUserId, string.Empty, Arg.Any<CancellationToken>())
            .Returns(new UserAccountDto
            {
                Id = CurrentUserId,
                Name = "Void User",
                Suffix = "0001"
            });
        var tokenClaimService = Substitute.For<ITokenClaimService>();
        tokenClaimService.GenerateAccessToken(Arg.Any<IEnumerable<System.Security.Claims.Claim>>())
            .Returns("void-access-token");
        tokenClaimService.GenerateRefreshToken(Arg.Any<IEnumerable<System.Security.Claims.Claim>>())
            .Returns("void-refresh-token");
        var provider = CreateProvider(
            stateStore,
            bungieService,
            accountDataProvider: accountDataProvider,
            tokenClaimService: tokenClaimService);

        var response = await provider.CallbackAsync(
            new OAuthCallbackContext("oauth-code", state.State, null),
            CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.Platform.ShouldBe(nameof(AppPlatformEnum.Bungie));
        response.Data.Flow.ShouldBe(OAuthStateFlow.Login);
        response.Data.ProviderUserId.ShouldBe("123456");
        response.Data.ProviderUserName.ShouldBe("Guardian#1234");
        response.Data.AccessToken.ShouldBe("void-access-token");
        response.Data.RefreshToken.ShouldBe("void-refresh-token");
        binding.AccessToken.ShouldBe("bungie-access-token");
        binding.RefreshToken.ShouldBe("bungie-refresh-token");
        await accountDataProvider.Received(1).UpdateThirdPartyAsync(binding, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LinkCallbackDoesNotGenerateVoidTokensAsync()
    {
        var stateStore = Substitute.For<IOAuthStateStore>();
        var state = CreateState(OAuthStateFlow.Link, CurrentUserId);
        stateStore.TakeAsync(Arg.Any<TakeOAuthStateRequest>(), Arg.Any<CancellationToken>())
            .Returns(new TakeOAuthStateResponse.Found(state));
        var accountDataProvider = Substitute.For<IAccountDataProvider>();
        accountDataProvider.GetUserAccountAsync(CurrentUserId, string.Empty, Arg.Any<CancellationToken>())
            .Returns(new UserAccountDto { Id = CurrentUserId, Name = "Void User", Suffix = "0001" });
        var tokenClaimService = Substitute.For<ITokenClaimService>();
        var provider = CreateProvider(
            stateStore,
            CreateAuthenticatedBungieService(),
            accountDataProvider: accountDataProvider,
            tokenClaimService: tokenClaimService);

        var response = await provider.CallbackAsync(
            new OAuthCallbackContext("oauth-code", state.State, null),
            CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.Flow.ShouldBe(OAuthStateFlow.Link);
        response.Data.AccessToken.ShouldBeEmpty();
        response.Data.RefreshToken.ShouldBeEmpty();
        tokenClaimService.DidNotReceiveWithAnyArgs().GenerateAccessToken(default!);
        tokenClaimService.DidNotReceiveWithAnyArgs().GenerateRefreshToken(default!);
        await accountDataProvider.Received(1).LinkThirdPartyAsync(
            Arg.Is<UserThirdPartyDto>(binding =>
                binding.UserId == CurrentUserId &&
                binding.Provider == nameof(AppPlatformEnum.Bungie) &&
                binding.ThirdPartyUserId == "123456"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CallbackIdentityOwnedByAnotherAccountReturnsConflictAsync()
    {
        var stateStore = Substitute.For<IOAuthStateStore>();
        var state = CreateState(OAuthStateFlow.Link, CurrentUserId);
        stateStore.TakeAsync(Arg.Any<TakeOAuthStateRequest>(), Arg.Any<CancellationToken>())
            .Returns(new TakeOAuthStateResponse.Found(state));
        var bungieService = CreateAuthenticatedBungieService();
        var accountDataProvider = Substitute.For<IAccountDataProvider>();
        accountDataProvider.GetUserAccountAsync(CurrentUserId, string.Empty, Arg.Any<CancellationToken>())
            .Returns(new UserAccountDto { Id = CurrentUserId, Name = "Void User", Suffix = "0001" });
        accountDataProvider.GetThirdPartyBindingAsync(
                nameof(AppPlatformEnum.Bungie),
                "123456",
                Arg.Any<CancellationToken>())
            .Returns(new UserThirdPartyDto
            {
                UserId = Guid.NewGuid(),
                Provider = nameof(AppPlatformEnum.Bungie),
                ThirdPartyUserId = "123456"
            });
        var provider = CreateProvider(stateStore, bungieService, accountDataProvider: accountDataProvider);

        var response = await provider.CallbackAsync(
            new OAuthCallbackContext("oauth-code", state.State, null),
            CancellationToken.None);

        response.Code.ShouldBe(HttpStatusCode.Conflict);
        response.Message.ShouldBe(OAuthError.BindingConflict.Code);
        response.Data.ShouldBeNull();
    }

    [Fact]
    public async Task UnlinkDeletesBungieBindingAsync()
    {
        var accountService = Substitute.For<IAccountService>();
        var accountDataProvider = Substitute.For<IAccountDataProvider>();
        accountDataProvider.GetThirdPartyByUserIdAsync(
                CurrentUserId,
                nameof(AppPlatformEnum.Bungie),
                Arg.Any<CancellationToken>())
            .Returns(new UserThirdPartyDto
            {
                UserId = CurrentUserId,
                Provider = nameof(AppPlatformEnum.Bungie),
                ThirdPartyUserId = "123456"
            });
        var provider = CreateProvider(
            Substitute.For<IOAuthStateStore>(),
            Substitute.For<IBungieService>(),
            accountService,
            accountDataProvider);

        var response = await provider.UnlinkAsync(CurrentUserId, CancellationToken.None);

        await accountService.Received(1).UnlinkAccountFromThirdPartyAsync(
            Arg.Is<Void.Bot.Message.Commands.Account.UnlinkUserAccountFromThirdPartyCommand>(command =>
                command.UserId == CurrentUserId && command.Platform == AppPlatformEnum.Bungie),
            Arg.Any<CancellationToken>());
    }

    private static BungieOAuthProvider CreateProvider(
        IOAuthStateStore stateStore,
        IBungieService bungieService,
        IAccountService? accountService = null,
        IAccountDataProvider? accountDataProvider = null,
        ITokenClaimService? tokenClaimService = null)
    {
        var setting = new OAuthSetting(new ConfigurationBuilder().Build())
        {
            StateTtlSeconds = OAuthSetting.DefaultStateTtlSeconds
        };

        return new BungieOAuthProvider(
            stateStore,
            bungieService,
            setting,
            accountService ?? Substitute.For<IAccountService>(),
            accountDataProvider ?? Substitute.For<IAccountDataProvider>(),
            tokenClaimService ?? Substitute.For<ITokenClaimService>(),
            Substitute.For<ILogger>());
    }

    private static IBungieService CreateAuthenticatedBungieService()
    {
        var bungieService = Substitute.For<IBungieService>();
        bungieService.GenerateTokenAsync(
                Arg.Any<GenerateBungieTokenCommand>(),
                Arg.Any<CancellationToken>())
            .Returns(new GenerateBungieTokenResponse
            {
                Code = HttpStatusCode.OK,
                Data = new BungieApiGenerateTokenResponse
                {
                    MembershipId = "123456",
                    AccessToken = "bungie-access-token",
                    AccessTokenExpiresIn = 3600,
                    RefreshToken = "bungie-refresh-token",
                    RefreshExpiresIn = 7200
                }
            });
        bungieService.GetDestiny2LinkedProfileAsync(
                Arg.Any<GetDestiny2LinkedProfileRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(new GetDestiny2LinkedProfileResponse
            {
                Data = new Destiny2LinkedProfilesResponse
                {
                    Response = new Destiny2LinkedProfilesDataDto
                    {
                        Profiles = [new Destiny2LinkedProfileItemDto()],
                        BungieNetMembershipDto = new Void.Bot.Message.Dto.Bungie.BungieApiLinkedProfileNetMembershipDto
                        {
                            SupplementalDisplayName = "Guardian#1234"
                        }
                    }
                }
            });

        return bungieService;
    }

    private static OAuthStateRecord CreateState(OAuthStateFlow flow, Guid? initiatingAccountId = null)
    {
        var issuedAt = DateTimeOffset.UtcNow;
        return new OAuthStateRecord
        {
            State = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
            Platform = AppPlatformEnum.Bungie,
            Flow = flow,
            IssuedAt = issuedAt,
            ExpiresAt = issuedAt.AddMinutes(5),
            InitiatingAccountId = initiatingAccountId
        };
    }
}
