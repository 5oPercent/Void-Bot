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
    
    public BungieService(IBungieDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }
    
    public string GenerateAuthenticateUrl(AuthorizationDto dto)
    {
        var authUrl = _dataProvider.GenerateAuthenticateUrl(dto.Platform, dto.AccountId);

        return authUrl;
    }

    public async Task<GenerateBungieTokenResponse> GenerateTokenAsync(GenerateBungieTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await _dataProvider.GenerateTokenAsync(command.OAuthCode, cancellationToken).ConfigureAwait(false);

        return new GenerateBungieTokenResponse
        {
            Data = result
        };
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