using Autofac;
using Shouldly;
using TestStack.BDDfy;
using Void.Bot.Core.Resources.i18n;
using Void.Bot.Core.Services.Language;
using Void.Bot.IntegrationTest.Services.Language;

namespace Void.Bot.IntegrationTest.BDD.Language;

public class GetContextFallbackScenario(IntegrationTestBase testBase) : IntegrationFixture(testBase), IDisposable
{
    private string _tempRootPath = Path.Combine(Path.GetTempPath(), "VoidBot", "BDD", Guid.NewGuid().ToString());
    private string _result = string.Empty;

    private async Task GivenServiceRegistered()
    {
        if (Directory.Exists(_tempRootPath))
            Directory.Delete(_tempRootPath, true);
            
        await Task.CompletedTask;
    }

    private async Task WhenGetContext()
    {
        await Run<ILanguageService>(async service =>
            {
                _result = service.GetContext<ShouldlyTestTemplate>(x => x.Greeting);
                await Task.CompletedTask;
            },
            builder =>
            {
                builder.RegisterType<ShouldlyTestTemplate>().As<IContextTemplate>();
                builder.RegisterType<LanguageService>().As<ILanguageService>().WithParameter("rootPath", _tempRootPath);
            });
    }

    private Task ThenResultIsDefault()
    {
        _result.ShouldBe("Hello Shouldly");
        return Task.CompletedTask;
    }

    [Fact]
    public void Execute()
    {
        this.Given(x => x.GivenServiceRegistered())
            .When(x => x.WhenGetContext())
            .Then(x => x.ThenResultIsDefault())
            .BDDfy();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRootPath)) try { Directory.Delete(_tempRootPath, true); } catch { }
    }
}