using Autofac;
using Shouldly;
using TestStack.BDDfy;
using Void.Bot.Core.Resources.i18n;
using Void.Bot.Core.Services.Language;
using Void.Bot.IntegrationTest.Services.Language;
using Void.Bot.Message.Enum;

namespace Void.Bot.IntegrationTest.BDD.Language;

public class GetContextFromFileScenario(IntegrationTestBase testBase) : IntegrationFixture(testBase), IDisposable
{
    private string _tempRootPath = Path.Combine(Path.GetTempPath(), "VoidBot", "BDD", Guid.NewGuid().ToString());
    private string _yamlContent = "Greeting: Loaded From File";
    private string _result = string.Empty;

    private async Task GivenFileExists()
    {
        var relativePath = Path.Combine("Resources", "i18n", "Test", $"ShouldlySample.{LanguageEnum.Default}.yaml");
        var fullPath = Path.Combine(_tempRootPath, relativePath);

        var directory = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);

        await File.WriteAllTextAsync(fullPath, _yamlContent);
    }

    private async Task WhenServiceStartsAndGetContext()
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

    private Task ThenResultIsFromFile()
    {
        _result.ShouldBe("Loaded From File");
        return Task.CompletedTask;
    }

    [Fact]
    public void Execute()
    {
        this.Given(x => x.GivenFileExists())
            .When(x => x.WhenServiceStartsAndGetContext())
            .Then(x => x.ThenResultIsFromFile())
            .BDDfy();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRootPath)) try { Directory.Delete(_tempRootPath, true); } catch { }
    }
}