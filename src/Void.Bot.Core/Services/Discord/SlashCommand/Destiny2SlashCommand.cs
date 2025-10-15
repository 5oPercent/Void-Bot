using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Mediator.Net;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Services.Discord.DataProvider;
using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Enum.Bungie;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Services.Discord.SlashCommand;

public partial class Destiny2SlashCommand(IMediator mediator, IDiscordDataProvider dataProvider) : IGuildSlashCommand
{
    public IEnumerable<ulong> GuildIds => [ 857616168549351474 ];

    public bool ShouldDefer => true;

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
        var commandName = commandCallback.Data.Options.First().Name;
        var findBungieName = commandCallback.Data.Options.First().Options.FirstOrDefault(x => x.Name.Equals("BungieName", StringComparison.OrdinalIgnoreCase))?.Value.ToString();
        var findMembership = commandCallback.Data.Options.First().Options.FirstOrDefault(x => x.Name.Equals("Platform", StringComparison.OrdinalIgnoreCase))?.Value.ToEnum<BungieMembershipTypeEnum>();

        if (!string.IsNullOrEmpty(findBungieName) && !BungieIdRegex().IsMatch(findBungieName))
        {
            await commandCallback.ModifyOriginalResponseAsync(builder =>
            {
                builder.Content = $"请输入正确的 BungieName, 示例: {BungieConstants.StandardGuardianName}.";
            }).ConfigureAwait(false);
            
            return;
        }
        
        var parts = findBungieName?.Split('#', StringSplitOptions.RemoveEmptyEntries);
        
        var profileResponse = await mediator.RequestAsync<GetDestiny2BaseProfileRequest, GetDestiny2BaseProfileResponse>(
            new GetDestiny2BaseProfileRequest
            {
                DisplayName = parts?[0] ?? string.Empty,
                DisplayNameCode = parts?[1] ?? string.Empty,
                MembershipType = findMembership ?? BungieMembershipTypeEnum.None
            }, cancellationToken).ConfigureAwait(false);

        if (profileResponse.Data == null)
        {
            await commandCallback.ModifyOriginalResponseAsync(builder =>
            {
                builder.Content = $"{profileResponse.Message}";
            }).ConfigureAwait(false);
            
            return;
        }
        
        var displayName = profileResponse.Data.DisplayName;
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
                var response = await mediator.RequestAsync<GetDestiny2CareerRequest, GetDestiny2CareerResponse>(
                    new GetDestiny2CareerRequest
                    {
                        DisplayName = parts?[0] ?? string.Empty,
                        DisplayNameCode = parts?[1] ?? string.Empty,
                        MembershipType = membershipType
                    }, cancellationToken).ConfigureAwait(false);
                
                if (response.Data == null)
                {
                    await commandCallback.ModifyOriginalResponseAsync(builder =>
                    {
                        builder.Content = $"{response.Message}";
                    }).ConfigureAwait(false);
                    
                    return;
                }
                
                var result = await dataProvider.GenerateDestiny2CareerAsync(response.Data, cancellationToken).ConfigureAwait(false);

                await commandCallback.ModifyOriginalResponseAsync(builder =>
                {
                    builder.Content = $"查询到 `{displayName}` 的生涯信息.";
                    builder.Embeds = result.Embeds.ToArray();
                    //builder.Attachments = result.Attachments;
                }).ConfigureAwait(false);
                break;
            }
        }
    }

    [GeneratedRegex(BungieConstants.IdPattern)]
    private static partial Regex BungieIdRegex();
}