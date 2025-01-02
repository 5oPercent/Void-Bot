using Void.Bot.Core.Extension;
using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Enum.Bungie;
using Void.Bot.Message.Enum.Bungie.Destiny;
using Void.Bot.Message.Request.Bungie;
using Void.Bot.Message.Request.Bungie.Domain;

namespace Void.Bot.Core.Services.Bungie;

public partial interface IBungieDataProvider : IScope
{
    Task<BungieApiLinkedProfilesResponse?> GetLinkedProfilesAsync(
        BungieMembershipType membershipType, long membershipId, CancellationToken cancellationToken);
    
    Task<string?> GetProfileAsync(
        BungieMembershipType membershipType, long membershipId,
        List<DestinyComponentTypeEnum> components, CancellationToken cancellationToken);
}

public partial class BungieDataProvider : IBungieDataProvider
{
    public async Task<BungieApiLinkedProfilesResponse?> GetLinkedProfilesAsync(
        BungieMembershipType membershipType, long membershipId, CancellationToken cancellationToken)
    {
        var url = BungieConstants.Destiny2LinkedProfiles
            .Replace("{membershipType}", ((int)membershipType).ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("{membershipId}", membershipId.ToString(), StringComparison.OrdinalIgnoreCase);
        
        var result = await GetBungieHttpClient().GetAsync<BungieApiLinkedProfilesResponse>(url, cancellationToken).ConfigureAwait(false);
        
        return result.Data;
    }

    public async Task<string?> GetProfileAsync(
        BungieMembershipType membershipType, long membershipId,
        List<DestinyComponentTypeEnum> components, CancellationToken cancellationToken)
    {
        var url = BungieConstants.Destiny2ProfileUrl
            .Replace("{membershipType}", ((int)membershipType).ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("{membershipId}", membershipId.ToString(), StringComparison.OrdinalIgnoreCase);
        
        var result = await GetBungieHttpClient().GetAsync<string>(url, cancellationToken).ConfigureAwait(false);

        return result.Data;
    }
}