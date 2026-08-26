using Mediator.Net;
using Shouldly;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Account;
using Void.Bot.IntegrationTest.Utils;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Account;

namespace Void.Bot.IntegrationTest.Services.Account;

public class AccountFixture(IntegrationTestBase testBase) : IntegrationFixture(testBase)
{
    private readonly AccountUtil _util = new(testBase);

    #region Get UserAccount
    
    [Fact]
    public async Task GetUserAccountPageAsync()
    {
        await _util.AddUserAccountAsync();
        await _util.AddUserAccountAsync();
        await _util.AddUserAccountAsync();
        
        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.RequestAsync<GetUserAccountPageRequest, GetUserAccountPageResponse>(
                new GetUserAccountPageRequest
                {
                    PageIndex = 1,
                    PageSize = 20
                }).ConfigureAwait(false);

            response.ShouldNotBeNull();
            response.Data.ShouldNotBeNull();
            response.Data.Total.ShouldBe(3);
            response.Data.Items.ShouldNotBeNull();
            response.Data.Items.Count().ShouldBe(3);
        });
    }

    [Fact]
    public async Task GetUserAccountPageIncludesThirdPartyBindingsAsync()
    {
        var userId = await _util.AddUserAccountAsync();
        await RunWithUnitOfWork<IRepository<UserThirdParty>>(repository =>
            repository.InsertAsync(new UserThirdParty
            {
                UserId = userId,
                Provider = nameof(AppPlatformEnum.Discord),
                ThirdPartyUserId = "discord-user",
                AccessToken = "access-token",
                RefreshToken = "refresh-token"
            }));

        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.RequestAsync<GetUserAccountPageRequest, GetUserAccountPageResponse>(
                new GetUserAccountPageRequest
                {
                    PageIndex = 1,
                    PageSize = 20
                }).ConfigureAwait(false);

            response.Data.ShouldNotBeNull();
            var user = response.Data.Items.ShouldHaveSingleItem();
            var thirdParty = user.ThirdParties.ShouldHaveSingleItem();
            thirdParty.Provider.ShouldBe(nameof(AppPlatformEnum.Discord));
            thirdParty.ThirdPartyUserId.ShouldBe("discord-user");
        });
    }
    
    [Fact]
    public async Task GetEmptyUserAccountPageAsync()
    {
        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.RequestAsync<GetUserAccountPageRequest, GetUserAccountPageResponse>(
                new GetUserAccountPageRequest
                {
                    PageIndex = 1,
                    PageSize = 20
                }).ConfigureAwait(false);

            response.ShouldNotBeNull();
            response.Data.ShouldNotBeNull();
            response.Data.Total.ShouldBe(0);
            response.Data.Items.ShouldBeEmpty();
        });
    }
    
    [Fact]
    public async Task GetUserAccountPageWithFilterAsync()
    {
        var id = await _util.AddUserAccountAsync();
        await _util.AddUserAccountAsync();

        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.RequestAsync<GetUserAccountPageRequest, GetUserAccountPageResponse>(
                new GetUserAccountPageRequest
                {
                    UserIds = [ id ],
                    PageIndex = 1,
                    PageSize = 20
                }).ConfigureAwait(false);

            response.ShouldNotBeNull();
            response.Data.ShouldNotBeNull();
            response.Data.Total.ShouldBe(1);
            response.Data.Items.First().Id.ShouldBe(id);
        });
    }
    
    [Fact]
    public async Task GetUserAccountPagePaginationAsync()
    {
        for (var i = 0; i < 5; i++)
        {
            await _util.AddUserAccountAsync();
        }

        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.RequestAsync<GetUserAccountPageRequest, GetUserAccountPageResponse>(
                new GetUserAccountPageRequest
                {
                    PageIndex = 1,
                    PageSize = 2
                }).ConfigureAwait(false);

            response.ShouldNotBeNull();
            response.Data.ShouldNotBeNull();
            response.Data.Total.ShouldBe(5);
            response.Data.Items.Count().ShouldBe(2);

            var lastPageResponse = await mediator.RequestAsync<GetUserAccountPageRequest, GetUserAccountPageResponse>(
                new GetUserAccountPageRequest
                {
                    PageIndex = 3,
                    PageSize = 2
                }).ConfigureAwait(false);

            lastPageResponse.ShouldNotBeNull();
            lastPageResponse.Data.ShouldNotBeNull();
            lastPageResponse.Data.Items.Count().ShouldBe(1);
        });
    }
    
    [Fact]
    public async Task GetUserAccountPageEdgeCasesAsync()
    {
        await _util.AddUserAccountAsync();

        await Run<IMediator>(async mediator =>
        {
            await Should.ThrowAsync<ArgumentOutOfRangeException>(() =>
                mediator.RequestAsync<GetUserAccountPageRequest, GetUserAccountPageResponse>(
                    new GetUserAccountPageRequest
                    {
                        PageIndex = 0,
                        PageSize = 50
                    }));
        });
    }

    [Fact]
    public async Task GetUserAccountPagePaginationMetadataAsync()
    {
        await _util.AddUserAccountsAsync(5);

        await Run<IMediator>(async mediator =>
        {
            var firstPageResponse = await mediator.RequestAsync<GetUserAccountPageRequest, GetUserAccountPageResponse>(
                new GetUserAccountPageRequest
                {
                    PageIndex = 1,
                    PageSize = 2
                }).ConfigureAwait(false);

            firstPageResponse.ShouldNotBeNull();
            firstPageResponse.Data.ShouldNotBeNull();
            firstPageResponse.Data.Total.ShouldBe(5);
            firstPageResponse.Data.TotalPages.ShouldBe(3);
            firstPageResponse.Data.PageIndex.ShouldBe(1);
            firstPageResponse.Data.PageSize.ShouldBe(2);
            firstPageResponse.Data.HasPrevPage.ShouldBeFalse();
            firstPageResponse.Data.HasNextPage.ShouldBeTrue();
            firstPageResponse.Data.Items.Count().ShouldBe(2);

            var secondPageResponse = await mediator.RequestAsync<GetUserAccountPageRequest, GetUserAccountPageResponse>(
                new GetUserAccountPageRequest
                {
                    PageIndex = 2,
                    PageSize = 2
                }).ConfigureAwait(false);

            secondPageResponse.ShouldNotBeNull();
            secondPageResponse.Data.ShouldNotBeNull();
            secondPageResponse.Data.HasPrevPage.ShouldBeTrue();
            secondPageResponse.Data.HasNextPage.ShouldBeTrue();

            var lastPageResponse = await mediator.RequestAsync<GetUserAccountPageRequest, GetUserAccountPageResponse>(
                new GetUserAccountPageRequest
                {
                    PageIndex = 3,
                    PageSize = 2
                }).ConfigureAwait(false);

            lastPageResponse.ShouldNotBeNull();
            lastPageResponse.Data.ShouldNotBeNull();
            lastPageResponse.Data.HasPrevPage.ShouldBeTrue();
            lastPageResponse.Data.HasNextPage.ShouldBeFalse();
        });
    }

    [Fact]
    public async Task GetUserAccountPageBeyondTotalPagesAsync()
    {
        await _util.AddUserAccountsAsync(3);

        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.RequestAsync<GetUserAccountPageRequest, GetUserAccountPageResponse>(
                new GetUserAccountPageRequest
                {
                    PageIndex = 100,
                    PageSize = 2
                }).ConfigureAwait(false);

            response.ShouldNotBeNull();
            response.Data.ShouldNotBeNull();
            response.Data.Total.ShouldBe(3);
            response.Data.TotalPages.ShouldBe(2);
            response.Data.Items.ShouldBeEmpty();
        });
    }

    [Fact]
    public async Task GetUserAccountPageEmptyUserIdsAsync()
    {
        await _util.AddUserAccountsAsync(3);

        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.RequestAsync<GetUserAccountPageRequest, GetUserAccountPageResponse>(
                new GetUserAccountPageRequest
                {
                    UserIds = [],
                    PageIndex = 1,
                    PageSize = 20
                }).ConfigureAwait(false);

            response.ShouldNotBeNull();
            response.Data.ShouldNotBeNull();
            response.Data.Total.ShouldBe(3);
            response.Data.Items.Count().ShouldBe(3);
        });
    }

    [Fact]
    public async Task GetUserAccountPageWithNonExistentUserIdAsync()
    {
        await _util.AddUserAccountsAsync(2);

        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.RequestAsync<GetUserAccountPageRequest, GetUserAccountPageResponse>(
                new GetUserAccountPageRequest
                {
                    UserIds = [Guid.NewGuid()],
                    PageIndex = 1,
                    PageSize = 20
                }).ConfigureAwait(false);

            response.ShouldNotBeNull();
            response.Data.ShouldNotBeNull();
            response.Data.Total.ShouldBe(0);
            response.Data.Items.ShouldBeEmpty();
        });
    }

    [Fact]
    public async Task GetUserAccountPageWithDuplicateUserIdsAsync()
    {
        var ids = await _util.AddUserAccountsAsync(3);

        await Run<IMediator>(async mediator =>
        {
            var duplicatedUserIds = new List<Guid> { ids[0], ids[0], ids[1] };

            var response = await mediator.RequestAsync<GetUserAccountPageRequest, GetUserAccountPageResponse>(
                new GetUserAccountPageRequest
                {
                    UserIds = duplicatedUserIds,
                    PageIndex = 1,
                    PageSize = 20
                }).ConfigureAwait(false);

            response.ShouldNotBeNull();
            response.Data.ShouldNotBeNull();
            response.Data.Total.ShouldBe(2);
            response.Data.Items.Count().ShouldBe(2);
        });
    }

    #endregion
}
