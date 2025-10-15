using Discord;
using Discord.WebSocket;
using Mediator.Net;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Dto.Authorization;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Account;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Core.Services.Discord.SlashCommand;

public class LinkSlashCommand(IMediator mediator, IAccountService accountService) : IGlobalSlashCommand
{
    public bool ShouldDefer => true;

    public SlashCommandProperties Command => new SlashCommandBuilder()
        .WithName("link")
        .WithDescription("绑定平台账号.")
        .AddOption(new SlashCommandOptionBuilder()
            .WithName(nameof(AppPlatformEnum.Void).ToLower())
            .WithDescription("绑定Void账号.")
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption("账号名称", ApplicationCommandOptionType.String, "填写示例: Void#0000")
            .AddOption("密码", ApplicationCommandOptionType.String, "填写示例: a1b2C3D4"))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName(nameof(AppPlatformEnum.Bungie).ToLower())
            .WithDescription("绑定Bungie账号.")
            .WithType(ApplicationCommandOptionType.SubCommand))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName(nameof(AppPlatformEnum.Steam).ToLower())
            .WithDescription("绑定Steam账号.")
            .WithType(ApplicationCommandOptionType.SubCommand))
        .Build();

    public async Task ExecuteAsync(SocketSlashCommand commandCallback, CancellationToken cancellationToken = default)
    {
        var authorization = new AuthorizationDto
        {
            Platform = AppPlatformEnum.Discord,
            AccountId = commandCallback.User.Id.ToString(),
            AccountName = commandCallback.User.GlobalName
        };
        
        var platform = commandCallback.Data.Options.First().Name.ToLower();
        if (platform.Equals("void"))
        {
            var masterDisplayName = commandCallback.Data.Options.First().Options.FirstOrDefault()?.Value.ToString();
            var password = commandCallback.Data.Options.First().Options.Skip(1).FirstOrDefault()?.Value.ToString();
            
            if (string.IsNullOrEmpty(masterDisplayName) || string.IsNullOrEmpty(password))
            {
                await commandCallback.ModifyOriginalResponseAsync(builder =>
                {
                    builder.Content = "请填写账号名称和密码.";
                }).ConfigureAwait(false);
                
                return;
            }

            var userAccount = await accountService.GetUserAccountAsync(
                new GetUserAccountRequest { DisplayName = masterDisplayName }, cancellationToken).ConfigureAwait(false);
            
            if (userAccount.Data == null)
            {
                await commandCallback.ModifyOriginalResponseAsync(builder =>
                {
                    builder.Content = "账号不存在.";
                }).ConfigureAwait(false);
                
                return;
            }
            
            var isCorrectPassword = await accountService.VerifyPassword(
                userAccount.Data.Id, password, cancellationToken).ConfigureAwait(false);

            if (!isCorrectPassword)
            {
                await commandCallback.ModifyOriginalResponseAsync(builder =>
                {
                    builder.Content = "验证失败.";
                }).ConfigureAwait(false);
                
                return;
            }
            
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
                    builder.Content = $"绑定{AppPlatformEnum.Void}成功!";
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
        
        var findVoidAccountResponse = await mediator.RequestAsync<GetUserAccountByThirdPartyRequest, GetUserAccountByThirdPartyResponse>(
            new GetUserAccountByThirdPartyRequest
            {
                Platform = authorization.Platform,
                ThirdPartyUserId = authorization.AccountId
            }, cancellationToken).ConfigureAwait(false);

        if (findVoidAccountResponse.Data == null)
        {
            await commandCallback.ModifyOriginalResponseAsync(builder =>
            {
                builder.Content = $"请先绑定{AppPlatformEnum.Void}账号.";
            }).ConfigureAwait(false);
            
            return;
        }
        
        var url = string.Empty;
        switch (platform)
        {
            case "bungie":
            {
                var response = await mediator.RequestAsync<GenerateAuthenticateUrlFromBungieRequest, GenerateAuthenticateUrlFromBungieResponse>(
                    new GenerateAuthenticateUrlFromBungieRequest
                    {
                        Authorization = authorization
                    }, cancellationToken).ConfigureAwait(false);
                
                url = response.Data ?? "error";
                
                break;
            }
            case "steam":
            {
                var response = await mediator.RequestAsync<GenerateAuthenticateUrlFromSteamRequest, GenerateAuthenticateUrlFromSteamResponse>(
                    new GenerateAuthenticateUrlFromSteamRequest
                    {
                        Authorization = authorization
                    }, cancellationToken).ConfigureAwait(false);
                
                url = response.Data ?? "error";
                
                break;
            }
        }
        
        await commandCallback.ModifyOriginalResponseAsync(builder =>
        {
            builder.Content = $"请点击以下链接进行绑定: \r" + url;
        }).ConfigureAwait(false);
    }
}