using Microsoft.Extensions.Configuration;
using Shouldly;
using Void.Bot.Core.Settings.Bungie;

namespace Void.Bot.UnitTest.Bungie.Destiny2;

public class Destiny2SettingFixture
{
    [Fact]
    public void ShouldUseDefaultRaidDungeonSettings()
    {
        var setting = new Destiny2Setting(new ConfigurationBuilder().Build());

        setting.CachePlayerDataMinutes.ShouldBe(5);
        setting.CacheRaidDungeonStatsMinutes.ShouldBe(5);
        setting.RaidDungeonPlayerStartThresholdSeconds.ShouldBe(60);
    }

    [Fact]
    public void ShouldReadRaidDungeonSettingsFromConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Destiny2:Cache:PlayerDataMinutes"] = "10",
                ["Destiny2:Cache:RaidDungeonStatsMinutes"] = "15",
                ["Destiny2:RaidDungeon:PlayerStartThresholdSeconds"] = "90"
            })
            .Build();

        var setting = new Destiny2Setting(configuration);

        setting.CachePlayerDataMinutes.ShouldBe(10);
        setting.CacheRaidDungeonStatsMinutes.ShouldBe(15);
        setting.RaidDungeonPlayerStartThresholdSeconds.ShouldBe(90);
    }
}
