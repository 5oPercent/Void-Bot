using Discord;
using Discord.WebSocket;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Core.Services.Steam;
using Void.Bot.Message.Dto.Authorization;
using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Services.Discord.SlashCommand;

public class LinkSlashCommand : IGlobalSlashCommand
{
    public SlashCommandProperties Command => new SlashCommandBuilder()
        .WithName("绑定")
        .WithDescription("绑定平台账号.")
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("平台")
            .WithDescription("选择一个平台.")
            .WithRequired(true)
            .AddChoice("Steam", (long)AppPlatformEnum.Steam)
            .AddChoice("Bungie", (long)AppPlatformEnum.Bungie)
            .WithType(ApplicationCommandOptionType.Integer))
        .Build();

    private readonly ISteamService _steamService;
    private readonly IBungieService _bungieService;

    public LinkSlashCommand(ISteamService steamService, IBungieService bungieService)
    {
        _steamService = steamService;
        _bungieService = bungieService;
    }
    
    public async Task ExecuteAsync(SocketSlashCommand commandCallback, CancellationToken cancellationToken = default)
    {
        var authorization = new AuthorizationDto
        {
            Platform = AppPlatformEnum.Discord,
            AccountId = commandCallback.User.Id.ToString()
        };
        
        var platformId = (long)commandCallback.Data.Options.FirstOrDefault(o => o.Name == "平台")!.Value;
        var url = (AppPlatformEnum)platformId switch
        {
            AppPlatformEnum.Steam => _steamService.GenerateAuthenticateUrl(authorization),
            AppPlatformEnum.Bungie => _bungieService.GenerateAuthenticateUrl(authorization),
            _ => "error"
        };
        
        await commandCallback.RespondAsync("请点击以下链接进行绑定: \r" + url, ephemeral: true).ConfigureAwait(false);
    }
}