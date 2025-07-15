using Void.Bot.Core.Extension;
using Void.Bot.Message.Constants.Bungie.Destiny2;
using Void.Bot.Message.Dto.Bungie.Destiny2;
using Void.Bot.Message.Enum.Bungie;
using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Core.Services.Bungie;

public partial interface IBungieDataProvider : IScope
{
    Task<Destiny2ManifestResponse?> GetDestiny2ManifestAsync(CancellationToken cancellationToken);
    
    Task<Destiny2LinkedProfilesResponse?> GetDestiny2LinkedProfilesAsync(
        BungieMembershipTypeEnum membershipType, long membershipId, CancellationToken cancellationToken, string accessToken = "");
    
    Task<Destiny2ProfileResponse?> GetDestiny2ProfileAsync(
        BungieMembershipTypeEnum membershipType, string membershipId, List<Destiny2ComponentTypeEnum> components,
        CancellationToken cancellationToken, string accessToken = "");
    
    Task<Destiny2PlayerByBungieNameResponse?> GetDestiny2PlayerByBungieNameAsync(
        BungieMembershipTypeEnum membershipType, string displayName, string displayNameCode,
        CancellationToken cancellationToken, string accessToken = "");
}

public partial class BungieDataProvider : IBungieDataProvider
{
    public async Task<Destiny2ManifestResponse?> GetDestiny2ManifestAsync(CancellationToken cancellationToken)
    {
        var url = Destiny2Constants.ManifestUrl;
        
        var result = await GetBungieHttpClient().GetAsync<Destiny2ManifestResponse>(url, cancellationToken).ConfigureAwait(false);
        
        return result.Data;
    }

    public async Task<Destiny2LinkedProfilesResponse?> GetDestiny2LinkedProfilesAsync(
        BungieMembershipTypeEnum membershipType, long membershipId, CancellationToken cancellationToken, string accessToken = "")
    {
        var url = Destiny2Constants.LinkedProfilesUrl
            .Replace("{membershipType}", ((int)membershipType).ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("{membershipId}", membershipId.ToString(), StringComparison.OrdinalIgnoreCase);

        var result = await GetBungieHttpClient(accessToken).GetAsync<Destiny2LinkedProfilesResponse>(url, cancellationToken).ConfigureAwait(false);
        
        return result.Data;
    }

    public async Task<Destiny2ProfileResponse?> GetDestiny2ProfileAsync(
        BungieMembershipTypeEnum membershipType, string membershipId, List<Destiny2ComponentTypeEnum> components,
        CancellationToken cancellationToken, string accessToken = "")
    {
        var url = Destiny2Constants.ProfileUrl
            .Replace("{membershipType}", ((int)membershipType).ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("{membershipId}", membershipId, StringComparison.OrdinalIgnoreCase);

        url += $"?components={string.Join(",", components.Select(x => (int)x))}";
        
        var result = await GetBungieHttpClient(accessToken).GetAsync<Destiny2ProfileResponse>(url, cancellationToken).ConfigureAwait(false);

        return result.Data;
    }

    public async Task<Destiny2PlayerByBungieNameResponse?> GetDestiny2PlayerByBungieNameAsync(
        BungieMembershipTypeEnum membershipType, string displayName, string displayNameCode,
        CancellationToken cancellationToken, string accessToken = "")
    {
        var url = Destiny2Constants.SearchPlayerProfileUrl
            .Replace("{membershipType}", ((int)membershipType).ToString(), StringComparison.OrdinalIgnoreCase);
        
        var convertCodeStatus = int.TryParse(displayNameCode, out var displayNameCodeValue);
    
        var request = new Destiny2PlayerByBungieNameRequest
        {
            DisplayName = displayName,
            DisplayNameCode = convertCodeStatus ? displayNameCodeValue : 0
        };
        
        var result = await GetBungieHttpClient(accessToken)
            .PostAsync<Destiny2PlayerByBungieNameResponse>(url, request, cancellationToken).ConfigureAwait(false);
        
        return result.Data;
    }
}