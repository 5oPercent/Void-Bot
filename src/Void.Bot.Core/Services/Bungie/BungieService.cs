using System.Net;
using Void.Bot.Message.Commands.Bungie;
using Void.Bot.Message.Dto.Authorization;
using Void.Bot.Message.Request.Bungie;

namespace Void.Bot.Core.Services.Bungie;

public partial interface IBungieService : IScope
{
    string GenerateAuthenticateUrl(AuthorizationDto dto);

    Task<GenerateBungieTokenResponse> GenerateTokenAsync(
        GenerateBungieTokenCommand command, CancellationToken cancellationToken);

    Task<RefreshBungieTokenResponse> RefreshTokenAsync(
        RefreshBungieTokenCommand command, CancellationToken cancellationToken);
    
    Task<GetBungieNetUserResponse> GetBungieNetUserAsync(
        GetBungieNetUserRequest request, CancellationToken cancellationToken);
    
    Task<GetBungieMembershipsByIdResponse> GetMembershipsByIdAsync(
        GetBungieMembershipsByIdRequest request, CancellationToken cancellationToken);
}

public partial class BungieService : IBungieService
{
    private readonly IBungieDataProvider _dataProvider;
    private readonly Destiny2PlayerData _playerData;
    
    public BungieService(
        IBungieDataProvider dataProvider,
        Destiny2PlayerData playerData)
    {
        _dataProvider = dataProvider;
        _playerData = playerData;
    }
    
    public string GenerateAuthenticateUrl(AuthorizationDto dto)
    {
        var authUrl = _dataProvider.GenerateAuthenticateUrl(dto.Platform, dto.AccountId);

        return authUrl;
    }

    public async Task<GenerateBungieTokenResponse> GenerateTokenAsync(
        GenerateBungieTokenCommand command, CancellationToken cancellationToken)
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
    
    public async Task<RefreshBungieTokenResponse> RefreshTokenAsync(
        RefreshBungieTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await _dataProvider.RefreshTokenAsync(command.Token, cancellationToken).ConfigureAwait(false);

        return new RefreshBungieTokenResponse
        {
            Data = result
        };
    }

    public async Task<GetBungieNetUserResponse> GetBungieNetUserAsync(
        GetBungieNetUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _dataProvider.GetNetUserAsync(
            request.Id, cancellationToken).ConfigureAwait(false);
        
        return new GetBungieNetUserResponse
        {
            Data = result
        };
    }

    public async Task<GetBungieMembershipsByIdResponse> GetMembershipsByIdAsync(
        GetBungieMembershipsByIdRequest request, CancellationToken cancellationToken)
    {
        var result = await _dataProvider.GetMembershipsByIdAsync(
            request.MembershipType, request.MembershipId, cancellationToken).ConfigureAwait(false);

        return new GetBungieMembershipsByIdResponse
        {
            Data = result
        };
    }
}