using Shouldly;
using Void.Bot.Core.Services.Http;

namespace Void.Bot.IntegrationTest.Services.Http;

public class HttpClientFixture(IntegrationTestBase testBase) : IntegrationFixture(testBase)
{
    [Fact]
    public async Task RequestAsync()
    {
        await Run<IVoidBotHttpClientFactory>(async factory =>
        {
            var response = await factory.GetAsync<string>("http://www.baidu.com", default);

            response.Data.ShouldNotBeNull();
        });
    }
}