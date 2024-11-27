using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Services.Discord.SlashCommand
{
	public class HelloSlashCommand : IGuildSlashCommand
	{
		public IEnumerable<ulong> GuildIds => [857616168549351474];

		public SlashCommandProperties Command => new SlashCommandBuilder()
			.WithName("历史活动")
			.WithDescription("查询近50场游戏活动")
			.Build();

		public async Task ExecuteAsync(SocketSlashCommand commandCallback, CancellationToken cancellationToken = default)
		{
			//获取数据库的缓存的棒鸡ID

			//历史活动

			await commandCallback.RespondAsync("return").ConfigureAwait(false);
		}
	}
}
