using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Core.Services.Discord.DataProvider;
using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Enum.Bungie;
using Void.Bot.Message.Request.Bungie;

namespace Void.Bot.Core.Services.Discord.SlashCommand;

public class Destiny2SlashCommand(IBungieService bungieService, IDiscordDataProvider dataProvider) : IGuildSlashCommand
{
    public IEnumerable<ulong> GuildIds => [ 857616168549351474 ];
    
    public SlashCommandProperties Command => new SlashCommandBuilder()
        .WithName("d2")
        .WithDescription("提供一系列 Destiny2 相关指令.")
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("生涯")
            .WithDescription("查询玩家生涯信息.")
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddDestinyPlayerOptions())
        .Build();
    
    public async Task ExecuteAsync(SocketSlashCommand commandCallback, CancellationToken cancellationToken = default)
    {
        await commandCallback.DeferAsync().ConfigureAwait(false);
        
        var commandName = commandCallback.Data.Options.First().Name;
        var findBungieName = commandCallback.Data.Options.First().Options.FirstOrDefault(x => x.Name.Equals("BungieName", StringComparison.OrdinalIgnoreCase))?.Value.ToString();
        var findMembership = commandCallback.Data.Options.First().Options.FirstOrDefault(x => x.Name.Equals("Platform", StringComparison.OrdinalIgnoreCase))?.Value.ToEnum<BungieMembershipType>();

        if (!string.IsNullOrEmpty(findBungieName) && !Regex.IsMatch(findBungieName, BungieConstants.IdPattern))
        {
            await commandCallback.ModifyOriginalResponseAsync(builder =>
            {
                builder.Content = $"请输入正确的 BungieName, 示例: {BungieConstants.StandardGuardianName}.";
            }).ConfigureAwait(false);
            
            return;
        }
        
        var profileResponse = await bungieService.GetDestinyBaseProfileAsync(
            new GetDestinyBaseProfileRequest
            {
                FindBungieName = findBungieName,
                MembershipType = findMembership
            }, cancellationToken).ConfigureAwait(false);

        if (profileResponse.Data == null)
        {
            await commandCallback.ModifyOriginalResponseAsync(builder =>
            {
                builder.Content = $"{profileResponse.Message}";
            }).ConfigureAwait(false);
            
            return;
        }
        
        var isSinglePlatform = profileResponse.Data.IsSinglePlatform;
        var onCrossSave = profileResponse.Data.OnCrossSave;
        var membershipType = profileResponse.Data.MembershipType;
        var membershipId = profileResponse.Data.MembershipId;
        
        if (!isSinglePlatform && !onCrossSave && findMembership == null)
        {
            await commandCallback.ModifyOriginalResponseAsync(builder =>
            {
                builder.Content = $"查询到未启用跨平台, 需要补充游玩平台参数.";
            }).ConfigureAwait(false);
            return;
        }
        
        switch (commandName)
        {
            case "生涯":
            {
                var request = new GetDestinyCareerRequest
                {
                    MembershipType = membershipType,
                    MembershipId = membershipId
                };
                
                //var embed = await dataProvider.Destiny2CareerBuildEmbedAsync(request, cancellationToken).ConfigureAwait(false);
                
                var imageStream = await dataProvider.Destiny2CareerGenerateImageAsync(request, cancellationToken).ConfigureAwait(false);

                var fileList = new List<FileAttachment>();
                if (imageStream is not null)
                {
                    fileList.Add(new FileAttachment(imageStream, "destiny2_career.png"));
                }
                await commandCallback.ModifyOriginalResponseAsync(builder =>
                {
                    builder.Content = $"查询到生涯信息.";
                    //builder.Embed = embed;
                    builder.Attachments = fileList;
                }).ConfigureAwait(false);
                
                break;
            }
        }
    }
}