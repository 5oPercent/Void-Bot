using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Mediator.Net;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Enum.Bungie;
using Void.Bot.Message.Request.Bungie;

namespace Void.Bot.Core.Services.Discord.SlashCommand;

public class Destiny2SlashCommand(IMediator mediator) : IGuildSlashCommand
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
        
        var profileResponse = await mediator.RequestAsync<GetDestinyBaseProfileRequest, GetDestinyBaseProfileResponse>(
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
                var result = await mediator.RequestAsync<GetDestinyCareerRequest, GetDestinyCareerResponse>(
                    new GetDestinyCareerRequest
                    {
                        MembershipType = membershipType,
                        MembershipId = membershipId
                    }, cancellationToken).ConfigureAwait(false);

                if (result.Data == null)
                {
                    await commandCallback.ModifyOriginalResponseAsync(builder =>
                    {
                        builder.Content = "查询异常: " + result.Message;
                    }).ConfigureAwait(false);
                    return;
                }

                var profileInfo = result.Data.Response.Profile.Data;
                var commendationInfo = result.Data.Response.ProfileCommendations.Data;

                var globalDisplayName = $"{profileInfo.UserInfo.BungieGlobalDisplayName}#{profileInfo.UserInfo.BungieGlobalDisplayNameCode}";
                var targetPlatform = profileInfo.UserInfo.ApplicableMembershipTypes.Select(x => x.ToEnum<BungieMembershipType>().ToString()).ToList();
                
                var embed = new EmbedBuilder()
                    .WithColor(139,139,255)
                    .WithTitle($"{globalDisplayName} 的生涯信息")
                    .AddField("当前赛季:", ">>>>>>>", true)
                        .AddField("守护者等级", $"`{profileInfo.CurrentGuardianRank}`", true)
                        .AddField("硬光上限", $"`{profileInfo.CurrentSeasonRewardPowerCap}`", true)
                    .AddField("历史生涯:", ">>>>>>>", true)
                        .AddField("最高守护者等级", $"`{profileInfo.LifetimeHighestGuardianRank}`", true)
                        .AddField("嘉奖点赞", $"`{commendationInfo.TotalScore}`", true)
                    .AddField("其它数据", ">>>>>>>", true)
                        .AddField("角色数量", $"`{profileInfo.CharacterIds.Count}`", true)
                        .AddField("遊玩平台", $"`{string.Join(",", targetPlatform)}`", true)
                    .WithFooter("By Void Bot")
                    .Build();
                
                await commandCallback.ModifyOriginalResponseAsync(builder =>
                {
                    builder.Embed = embed;
                }).ConfigureAwait(false);
                
                break;
            }
        }
    }
}