using Shouldly;
using TestStack.BDDfy;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.IntegrationTest.BDD.Identity;

public class RegisterAndLoginUserScenario(IntegrationTestBase testBase) : IntegrationFixture(testBase)
{
    private string _userName { get; set; } = "test";
    private string _password { get; set; } = "".GenerateRandomString();
    private string _suffix { get; set; } = string.Empty;
    private string _accessToken { get; set; } = string.Empty;
    private string _refreshToken { get; set; } = string.Empty;
    
    private async Task GivenRegister()
    {
        await RunWithUnitOfWork<IAccountService>(async service =>
        {
            var response = await service.RegisterUserAccountAsync(
                new RegisterUserAccountCommand
                {
                    Name = _userName,
                    Password = _password
                }, CancellationToken.None).ConfigureAwait(false);

            if (response is { Data: not null })
            {
                _suffix = response.Data.Suffix;
            }
        });
    }
    
    private async Task WhenLogin()
    {
        await Run<IIdentityService>(async service =>
        {
            var response = await service.AuthenticateAsync(
                new AuthenticateRequest
                {
                    Name = _userName,
                    Password = _password,
                    Suffix = _suffix
                }, CancellationToken.None).ConfigureAwait(false);
            
            if (response is { Data: not null })
            {
                _accessToken = response.Data.AccessToken;
                _refreshToken = response.Data.RefreshToken;
            }
        });
    }

    private Task ThenLogged()
    {
        _accessToken.ShouldNotBeNullOrEmpty();
        _refreshToken.ShouldNotBeNullOrEmpty();
        
        return Task.CompletedTask;
    }

    [Fact]
    public void Execute()
    {
        this.Given(x => x.GivenRegister())
            .When(x => x.WhenLogin())
            .Then(x => x.ThenLogged())
            .BDDfy();
    }
}