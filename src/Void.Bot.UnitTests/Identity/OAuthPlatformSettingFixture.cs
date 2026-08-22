using Microsoft.Extensions.Configuration;
using Shouldly;
using Void.Bot.Core.Settings.Discord;
using Void.Bot.Core.Settings.OAuth;

namespace Void.Bot.UnitTest.Identity;

public class OAuthPlatformSettingFixture
{
    [Fact]
    public void ShouldKeepWorkflowAndPlatformSettingsSeparate()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OAuth:StateTtlSeconds"] = "600",
                ["OAuth:ExchangeTtlSeconds"] = "120",
                ["OAuth:FrontendCallbackUrl"] = "https://voidbot.example/oauth/callback",
                ["Discord:ClientId"] = "discord-client-id",
                ["Discord:ClientSecret"] = "discord-client-secret"
            })
            .Build();

        var oauthSetting = new OAuthSetting(configuration);
        var discordSetting = new DiscordSetting(configuration);

        oauthSetting.StateTtlSeconds.ShouldBe(600);
        oauthSetting.ExchangeTtlSeconds.ShouldBe(120);
        oauthSetting.FrontendCallbackUrl.ShouldBe("https://voidbot.example/oauth/callback");
        discordSetting.ClientId.ShouldBe("discord-client-id");
        discordSetting.ClientSecret.ShouldBe("discord-client-secret");
    }
}
