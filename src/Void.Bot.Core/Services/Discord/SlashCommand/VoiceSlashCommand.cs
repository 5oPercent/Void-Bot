using Discord;
using Discord.WebSocket;
using Void.Bot.Core.Resources.i18n.Discord;
using Void.Bot.Core.Services.Language;

namespace Void.Bot.Core.Services.Discord.SlashCommand;

public class VoiceSlashCommand(
    IDiscordVoiceChannelService voiceChannelService,
    ILocalizationService localizationService) : IGlobalSlashCommand
{
    public bool Ephemeral => true;

    public SlashCommandProperties Command => new SlashCommandBuilder()
        .WithName("voice")
        .WithDescription("管理专属语音频道.")
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("limit")
            .WithDescription("设置频道人数上限.")
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption("count", ApplicationCommandOptionType.Integer, "人数范围 3-50.", isRequired: true))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("private")
            .WithDescription("将频道设置为私密.")
            .WithType(ApplicationCommandOptionType.SubCommand))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("public")
            .WithDescription("将频道设置为公开.")
            .WithType(ApplicationCommandOptionType.SubCommand))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("allow")
            .WithDescription("允许用户查看和加入频道.")
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption("user", ApplicationCommandOptionType.User, "要允许的用户.", isRequired: true))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("deny")
            .WithDescription("移除用户允许权限并踢出频道.")
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption("user", ApplicationCommandOptionType.User, "要移除的用户.", isRequired: true))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("kick")
            .WithDescription("踢出频道内用户.")
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption("user", ApplicationCommandOptionType.User, "要踢出的用户.", isRequired: true))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("delete")
            .WithDescription("立即删除自己的专属语音频道.")
            .WithType(ApplicationCommandOptionType.SubCommand))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("info")
            .WithDescription("查看专属语音频道信息.")
            .WithType(ApplicationCommandOptionType.SubCommand))
        .Build();

    public async Task ExecuteAsync(SocketSlashCommand commandCallback, CancellationToken cancellationToken = default)
    {
        if (commandCallback.User is not SocketGuildUser user)
        {
            await ModifyResponseAsync(commandCallback, localizationService.GetValue<DiscordContextTemplate>(x => x.GuildOnlyCommand, commandCallback.UserLocale)).ConfigureAwait(false);
            return;
        }

        var subCommand = commandCallback.Data.Options.FirstOrDefault();
        if (subCommand == null)
        {
            await ModifyResponseAsync(commandCallback, localizationService.GetValue<DiscordVoiceContextTemplate>(x => x.UnknownVoiceCommand, commandCallback.UserLocale)).ConfigureAwait(false);
            return;
        }

        var result = subCommand.Name switch
        {
            "limit" => await voiceChannelService.SetLimitAsync(user, GetIntOption(subCommand, "count"), cancellationToken).ConfigureAwait(false),
            "private" => await voiceChannelService.SetPrivacyAsync(user, true, cancellationToken).ConfigureAwait(false),
            "public" => await voiceChannelService.SetPrivacyAsync(user, false, cancellationToken).ConfigureAwait(false),
            "allow" => await ExecuteWithTargetUserAsync(user, subCommand, voiceChannelService.AllowUserAsync, cancellationToken).ConfigureAwait(false),
            "deny" => await ExecuteWithTargetUserAsync(user, subCommand, voiceChannelService.DenyUserAsync, cancellationToken).ConfigureAwait(false),
            "kick" => await ExecuteWithTargetUserAsync(user, subCommand, voiceChannelService.KickUserAsync, cancellationToken).ConfigureAwait(false),
            "delete" => await voiceChannelService.DeleteOwnChannelAsync(user, cancellationToken).ConfigureAwait(false),
            "info" => await voiceChannelService.GetInfoAsync(user, cancellationToken).ConfigureAwait(false),
            _ => new DiscordVoiceCommandResult { Message = localizationService.GetValue<DiscordVoiceContextTemplate>(x => x.UnknownVoiceCommand, commandCallback.UserLocale) }
        };

        await ModifyResponseAsync(commandCallback, result.Message).ConfigureAwait(false);
    }

    private static async Task<DiscordVoiceCommandResult> ExecuteWithTargetUserAsync(
        SocketGuildUser user,
        IApplicationCommandInteractionDataOption subCommand,
        Func<SocketGuildUser, SocketGuildUser, CancellationToken, Task<DiscordVoiceCommandResult>> executeAsync,
        CancellationToken cancellationToken)
    {
        var targetUser = GetUserOption(user.Guild, subCommand, "user");
        if (targetUser == null)
        {
            return new DiscordVoiceCommandResult { Message = "未找到目标用户." };
        }

        if (targetUser.IsBot)
        {
            return new DiscordVoiceCommandResult { Message = "不能对 bot 用户执行该操作." };
        }

        return await executeAsync(user, targetUser, cancellationToken).ConfigureAwait(false);
    }

    private static int GetIntOption(IApplicationCommandInteractionDataOption subCommand, string name)
    {
        var value = subCommand.Options.FirstOrDefault(x => x.Name == name)?.Value;
        return Convert.ToInt32(value);
    }

    private static SocketGuildUser? GetUserOption(SocketGuild guild, IApplicationCommandInteractionDataOption subCommand, string name)
    {
        var value = subCommand.Options.FirstOrDefault(x => x.Name == name)?.Value;
        return value switch
        {
            SocketGuildUser guildUser => guildUser,
            SocketUser socketUser => guild.GetUser(socketUser.Id),
            IUser discordUser => guild.GetUser(discordUser.Id),
            _ => null
        };
    }

    private static async Task ModifyResponseAsync(SocketSlashCommand commandCallback, string message)
    {
        await commandCallback.ModifyOriginalResponseAsync(builder =>
        {
            builder.Content = message;
        }).ConfigureAwait(false);
    }
}
