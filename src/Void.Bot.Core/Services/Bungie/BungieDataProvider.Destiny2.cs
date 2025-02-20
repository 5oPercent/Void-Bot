using Void.Bot.Core.Extension;
using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Enum.Bungie;
using Void.Bot.Message.Enum.Bungie.Destiny;
using Void.Bot.Message.Request.Bungie.Domain;

namespace Void.Bot.Core.Services.Bungie;

public partial interface IBungieDataProvider : IScope
{
    Task<BungieApiLinkedProfilesResponse?> GetLinkedProfilesAsync(
        BungieMembershipType membershipType, long membershipId, CancellationToken cancellationToken, string accessToken = "");
    
    Task<BungieApiGetProfileFullResponse?> GetProfileAsync(
        BungieMembershipType membershipType, long membershipId, List<DestinyComponentTypeEnum> components,
        CancellationToken cancellationToken, string accessToken = "");
    
    Task<BungieApiSearchDestinyPlayerByBungieNameResponse?> SearchDestinyPlayerByBungieNameAsync(
        BungieMembershipType membershipType, string displayName, int displayNameCode,
        CancellationToken cancellationToken, string accessToken = "");
}

public partial class BungieDataProvider : IBungieDataProvider
{
    public async Task<BungieApiLinkedProfilesResponse?> GetLinkedProfilesAsync(
        BungieMembershipType membershipType, long membershipId, CancellationToken cancellationToken, string accessToken = "")
    {
        var url = BungieConstants.Destiny2LinkedProfiles
            .Replace("{membershipType}", ((int)membershipType).ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("{membershipId}", membershipId.ToString(), StringComparison.OrdinalIgnoreCase);

        var result = await GetBungieHttpClient(accessToken).GetAsync<BungieApiLinkedProfilesResponse>(url, cancellationToken).ConfigureAwait(false);
        
        return result.Data;
    }

    public async Task<BungieApiGetProfileFullResponse?> GetProfileAsync(
        BungieMembershipType membershipType, long membershipId, List<DestinyComponentTypeEnum> components,
        CancellationToken cancellationToken, string accessToken = "")
    {
        var url = BungieConstants.Destiny2ProfileUrl
            .Replace("{membershipType}", ((int)membershipType).ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("{membershipId}", membershipId.ToString(), StringComparison.OrdinalIgnoreCase);

        url += $"?components={string.Join(",", components.Select(x => (int)x))}";
        
        var result = await GetBungieHttpClient(accessToken).GetAsync<BungieApiGetProfileFullResponse>(url, cancellationToken).ConfigureAwait(false);

        return result.Data;
    }

    public async Task<BungieApiSearchDestinyPlayerByBungieNameResponse?> SearchDestinyPlayerByBungieNameAsync(
        BungieMembershipType membershipType, string displayName, int displayNameCode,
        CancellationToken cancellationToken, string accessToken = "")
    {
        var url = BungieConstants.Destiny2SearchPlayerProfile
            .Replace("{membershipType}", ((int)membershipType).ToString(), StringComparison.OrdinalIgnoreCase);

        var request = new BungieApiSearchDestinyPlayerByBungieNameRequest
        {
            DisplayName = displayName, DisplayNameCode = displayNameCode
        };
        
        var result = await GetBungieHttpClient(accessToken)
            .PostAsync<BungieApiSearchDestinyPlayerByBungieNameResponse>(url, request, cancellationToken).ConfigureAwait(false);
        
        return result.Data;
    }
}