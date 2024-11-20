using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Enum.Bungie.Destiny;

namespace Void.Bot.Core.Services.Bungie;

public partial interface IBungieDataProvider : IScope
{
    Task<string?> GetProfileAsync(
        int membershipType, long destinyMembershipId, List<DestinyComponentTypeEnum> components, CancellationToken cancellationToken);
}

public partial class BungieDataProvider : IBungieDataProvider
{
    public async Task<string?> GetProfileAsync(
        int membershipType, long destinyMembershipId, List<DestinyComponentTypeEnum> components, CancellationToken cancellationToken)
    {
        var url = BungieConstants.Destiny2ProfileUrl
            .Replace("{membershipType}", membershipType.ToString())
            .Replace("{destinyMembershipId}", destinyMembershipId.ToString());
        
        var result = await _httpClientFactory.GetAsync<string>(url, cancellationToken, headers: _headers);

        return result.Data;
    }
}