using Serilog;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Core.Services.Identity;

public interface IIdentityService : IScope
{
    Task<LoginSteamResponse> LoginFromSteamAsync(LoginSteamRequest request, CancellationToken cancellationToken);
    
    Task<LoginBungieResponse> LoginFromBungieAsync(LoginBungieRequest request, CancellationToken cancellationToken);
}

public class IdentityService : IIdentityService
{
    private readonly ILogger _logger;

    public IdentityService(ILogger logger)
    {
        _logger = logger;
    }
    
    public async Task<LoginSteamResponse> LoginFromSteamAsync(LoginSteamRequest request, CancellationToken cancellationToken)
    {
        var claimedId = request.OpenID.Claimed_id;
        if (claimedId == null)
        {
            throw new Exception("绑定失败, Steam ID 未找到");
        }
        
        var lastSlashIndex = claimedId.LastIndexOf('/');
        var steamId = claimedId[(lastSlashIndex + 1)..];
        
        _logger.Information($"Platform: {request.Authorization.Platform}\n" +
                            $"Account ID: {request.Authorization.AccountId}\n" +
                            $"Steam ID: {steamId}");

        return new LoginSteamResponse
        {
            Data = $"Steam ID: {steamId}"
        };
    }

    public async Task<LoginBungieResponse> LoginFromBungieAsync(LoginBungieRequest request, CancellationToken cancellationToken)
    {
        var state = request.State.Split('|');
        var platform = state[0];
        var accountId = state[1];
        
        if (string.IsNullOrEmpty(platform) || string.IsNullOrEmpty(accountId))
        {
            throw new Exception("绑定失败, Platform 或 Account ID 为空");
        }
        
        _logger.Information($"Platform: {platform}\n" +
                            $"Account ID: {accountId}\n" +
                            $"Bungie OAuth Code: {request.Code}");

        return new LoginBungieResponse
        {
            Data = $"Bungie OAuth Code: {request.Code}"
        };
    }
}