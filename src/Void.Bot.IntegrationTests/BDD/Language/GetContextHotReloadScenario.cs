using Autofac;
using Shouldly;
using TestStack.BDDfy;
using Void.Bot.Core.Resources.i18n;
using Void.Bot.Core.Services.Language;
using Void.Bot.IntegrationTest.Services.Language;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Extension;

namespace Void.Bot.IntegrationTest.BDD.Language;

public class GetContextHotReloadScenario(IntegrationTestBase testBase) : IntegrationFixture(testBase), IDisposable
{
    private string _tempRootPath = Path.Combine(Path.GetTempPath(), "VoidBotBDD", Guid.NewGuid().ToString());
    private string _initialContent = "Greeting: Initial Value";
    private string _updatedContent = "Greeting: Updated Runtime";
    private string _englishContent = "Greeting: English Runtime";
    private string _result = string.Empty;
    private string _fullFilePath = string.Empty;
    private string _renamedFilePath = string.Empty;

    private async Task GivenServiceRunningWithFile()
    {
        var relativePath = Path.Combine("Resources", "i18n", "Test", $"ShouldlySample.{LanguageEnum.Default.ToLanguageCode()}.yaml");
        _fullFilePath = Path.Combine(_tempRootPath, relativePath);

        var directory = Path.GetDirectoryName(_fullFilePath);
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory!);
        
        await File.WriteAllTextAsync(_fullFilePath, _initialContent);
    }

    private async Task WhenModifyFileRuntime()
    {
        await Run<ILanguageService>(async service =>
        {
            var initial = service.GetContext<ShouldlyTestTemplate>(x => x.Greeting);
            initial.ShouldBe("Initial Value");

            await File.WriteAllTextAsync(_fullFilePath, _updatedContent);

            await Task.Delay(500);

            _result = service.GetContext<ShouldlyTestTemplate>(x => x.Greeting);
        },
        builder =>
        {
            builder.RegisterType<ShouldlyTestTemplate>().As<IContextTemplate>();
            builder.RegisterType<LanguageService>().As<ILanguageService>().WithParameter("rootPath", _tempRootPath);
        });
    }

    private async Task WhenRenameFileRuntime()
    {
        await Run<ILanguageService>(async service =>
        {
            var initial = service.GetContext<ShouldlyTestTemplate>(x => x.Greeting);
            initial.ShouldBe("Initial Value");

            _renamedFilePath = Path.Combine(
                Path.GetDirectoryName(_fullFilePath)!,
                $"ShouldlySample.{LanguageEnum.En.ToLanguageCode()}.yaml");

            File.Move(_fullFilePath, _renamedFilePath);
            await File.WriteAllTextAsync(_renamedFilePath, _englishContent);

            await Task.Delay(500);

            _result = service.GetContext<ShouldlyTestTemplate>(x => x.Greeting);
        },
        builder =>
        {
            builder.RegisterType<ShouldlyTestTemplate>().As<IContextTemplate>();
            builder.RegisterType<LanguageService>().As<ILanguageService>().WithParameter("rootPath", _tempRootPath);
        });
    }

    private Task ThenResultIsUpdated()
    {
        _result.ShouldBe("Updated Runtime");
        return Task.CompletedTask;
    }

    private Task ThenResultFallbackToTemplate()
    {
        _result.ShouldBe("Hello Shouldly");
        return Task.CompletedTask;
    }

    [Fact]
    public void Execute()
    {
        this.Given(x => x.GivenServiceRunningWithFile())
            .When(x => x.WhenModifyFileRuntime())
            .Then(x => x.ThenResultIsUpdated())
            .BDDfy();
    }

    [Fact]
    public void RenameFile()
    {
        this.Given(x => x.GivenServiceRunningWithFile())
            .When(x => x.WhenRenameFileRuntime())
            .Then(x => x.ThenResultFallbackToTemplate())
            .BDDfy();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRootPath)) try { Directory.Delete(_tempRootPath, true); } catch { }
    }
}
