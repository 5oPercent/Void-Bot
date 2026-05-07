using Autofac;
using Shouldly;
using Void.Bot.Core.Resources.i18n;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Extension;

namespace Void.Bot.IntegrationTest.Services.Language;

public class LanguageFixture(IntegrationTestBase testBase) : IntegrationFixture(testBase)
{
    [Fact]
    public async Task GenerateDefaultYamlFile_CheckPathAndContent()
    {
        var tempRootPath = Path.Combine(Path.GetTempPath(), "VoidBotTests", Guid.NewGuid().ToString());
        var relativePath = Path.Combine("Resources", "i18n", "Test", $"ShouldlySample.{LanguageEnum.Default.ToLanguageCode()}.yaml");
        var fullPath = Path.Combine(tempRootPath, relativePath);

        try
        {
            await Run<ILanguageService>(async service =>
            {
                await service.GenerateDefaultYamlFileAsync(CancellationToken.None);

                File.Exists(fullPath).ShouldBeTrue($"Write Path: {fullPath}");

                var content = await File.ReadAllTextAsync(fullPath);

                content.ShouldNotBeNullOrEmpty();
                content.ShouldContain("Hello Shouldly");
            },
            builder =>
            {
                builder.RegisterType<ShouldlyTestTemplate>().As<IContextTemplate>();
                builder.RegisterType<LanguageService>().As<ILanguageService>().WithParameter("rootPath", tempRootPath);
            });
        }
        finally
        {
            if (Directory.Exists(tempRootPath)) try { Directory.Delete(tempRootPath, true); } catch { }
        }
    }

    [Fact]
    public async Task GenerateDefaultYamlFile_ShouldNotOverwriteExistingFileByDefault()
    {
        var tempRootPath = Path.Combine(Path.GetTempPath(), "VoidBotTests", Guid.NewGuid().ToString());
        var relativePath = Path.Combine("Resources", "i18n", "Test", $"ShouldlySample.{LanguageEnum.Default.ToLanguageCode()}.yaml");
        var fullPath = Path.Combine(tempRootPath, relativePath);
        var existingContent = "Greeting: Customized";

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            await File.WriteAllTextAsync(fullPath, existingContent);

            await Run<ILanguageService>(async service =>
            {
                await service.GenerateDefaultYamlFileAsync(CancellationToken.None);

                var content = await File.ReadAllTextAsync(fullPath);

                content.ShouldBe(existingContent);
            },
            builder =>
            {
                builder.RegisterType<ShouldlyTestTemplate>().As<IContextTemplate>();
                builder.RegisterType<LanguageService>().As<ILanguageService>().WithParameter("rootPath", tempRootPath);
            });
        }
        finally
        {
            if (Directory.Exists(tempRootPath)) try { Directory.Delete(tempRootPath, true); } catch { }
        }
    }

    [Fact]
    public async Task GenerateDefaultYamlFile_ShouldOverwriteExistingFileWhenEnabled()
    {
        var tempRootPath = Path.Combine(Path.GetTempPath(), "VoidBotTests", Guid.NewGuid().ToString());
        var relativePath = Path.Combine("Resources", "i18n", "Test", $"ShouldlySample.{LanguageEnum.Default.ToLanguageCode()}.yaml");
        var fullPath = Path.Combine(tempRootPath, relativePath);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            await File.WriteAllTextAsync(fullPath, "Greeting: Customized");

            await Run<ILanguageService>(async service =>
            {
                await service.GenerateDefaultYamlFileAsync(CancellationToken.None, overwrite: true);

                var content = await File.ReadAllTextAsync(fullPath);

                content.ShouldContain("Hello Shouldly");
            },
            builder =>
            {
                builder.RegisterType<ShouldlyTestTemplate>().As<IContextTemplate>();
                builder.RegisterType<LanguageService>().As<ILanguageService>().WithParameter("rootPath", tempRootPath);
            });
        }
        finally
        {
            if (Directory.Exists(tempRootPath)) try { Directory.Delete(tempRootPath, true); } catch { }
        }
    }
}
