using Discord;
using Discord.WebSocket;

namespace Void.Bot.Core.Services.Discord.SlashCommand;

public class PingSlashCommand : IGuildSlashCommand
{
    public IEnumerable<ulong> GuildIds => [ 857616168549351474 ];

    public bool Ephemeral => true;

    public SlashCommandProperties Command => new SlashCommandBuilder()
        .WithName("ping")
        .WithDescription("测试斜杠命令.")
        .Build();

    public async Task ExecuteAsync(SocketSlashCommand commandCallback, CancellationToken cancellationToken = default)
    {
        await commandCallback.ModifyOriginalResponseAsync(builder =>
        {
            builder.Content = "Pong!";
        }).ConfigureAwait(false);
    }
}