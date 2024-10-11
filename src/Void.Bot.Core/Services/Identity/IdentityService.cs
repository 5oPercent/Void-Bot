using Mediator.Net;
using Serilog;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Dto.Authorization;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Account;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Core.Services.Identity;

public interface IIdentityService : IScope
{
    Task<LoginSteamResponse> LinkToSteamAsync(LoginSteamRequest request, CancellationToken cancellationToken);
    
    Task<LoginBungieResponse> LinkToBungieAsync(LoginBungieRequest request, CancellationToken cancellationToken);
}

public class IdentityService : IIdentityService
{
    private readonly ILogger _logger;
    private readonly IMediator _mediator;
    private readonly IAccountService _accountService;

    public IdentityService(ILogger logger, IMediator mediator, IAccountService accountService)
    {
        _logger = logger;
        _mediator = mediator;
        _accountService = accountService;
    }
    
    public async Task<LoginSteamResponse> LinkToSteamAsync(LoginSteamRequest request, CancellationToken cancellationToken)
    {
        var claimedId = request.OpenID.Claimed_id;
        if (claimedId == null)
        {
            throw new Exception($"绑定{AppPlatformEnum.Steam}失败, Steam ID 未找到");
        }
        
        var lastSlashIndex = claimedId.LastIndexOf('/');
        var steamId = claimedId[(lastSlashIndex + 1)..];
        
        var findVoidAccountResponse = await _accountService.GetUserAccountByThirdPartyAsync(new GetUserAccountByThirdPartyRequest
        {
            Platform = request.Authorization.Platform,
            ThirdPartyUserId = request.Authorization.AccountId
        }, cancellationToken);

        if (findVoidAccountResponse.Data == null)
        {
            throw new Exception($"绑定{AppPlatformEnum.Steam}失败, 请先绑定{AppPlatformEnum.Void}.");
        }

        await _mediator.SendAsync<LinkUserAccountFromThirdPartyCommand, LinkUserAccountFromThirdPartyResponse>(
            new LinkUserAccountFromThirdPartyCommand
            {
                Authorization = new AuthorizationDto
                {
                    Platform = AppPlatformEnum.Steam,
                    AccountId = steamId
                },
                MasterDisplayName = findVoidAccountResponse.Data.DisplayName
            }, cancellationToken).ConfigureAwait(false);

        _logger.Debug($"Void: {findVoidAccountResponse.Data.DisplayName}\n" +
                      $"Platform: {request.Authorization.Platform}\n" +
                      $"Account ID: {request.Authorization.AccountId}\n" +
                      $"Steam ID: {steamId}");
        
        return new LoginSteamResponse
        {
            Data = "绑定成功"
        };
    }

    public async Task<LoginBungieResponse> LinkToBungieAsync(LoginBungieRequest request, CancellationToken cancellationToken)
    {
        var stateArray = request.State.Split('|');
        if (stateArray.Length != 2)
        {
            throw new Exception("绑定失败, State 格式错误");
        }

        var isParse = Enum.TryParse<AppPlatformEnum>(stateArray[0].Replace("Platform:", ""), true, out var platform);
        var accountId = stateArray[1].Replace("AccountId:", "");
        
        if (!isParse || string.IsNullOrEmpty(accountId))
        {
            throw new Exception("绑定失败, Platform 或 Account ID 为空");
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
        
        await _mediator.SendAsync<LinkUserAccountFromThirdPartyCommand, LinkUserAccountFromThirdPartyResponse>(
            new LinkUserAccountFromThirdPartyCommand
            {
                Authorization = new AuthorizationDto
                {
                    Platform = AppPlatformEnum.Bungie,
                    AccountId = request.Code
                },
                MasterDisplayName = findVoidAccountResponse.Data.DisplayName
            }, cancellationToken).ConfigureAwait(false);
        
        _logger.Debug($"Void: {findVoidAccountResponse.Data.DisplayName}\n" +
                            $"Platform: {platform}\n" +
                            $"Account ID: {accountId}\n" +
                            $"Bungie OAuth Code: {request.Code}");

        return new LoginBungieResponse
        {
            Data = "绑定成功"
        };
    }
}