using System.Net;
using Void.Bot.Message.Dto.Bungie.Destiny2;
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
    
    Task<GetDestiny2SeasonResponse> GetDestiny2SeasonAsync(
        GetDestiny2SeasonRequest request, CancellationToken cancellationToken);
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
            Code = result != null ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
            Data = result
        };
    }
    
    public async Task<GetDestiny2ProfileListByBungieNameResponse> GetDestiny2ProfileListByBungieNameAsync(
        GetDestiny2ProfileListByBungieNameRequest request, CancellationToken cancellationToken)
    {
        var response = new GetDestiny2ProfileListByBungieNameResponse();
        
        var result = await _dataProvider.GetDestiny2PlayerByBungieNameAsync(
            request.MembershipType, request.DisplayName, request.DisplayNameCode, cancellationToken).ConfigureAwait(false);

        if (result is not { ErrorCode: 1 } or { Response.Count: 0 })
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = result?.Message ?? $"Not found Destiny 2 profiles by {request.MembershipType} platform";
            return response;
        }
        
        response.Data = result;

        return response;
    }
    
    public async Task<GetDestiny2BaseProfileResponse> GetDestiny2BaseProfileAsync(
        GetDestiny2BaseProfileRequest request, CancellationToken cancellationToken)
    {
        var response = new GetDestiny2BaseProfileResponse();
        
        if (string.IsNullOrEmpty(_playerData.PrimaryMembershipId))
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = $"Not found Destiny 2 profile by {request.MembershipType} platform.";
            return response;
        }
        
        response.Data = new Destiny2BaseProfileDto
        {
            DisplayName = _playerData.UniqueName,
            OnCrossSave = _playerData.OnCrossSave,
            IsSinglePlatform = _playerData.IsSinglePlatform,
            MembershipType = _playerData.PrimaryMembershipType,
            MembershipId = _playerData.PrimaryMembershipId
        };
        
        return response;
    }

    public async Task<GetDestiny2CareerResponse> GetDestiny2CareerAsync(
        GetDestiny2CareerRequest request, CancellationToken cancellationToken)
    {
        var response = new GetDestiny2CareerResponse();

        if (request.MembershipType != _playerData.PrimaryMembershipType)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = $"Not found Destiny 2 profile by {request.MembershipType} platform.";
            return response;
        }
        
        var result = await _dataProvider.GetDestiny2ProfileAsync(
            _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId,
            [Destiny2ComponentTypeEnum.Profiles, Destiny2ComponentTypeEnum.SocialCommendations],
            cancellationToken).ConfigureAwait(false);

        if (result is not { ErrorCode: 1 })
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = $"Not found Destiny 2 career by {request.MembershipType} platform.";
            return response;
        }

        response.Data = result;

        return response;
    }

    public async Task<GetDestiny2SeasonResponse> GetDestiny2SeasonAsync(
        GetDestiny2SeasonRequest request, CancellationToken cancellationToken)
    {
        var response = new GetDestiny2SeasonResponse();

        if (string.IsNullOrEmpty(_playerData.PrimaryMembershipId))
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = "Player not bound";
            return response;
        }

        if (request.MembershipType != _playerData.PrimaryMembershipType)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = $"Not found Destiny 2 profile by {request.MembershipType} platform. Player's primary platform is {_playerData.PrimaryMembershipType}.";
            return response;
        }

        var cacheKey = $"{_playerData.PrimaryMembershipType}:{_playerData.PrimaryMembershipId}:season";
        var cached = await _cacheManager.Get<Destiny2SeasonStatsDto>(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cached != null)
        {
            response.Data = cached;
            response.Code = HttpStatusCode.OK;
            response.Message = "Cached";
            return response;
        }

        var charactersResult = await _dataProvider.GetDestiny2ProfileAsync(
            _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId,
            [Destiny2ComponentTypeEnum.Characters], cancellationToken).ConfigureAwait(false);

        if (charactersResult is not { ErrorCode: 1 })
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = charactersResult?.Message ?? "Failed to fetch profile data from Bungie API";
            return response;
        }

        var characterClassMap = charactersResult.Response.Characters.Data
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ClassType);

        var accountStatsResult = await _dataProvider.GetAccountStatsAsync(
            _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId, cancellationToken).ConfigureAwait(false);

        if (accountStatsResult is not { ErrorCode: 1 })
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = accountStatsResult?.Message ?? "Failed to fetch playtime data from Bungie API";
            return response;
        }

        // 10. Handle no data case (D-03: hasData: false case)
        if (accountStatsResult.Response.Characters == null)
        {
            response.Data = new Destiny2SeasonStatsDto();
            response.Code = HttpStatusCode.OK;
            response.Message = "No playtime data available";
            return response;
        }

        // 11. Build per-character breakdown (SEAS-03 requirement)
        var characterStats = new List<CharacterPlaytimeDto>();
        var characters = accountStatsResult.Response.Characters ?? [];

        foreach (var charData in characters)
        {
            if (string.IsNullOrEmpty(charData?.CharacterId))
                continue;

            var characterId = charData.CharacterId;

            // Get classType from the profile mapping
            var classType = characterClassMap.TryGetValue(characterId, out var ct) ? ct : Destiny2ClassEnum.Unknown;

            // Get playtime from merged stats
            var playtimeSeconds = (long)(charData.Merged?.AllTime?.SecondsPlayed?.Basic?.Value ?? 0);

            characterStats.Add(new CharacterPlaytimeDto
            {
                CharacterId = characterId,
                Class = classType,
                PlaytimeSeconds = playtimeSeconds
            });
        }

        // 12. Calculate total playtime
        var totalPlaytimeSeconds = characterStats.Sum(c => c.PlaytimeSeconds);

        response.Data = new Destiny2SeasonStatsDto
        {
            TotalPlaytimeSeconds = totalPlaytimeSeconds,
            Characters = characterStats
        };
        response.Code = HttpStatusCode.OK;
        response.Message = "Success";

        // 14. Cache result with 5-minute TTL (D-04)
        await _cacheManager.Set(cacheKey, response.Data, TimeSpan.FromMinutes(5), cancellationToken).ConfigureAwait(false);

        return response;
    }
}