using Mediator.Net;
using Shouldly;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.IntegrationTest.Services.Identity;

public class IdentityFixture() : IntegrationFixture("identity", "identity_test")
{
    [Fact]
    public async Task RegisterUserAccountAsync()
    {
        await Run<IMediator>(async mediator =>
        {
            var userName = "test";
            
            var response = await mediator.SendAsync<RegisterUserAccountCommand, RegisterUserAccountResponse>(
                new RegisterUserAccountCommand
                {
                    Name = userName,
                    Password = "".GenerateRandomString()
                }).ConfigureAwait(false);
            
            response.ShouldNotBeNull();
            response.Data.ShouldNotBeNull();
            response.Data.Name.ShouldBe(userName);
        });
    }
}