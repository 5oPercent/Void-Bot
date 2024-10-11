using Discord;
using Discord.WebSocket;
using Mediator.Net;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Dto.Authorization;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Core.Services.Discord.SlashCommand;

public class LinkSlashCommand(IMediator mediator) : IGlobalSlashCommand
{
    public SlashCommandProperties Command => new SlashCommandBuilder()
        .WithName("绑定")
        .WithDescription("绑定平台账号.")
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("平台")
            .WithDescription("选择一个平台.")
            .WithRequired(true)
            .AddChoice("Void", (long)AppPlatformEnum.Void)
            .AddChoice("Steam", (long)AppPlatformEnum.Steam)
            .AddChoice("Bungie", (long)AppPlatformEnum.Bungie)
            .WithType(ApplicationCommandOptionType.Integer))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("主账号完整名称")
            .WithDescription($"此选项仅在关联{AppPlatformEnum.Void.ToString()}平台时有效. 不提供则创建新Void账号. 填写示例: Void#0000")
            .WithRequired(false)
            .WithType(ApplicationCommandOptionType.String)
        )
        .Build();

    public async Task ExecuteAsync(SocketSlashCommand commandCallback, CancellationToken cancellationToken = default)
    {
        var authorization = new AuthorizationDto
        {
            Platform = AppPlatformEnum.Discord,
            AccountId = commandCallback.User.Id.ToString(),
            AccountName = commandCallback.User.GlobalName
        };
        
        var platformId = (long)commandCallback.Data.Options.FirstOrDefault(o => o.Name == "平台")!.Value;

        if ((AppPlatformEnum)platformId == AppPlatformEnum.Void)
        {
            var masterDisplayName = commandCallback.Data.Options.FirstOrDefault(o => o.Name == "主账号完整名称")?.Value.ToString();

            await commandCallback.DeferAsync(true).ConfigureAwait(false);

            try
            {
                await mediator.SendAsync<LinkUserAccountFromThirdPartyCommand, LinkUserAccountFromThirdPartyResponse>(
                    new LinkUserAccountFromThirdPartyCommand
                    {
                        Authorization = authorization,
                        MasterDisplayName = masterDisplayName
                    }, cancellationToken).ConfigureAwait(false);
                
                await commandCallback.ModifyOriginalResponseAsync(builder =>
                {
                    builder.Content = $"绑定{AppPlatformEnum.Void.ToString()}成功!";
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await commandCallback.ModifyOriginalResponseAsync(builder =>
                {
                    builder.Content = ex.Message;
                }).ConfigureAwait(false);
            }
            
            return;
        }

        string url;
        switch ((AppPlatformEnum)platformId)
        {
            case AppPlatformEnum.Steam:
            {
                var response = await mediator.RequestAsync<GenerateAuthenticateUrlFromSteamRequest, GenerateAuthenticateUrlFromSteamResponse>(new GenerateAuthenticateUrlFromSteamRequest
                {
                    Authorization = authorization
                }, cancellationToken).ConfigureAwait(false);
                
                url = response.Data ?? "error";
                
                break;
            }
            case AppPlatformEnum.Bungie:
            {
                var response = await mediator.RequestAsync<GenerateAuthenticateUrlFromBungieRequest, GenerateAuthenticateUrlFromBungieResponse>(new GenerateAuthenticateUrlFromBungieRequest
                {
                    Authorization = authorization
                }, cancellationToken).ConfigureAwait(false);
                
                url = response.Data ?? "error";
                
                break;
            }
            default:
                url = "error";
                break;
        }
        
        await commandCallback.RespondAsync("请点击以下链接进行绑定: \r" + url, ephemeral: true).ConfigureAwait(false);
    }
}