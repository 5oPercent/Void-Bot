using Mediator.Net;
using Shouldly;
using Void.Bot.IntegrationTest.Utils;
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
            var response = await mediator.RequestAsync<GetUserAccountPageRequest, GetUserAccountPageResponse>(
                new GetUserAccountPageRequest
                {
                    PageIndex = 0,
                    PageSize = 0
                }).ConfigureAwait(false);

            response.ShouldNotBeNull();
            response.Data.ShouldNotBeNull();
        });
    }
    
    #endregion
}