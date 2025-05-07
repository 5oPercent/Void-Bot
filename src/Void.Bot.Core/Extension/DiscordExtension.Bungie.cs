using Discord;
using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Core.Extension;

public static partial class DiscordExtension
{
    public static SlashCommandOptionBuilder AddDestinyPlayerOptions(this SlashCommandOptionBuilder builder)
    {
        return builder
            .AddOption("BungieName".ToLower(), ApplicationCommandOptionType.String, $"示例: {BungieConstants.StandardGuardianName}, 不提供则查询当前账号绑定数据.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("Platform".ToLower())
                .WithDescription("指定游玩平台, 已启用跨平台设置请忽略.")
                .WithType(ApplicationCommandOptionType.Integer)
                .AddChoice("steam", (long)BungieMembershipTypeEnum.Steam)
                .AddChoice("egs", (long)BungieMembershipTypeEnum.Egs)
                .AddChoice("xbox", (long)BungieMembershipTypeEnum.Xbox)
                .AddChoice("psn", (long)BungieMembershipTypeEnum.Psn));
    }
}