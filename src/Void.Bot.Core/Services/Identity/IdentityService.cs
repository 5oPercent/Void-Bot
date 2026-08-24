using System.Net;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Commands.Identity;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Dto.Authorization;
using Void.Bot.Message.Dto.Identity;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Account;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Core.Services.Identity;

public interface IIdentityService : IScope
{
    Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request, CancellationToken cancellationToken);

    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
    
    Task<LoginSteamResponse> LinkToSteamAsync(LoginSteamCommand command, CancellationToken cancellationToken);
}

public class IdentityService : IIdentityService
{
    private readonly ILogger _logger;
    private readonly ITokenClaimService _tokenClaimService;
    private readonly IAccountService _accountService;

    public IdentityService(ILogger logger, ITokenClaimService tokenClaimService, IAccountService accountService)
    {
        _logger = logger;
        _tokenClaimService = tokenClaimService;
        _accountService = accountService;
    }

    private List<Claim> GetUserAllClaimsAsync(UserAccountInfoDto user, CancellationToken cancellationToken)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("token_version", user.TokenVersion.ToString())
        };
        
        return claims;
    }
    
    public async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request, CancellationToken cancellationToken)
    {
        var response = new AuthenticateResponse();
        
        var getUserAccountResponse = await _accountService.GetUserAccountAsync(
            new GetUserAccountRequest
            {
                DisplayName = $"{request.Name}#{request.Suffix}"
            }, cancellationToken).ConfigureAwait(false);

        if (getUserAccountResponse.Data == null)
        {
            response.Code = HttpStatusCode.InternalServerError;
            response.Message = "用户不存在";
            return response;
        }
        
        var userAccount = getUserAccountResponse.Data;
        
        var result = await _accountService.VerifyPassword(userAccount.Id, request.Password, cancellationToken).ConfigureAwait(false);
        if (!result)
        {
            response.Code = HttpStatusCode.InternalServerError;
            response.Message = "密码错误";
            return response;
        }
        
        var claims = GetUserAllClaimsAsync(userAccount, cancellationToken);
        var accessToken = _tokenClaimService.GenerateAccessToken(claims);
        var refreshToken = _tokenClaimService.GenerateRefreshToken(claims);
        
        response.Data = new JwtToken
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
        return response;
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var response = new RefreshTokenResponse();

        ClaimsPrincipal principal;
        try
        {
            principal = _tokenClaimService.ValidateRefreshToken(request.RefreshToken);
        }
        catch (Exception exception) when (exception is SecurityTokenException or ArgumentException)
        {
            response.Code = HttpStatusCode.Unauthorized;
            response.Message = "刷新令牌无效或已过期";
            return response;
        }

        if (!Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            response.Code = HttpStatusCode.Unauthorized;
            response.Message = "刷新令牌无效";
            return response;
        }

        var accountResponse = await _accountService.GetUserAccountAsync(
            new GetUserAccountRequest { UserId = userId },
            cancellationToken).ConfigureAwait(false);
        if (accountResponse.Data == null)
        {
            response.Code = HttpStatusCode.Unauthorized;
            response.Message = "用户不存在";
            return response;
        }

        var tokenVersion = 0;
        var tokenVersionClaim = principal.FindFirstValue("token_version");
        if ((!string.IsNullOrEmpty(tokenVersionClaim) && !int.TryParse(tokenVersionClaim, out tokenVersion)) ||
            tokenVersion != accountResponse.Data.TokenVersion)
        {
            response.Code = HttpStatusCode.Unauthorized;
            response.Message = "刷新令牌已失效";
            return response;
        }

        var claims = GetUserAllClaimsAsync(accountResponse.Data, cancellationToken);
        response.Data = new JwtToken
        {
            AccessToken = _tokenClaimService.GenerateAccessToken(claims),
            RefreshToken = _tokenClaimService.GenerateRefreshToken(claims)
        };

        return response;
    }

    public async Task<LoginSteamResponse> LinkToSteamAsync(LoginSteamCommand command, CancellationToken cancellationToken)
    {
        var claimedId = command.OpenId.Claimed_Id;
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
                MasterDisplayName = findVoidAccountResponse.Data.DisplayName
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

}
