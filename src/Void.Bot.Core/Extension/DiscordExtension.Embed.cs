using Discord;

namespace Void.Bot.Core.Extension;

public static partial class DiscordExtension
{
    public static Embed BuildWithFooter(this EmbedBuilder builder, string footer = "By Void Bot")
    {
        return builder.WithFooter(footer).Build();
    }
}