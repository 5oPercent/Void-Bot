using System.Net;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Commands.Bungie;
using Void.Bot.Message.Dto.Authorization;

namespace Void.Bot.Core.Services.Bungie;

public partial interface IBungieService : IScope
{
    string GenerateAuthenticateUrl(AuthorizationDto dto);

    Task<GenerateBungieTokenResponse> GenerateTokenAsync(GenerateBungieTokenCommand command, CancellationToken cancellationToken);

    Task<RefreshBungieTokenResponse> RefreshTokenAsync(RefreshBungieTokenCommand command, CancellationToken cancellationToken);
}

public partial class BungieService : IBungieService
{
    private readonly IBungieDataProvider _dataProvider;
    private readonly IAccountService _accountService;
    private readonly IAccountDataProvider _accountDataProvider;
    
    public BungieService(IBungieDataProvider dataProvider, IAccountService accountService, IAccountDataProvider accountDataProvider)
    {
        _dataProvider = dataProvider;
        _accountService = accountService;
        _accountDataProvider = accountDataProvider;
    }
    
    public string GenerateAuthenticateUrl(AuthorizationDto dto)
    {
        var authUrl = _dataProvider.GenerateAuthenticateUrl(dto.Platform, dto.AccountId);

        return authUrl;
    }

    public async Task<GenerateBungieTokenResponse> GenerateTokenAsync(GenerateBungieTokenCommand command, CancellationToken cancellationToken)
    {
        var response = new GenerateBungieTokenResponse();

        var result = await _dataProvider.GenerateTokenAsync(command.OAuthCode, cancellationToken).ConfigureAwait(false);

        if (result == null)
        {
            response.Code = HttpStatusCode.InternalServerError;
            response.Message = "Failed to generate token.";
            return response;
        }
        
        if (!string.IsNullOrEmpty(result.Error))
        {
            response.Code = HttpStatusCode.InternalServerError;
            response.Message = $"{result.Error}: {result.ErrorDescription}";
            return response;
        }

        response.Code = HttpStatusCode.OK;
        response.Data = result;
        return response;
    }
    
    public async Task<RefreshBungieTokenResponse> RefreshTokenAsync(RefreshBungieTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await _dataProvider.RefreshTokenAsync(command.Token, cancellationToken).ConfigureAwait(false);

        return new RefreshBungieTokenResponse
        {
            Data = result
        };
    }
}