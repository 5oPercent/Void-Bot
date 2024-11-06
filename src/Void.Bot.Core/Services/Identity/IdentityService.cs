using Serilog;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Commands.Bungie;
using Void.Bot.Message.Commands.Identity;
using Void.Bot.Message.Dto.Authorization;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Account;

namespace Void.Bot.Core.Services.Identity;

public interface IIdentityService : IScope
{
    Task<LoginSteamResponse> LinkToSteamAsync(LoginSteamCommand command, CancellationToken cancellationToken);
    
    Task<LoginBungieResponse> LinkToBungieAsync(LoginBungieCommand command, CancellationToken cancellationToken);
}

public class IdentityService : IIdentityService
{
    private readonly ILogger _logger;
    private readonly IAccountService _accountService;
    private readonly IBungieService _bungieService;

    public IdentityService(ILogger logger, IAccountService accountService, IBungieService bungieService)
    {
        _logger = logger;
        _accountService = accountService;
        _bungieService = bungieService;
    }
    
    public async Task<LoginSteamResponse> LinkToSteamAsync(LoginSteamCommand command, CancellationToken cancellationToken)
    {
        var claimedId = command.OpenID.Claimed_id;
        if (claimedId == null)
        {
            throw new Exception($"绑定{AppPlatformEnum.Steam}失败, 未找到 Steam ID.");
        }
        
        var lastSlashIndex = claimedId.LastIndexOf('/');
        var steamId = claimedId[(lastSlashIndex + 1)..];
        
        var findVoidAccountResponse = await _accountService.GetUserAccountByThirdPartyAsync(new GetUserAccountByThirdPartyRequest
        {
            Platform = command.Authorization.Platform,
            ThirdPartyUserId = command.Authorization.AccountId
        }, cancellationToken);

        if (findVoidAccountResponse.Data == null)
        {
            throw new Exception($"绑定{AppPlatformEnum.Steam}失败, 请先绑定{AppPlatformEnum.Void}.");
        }

        await _accountService.LinkAccountFromThirdPartyAsync(
            new LinkUserAccountFromThirdPartyCommand
            {
                Authorization = new AuthorizationDto
                {
                    Platform = AppPlatformEnum.Steam,
                    AccountId = steamId
                },
                MasterDisplayName = findVoidAccountResponse.Data.DisplayName,
                ExpiresAt = DateTimeOffset.Now.AddYears(3)
            }, cancellationToken).ConfigureAwait(false);
        
        _logger.Debug($"Void: {findVoidAccountResponse.Data.DisplayName}\n" +
                      $"Platform: {command.Authorization.Platform}\n" +
                      $"Account ID: {command.Authorization.AccountId}\n" +
                      $"Steam ID: {steamId}");
        
        return new LoginSteamResponse
        {
            Data = "绑定成功"
        };
    }

    public async Task<LoginBungieResponse> LinkToBungieAsync(LoginBungieCommand command, CancellationToken cancellationToken)
    {
        var stateArray = command.State.Split('|');
        if (stateArray.Length != 2)
        {
            throw new Exception($"绑定{AppPlatformEnum.Bungie}失败, State 格式错误.");
        }

        var isParse = Enum.TryParse<AppPlatformEnum>(stateArray[0].Replace("Platform:", ""), true, out var platform);
        var accountId = stateArray[1].Replace("AccountId:", "");
        
        if (!isParse || string.IsNullOrEmpty(accountId))
        {
            throw new Exception($"绑定{AppPlatformEnum.Bungie}失败, Platform 或 Account ID 为空.");
        }
        
        var findVoidAccountResponse = await _accountService.GetUserAccountByThirdPartyAsync(new GetUserAccountByThirdPartyRequest
        {
            Platform = platform,
            ThirdPartyUserId = accountId
        }, cancellationToken);

        if (findVoidAccountResponse.Data == null)
        {
            throw new Exception($"绑定{AppPlatformEnum.Bungie}失败, 请先绑定{AppPlatformEnum.Void}.");
        }
        
        var tokenResponse = await _bungieService.GenerateTokenAsync(
            new GenerateBungieTokenCommand
            {
                OAuthCode = command.Code
            }, cancellationToken).ConfigureAwait(false);

        if (tokenResponse.Data == null)
        {
            throw new Exception($"绑定{AppPlatformEnum.Bungie}失败, 生成 Token 异常: {tokenResponse.Message}.");
        }
        
        await _accountService.LinkAccountFromThirdPartyAsync(
            new LinkUserAccountFromThirdPartyCommand
            {
                Authorization = new AuthorizationDto
                {
                    Platform = AppPlatformEnum.Bungie,
                    AccountId = tokenResponse.Data.Membership_Id
                },
                MasterDisplayName = findVoidAccountResponse.Data.DisplayName,
                AccessToken = tokenResponse.Data.Access_Token,
                ExpiresAt = DateTimeOffset.Now.AddSeconds(tokenResponse.Data.Expires_In)
            }, cancellationToken).ConfigureAwait(false);

        _logger.Debug($"Void: {findVoidAccountResponse.Data.DisplayName}\n" +
                      $"Platform: {platform}\n" +
                      $"Account ID: {accountId}\n" +
                      $"Bungie OAuth Code: {command.Code}");

        return new LoginBungieResponse
        {
            Data = "绑定成功"
        };
    }
}