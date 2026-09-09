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
using Void.Bot.Message.Enum.Account;
using Void.Bot.Message.Request.Account;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Core.Services.Identity;

public interface IIdentityService : IScope
{
    Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request, CancellationToken cancellationToken);

    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken);

    Task<VerifyTwoFactorResponse> VerifyTwoFactorAsync(
        VerifyTwoFactorCommand command, CancellationToken cancellationToken);
    
    Task<LoginSteamResponse> LinkToSteamAsync(LoginSteamCommand command, CancellationToken cancellationToken);
}

public class IdentityService : IIdentityService
{
    private readonly ILogger _logger;
    private readonly ITokenClaimService _tokenClaimService;
    private readonly IAccountService _accountService;
    private readonly IAccountTwoFactorService _accountTwoFactorService;
    private readonly ICurrentUser _currentUser;

    public IdentityService(
        ILogger logger,
        ITokenClaimService tokenClaimService,
        IAccountService accountService,
        IAccountTwoFactorService accountTwoFactorService,
        ICurrentUser currentUser)
    {
        _logger = logger;
        _tokenClaimService = tokenClaimService;
        _accountService = accountService;
        _accountTwoFactorService = accountTwoFactorService;
        _currentUser = currentUser;
    }

    private List<Claim> GetUserAllClaimsAsync(
        UserAccountInfoDto user,
        bool isTwoFactorAuthenticated = false)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("token_version", user.TokenVersion.ToString())
        };

        if (isTwoFactorAuthenticated)
        {
            claims.Add(new Claim(
                TokenClaimService.TwoFactorAuthenticationClaim,
                TokenClaimService.TwoFactorAuthenticationValue));
        }
        
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

        var twoFactorResult = await _accountTwoFactorService.VerifyLoginAsync(
            userAccount.Id, request.TwoFactorCode, cancellationToken).ConfigureAwait(false);
        if (twoFactorResult == TwoFactorLoginVerificationResult.CodeRequired)
        {
            response.Code = HttpStatusCode.Unauthorized;
            response.Message = "two_factor_code_required";
            response.RequiresTwoFactor = true;
            return response;
        }

        if (twoFactorResult == TwoFactorLoginVerificationResult.InvalidCode)
        {
            response.Code = HttpStatusCode.Unauthorized;
            response.Message = "invalid_two_factor_code";
            response.RequiresTwoFactor = true;
            return response;
        }
        
        var claims = GetUserAllClaimsAsync(
            userAccount, twoFactorResult == TwoFactorLoginVerificationResult.Valid);
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

        var isTwoFactorAuthenticated = accountResponse.Data.IsTwoFactorEnabled && principal.HasClaim(
            TokenClaimService.TwoFactorAuthenticationClaim,
            TokenClaimService.TwoFactorAuthenticationValue);
        var claims = GetUserAllClaimsAsync(accountResponse.Data, isTwoFactorAuthenticated);
        response.Data = new JwtToken
        {
            AccessToken = _tokenClaimService.GenerateAccessToken(claims),
            RefreshToken = _tokenClaimService.GenerateRefreshToken(claims)
        };

        return response;
    }

    public async Task<VerifyTwoFactorResponse> VerifyTwoFactorAsync(
        VerifyTwoFactorCommand command,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue || _currentUser.Id == Guid.Empty)
        {
            return new VerifyTwoFactorResponse
            {
                Code = HttpStatusCode.Unauthorized,
                Message = "account_not_authenticated"
            };
        }

        var accountResponse = await _accountService.GetUserAccountAsync(
            new GetUserAccountRequest { UserId = _currentUser.Id },
            cancellationToken).ConfigureAwait(false);
        if (accountResponse.Data is not { } account)
        {
            return new VerifyTwoFactorResponse
            {
                Code = HttpStatusCode.NotFound,
                Message = "account_not_found"
            };
        }

        if (!account.IsTwoFactorEnabled)
        {
            return new VerifyTwoFactorResponse
            {
                Code = HttpStatusCode.Conflict,
                Message = "two_factor_not_enabled"
            };
        }

        var verificationResult = await _accountTwoFactorService.VerifyLoginAsync(
            account.Id, command.Code, cancellationToken).ConfigureAwait(false);
        if (verificationResult != TwoFactorLoginVerificationResult.Valid)
        {
            return new VerifyTwoFactorResponse
            {
                Code = HttpStatusCode.Unauthorized,
                Message = "invalid_two_factor_code"
            };
        }

        var claims = GetUserAllClaimsAsync(account, true);
        return new VerifyTwoFactorResponse
        {
            Data = new JwtToken
            {
                AccessToken = _tokenClaimService.GenerateAccessToken(claims),
                RefreshToken = _tokenClaimService.GenerateRefreshToken(claims)
            }
        };
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
