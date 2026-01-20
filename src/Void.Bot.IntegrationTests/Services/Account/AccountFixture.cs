using Mediator.Net;
using Shouldly;
using Void.Bot.Core.Extension;
using Void.Bot.IntegrationTest.Utils;
using Void.Bot.Message.Request.Account;

namespace Void.Bot.IntegrationTest.Services.Account;

public class AccountFixture : IntegrationFixture
{
    private readonly AccountUtil _util;

    public AccountFixture(IntegrationTestBase testBase) : base(testBase)
    {
        _util = new AccountUtil(testBase);
    }
    
    [Fact]
    public async Task GetUserAccountPageAsync()
    {
        await _util.AddUserAccountAsync("".GenerateRandomString(), "".GenerateNumberSuffix());
        await _util.AddUserAccountAsync("".GenerateRandomString(), "".GenerateNumberSuffix());
        await _util.AddUserAccountAsync("".GenerateRandomString(), "".GenerateNumberSuffix());
        
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
}