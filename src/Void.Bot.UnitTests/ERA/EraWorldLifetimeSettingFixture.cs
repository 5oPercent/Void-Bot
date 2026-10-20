using Microsoft.Extensions.Configuration;
using Shouldly;
using Void.Bot.Core.Settings.ERA;

namespace Void.Bot.UnitTest.ERA;

public sealed class EraWorldLifetimeSettingFixture
{
    [Fact]
    public void Should_default_to_the_requested_sixty_second_grace_and_allow_configuration()
    {
        var defaults = new EraWorldLifetimeSetting(new ConfigurationBuilder().Build());
        defaults.InactivityTimeoutSeconds.ShouldBe(60);
        defaults.HeartbeatIntervalSeconds.ShouldBe(15);
        var configured = new EraWorldLifetimeSetting(new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["Era:World:InactivityTimeoutSeconds"] = "300" }).Build());
        configured.InactivityTimeoutSeconds.ShouldBe(300);
    }

    [Theory]
    [InlineData("InactivityTimeoutSeconds", "30")]
    [InlineData("HeartbeatIntervalSeconds", "0")]
    [InlineData("ScanIntervalSeconds", "0")]
    [InlineData("MissingConfirmationSeconds", "1")]
    [InlineData("BatchSize", "257")]
    public void Should_reject_policies_that_cannot_safely_confirm_inactivity(string name, string value)
    {
        Should.Throw<InvalidOperationException>(() => new EraWorldLifetimeSetting(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Era:World:" + name] = value }).Build()));
    }
}
