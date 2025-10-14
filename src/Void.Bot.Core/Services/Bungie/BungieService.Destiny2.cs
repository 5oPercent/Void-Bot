using System.Net;
using Void.Bot.Message.Enum.Bungie.Destiny2;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Services.Bungie;

public partial interface IBungieService : IScope
{
    public Task<GetDestiny2LinkedProfileResponse> GetDestiny2LinkedProfileAsync(
        GetDestiny2LinkedProfileRequest request, CancellationToken cancellationToken);
    
    public Task<GetDestiny2ProfileListByBungieNameResponse> GetDestiny2ProfileListByBungieNameAsync(
        GetDestiny2ProfileListByBungieNameRequest request, CancellationToken cancellationToken);

    public Task<GetDestiny2BaseProfileResponse> GetDestiny2BaseProfileAsync(
        GetDestiny2BaseProfileRequest request, CancellationToken cancellationToken);
    
    public Task<GetDestiny2CareerResponse> GetDestiny2CareerAsync(
        GetDestiny2CareerRequest request, CancellationToken cancellationToken);
}

public partial class BungieService : IBungieService
{
    public async Task<GetDestiny2LinkedProfileResponse> GetDestiny2LinkedProfileAsync(
        GetDestiny2LinkedProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _dataProvider.GetDestiny2LinkedProfilesAsync(
            request.MembershipType, request.MembershipId, cancellationToken).ConfigureAwait(false);

        return new GetDestiny2LinkedProfileResponse
        {
            Code = result != null ? HttpStatusCode.OK : HttpStatusCode.InternalServerError,
            Data = result
        };
    }
    
    public async Task<GetDestiny2ProfileListByBungieNameResponse> GetDestiny2ProfileListByBungieNameAsync(
        GetDestiny2ProfileListByBungieNameRequest request, CancellationToken cancellationToken)
    {
        var result = await _dataProvider.GetDestiny2PlayerByBungieNameAsync(
            request.MembershipType, request.DisplayName, request.DisplayNameCode, cancellationToken).ConfigureAwait(false);

        return new GetDestiny2ProfileListByBungieNameResponse
        {
            Code = result != null ? HttpStatusCode.OK : HttpStatusCode.InternalServerError,
            Data = result
        };
    }
    
    public async Task<GetDestiny2BaseProfileResponse> GetDestiny2BaseProfileAsync(
        GetDestiny2BaseProfileRequest request, CancellationToken cancellationToken)
    {
        var response = new GetDestiny2BaseProfileResponse
        {
            Data = new Destiny2BaseProfileDto
            {
                DisplayName = _playerData.UniqueName,
                OnCrossSave = _playerData.OnCrossSave,
                IsSinglePlatform = _playerData.IsSinglePlatform,
                MembershipType = _playerData.PrimaryMembershipType,
                MembershipId = _playerData.PrimaryMembershipId
            }
        };
        
        return response;
    }

    public async Task<GetDestiny2CareerResponse> GetDestiny2CareerAsync(
        GetDestiny2CareerRequest request, CancellationToken cancellationToken)
    {
        var result = await _dataProvider.GetDestiny2ProfileAsync(
            _playerData.PrimaryMembershipType,  _playerData.PrimaryMembershipId,
            [Destiny2ComponentTypeEnum.Profiles, Destiny2ComponentTypeEnum.SocialCommendations],
            cancellationToken).ConfigureAwait(false);

        return new GetDestiny2CareerResponse
        {
            Data = result
        };
    }
}