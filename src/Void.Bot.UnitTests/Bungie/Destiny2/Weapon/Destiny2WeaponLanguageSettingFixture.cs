using Microsoft.Extensions.Configuration;
using Shouldly;
using Void.Bot.Core.Settings.Bungie;

namespace Void.Bot.UnitTest.Bungie.Destiny2.Weapon;

public class Destiny2WeaponLanguageSettingFixture
{
    [Fact]
    public void ShouldReadSupportedAndDefaultLanguagesFromConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Destiny2:SyncLanguages:0"] = "en",
                ["Destiny2:SyncLanguages:1"] = "zh-cht"
            })
            .Build();

        var setting = new Destiny2Setting(configuration);

        setting.SupportedLanguages.SetEquals(["en", "zh-cht"]).ShouldBeTrue();
        setting.DefaultLanguage.ShouldBe("en");
    }

    [Fact]
    public void ShouldPreferSimplifiedChineseWhenConfigured()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Destiny2:SyncLanguages:0"] = "en",
                ["Destiny2:SyncLanguages:1"] = "zh-chs"
            })
            .Build();

        var setting = new Destiny2Setting(configuration);

        setting.DefaultLanguage.ShouldBe("zh-chs");
    }
}
