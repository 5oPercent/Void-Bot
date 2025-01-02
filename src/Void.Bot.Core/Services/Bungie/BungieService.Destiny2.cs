using System.Net;
using Void.Bot.Message.Request.Bungie;

namespace Void.Bot.Core.Services.Bungie;

public partial interface IBungieService : IScope
{
    public Task<GetDestinyLinkedProfileResponse> GetDestinyLinkedProfileAsync(
        GetDestinyLinkedProfileRequest request, CancellationToken cancellationToken);
}

public partial class BungieService : IBungieService
{
    public async Task<GetDestinyLinkedProfileResponse> GetDestinyLinkedProfileAsync(
        GetDestinyLinkedProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _dataProvider.GetLinkedProfilesAsync(
            request.MembershipType, request.MembershipId, cancellationToken).ConfigureAwait(false);

        return new GetDestinyLinkedProfileResponse
        {
            Code = result != null ? HttpStatusCode.OK : HttpStatusCode.InternalServerError,
            Data = result
        };
    }
}