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
}
