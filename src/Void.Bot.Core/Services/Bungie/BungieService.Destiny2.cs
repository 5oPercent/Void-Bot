using System.Net;
using Void.Bot.Core.Domain.Destiny2;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Commands.Bungie.Destiny2;
using Void.Bot.Message.Constants.Bungie.Destiny2;
using Void.Bot.Message.Dto.Bungie.Destiny2;
using Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;
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
        GetDestiny2SeasonCommand command, CancellationToken cancellationToken);
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
        GetDestiny2SeasonCommand command, CancellationToken cancellationToken)
    {
        const int activityPageSize = 250;
        var response = new GetDestiny2SeasonResponse();

        if (string.IsNullOrEmpty(_playerData.PrimaryMembershipId))
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = "Player not bound";
            return response;
        }

        if (command.MembershipType != _playerData.PrimaryMembershipType)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = $"Not found Destiny 2 profile by {command.MembershipType} platform. Player's primary platform is {_playerData.PrimaryMembershipType}.";
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

        var characterStats = new List<CharacterPlaytimeDto>();
        var characters = accountStatsResult.Response.Characters ?? [];

        foreach (var charData in characters)
        {
            if (string.IsNullOrEmpty(charData?.CharacterId))
                continue;

            var characterId = charData.CharacterId;

            var classType = characterClassMap.TryGetValue(characterId, out var ct) ? ct : Destiny2ClassEnum.Unknown;
            var playtimeSeconds = (long)(charData.Merged?.AllTime?.SecondsPlayed?.Basic?.Value ?? 0);

            characterStats.Add(new CharacterPlaytimeDto
            {
                CharacterId = characterId,
                Class = classType,
                PlaytimeSeconds = playtimeSeconds
            });
        }

        var totalPlaytimeSeconds = characterStats.Sum(c => c.PlaytimeSeconds);

        var manifestLanguageCode = LanguageExtension.MapI18nCodeToEnum(_playerData.Language).ToI18nCode();
        var seasonJson = await _cacheManager.Get<string>(
            $"{Destiny2ManifestConstants.SeasonCacheKey}:{manifestLanguageCode}", cancellationToken).ConfigureAwait(false);
        var seasonManifest = seasonJson?.DeserializeJson<Destiny2ManifestSeasonDto>();

        if (seasonManifest == null)
        {
            response.Code = HttpStatusCode.ServiceUnavailable;
            response.Message = "Destiny 2 season manifest is unavailable";
            return response;
        }

        var utcNow = DateTime.UtcNow;
        foreach (var season in seasonManifest.Values)
        {
            if (!Destiny2Constants.LegacySeasonDateRanges.TryGetValue(season.SeasonNumber, out var dateRange))
                continue;

            season.StartDate = dateRange.StartDate;
            season.EndDate = dateRange.EndDate;
        }

        var seasons = seasonManifest.Values
            .Where(x => x.StartDate < x.EndDate && x.StartDate <= utcNow)
            .OrderBy(x => x.StartDate)
            .ToList();

        var storedHistories = await _dataProvider.GetPlayerSeasonHistoryByDatabaseAsync(
            _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId,
            cancellationToken).ConfigureAwait(false);
        var storedHistoryMap = storedHistories.ToDictionary(x => x.SeasonHash);

        var completedSeasons = seasons.Where(x => x.EndDate <= utcNow).ToList();
        var missingCompletedSeasons = completedSeasons
            .Where(x => !storedHistoryMap.ContainsKey(x.Hash))
            .ToList();
        var currentSeason = seasons.LastOrDefault(x => x.StartDate <= utcNow && utcNow < x.EndDate);

        var seasonsToCalculate = missingCompletedSeasons.ToList();
        if (currentSeason != null)
            seasonsToCalculate.Add(currentSeason);

        var calculatedSeconds = seasonsToCalculate.ToDictionary(
            x => x.Hash,
            _ => new Dictionary<Destiny2ClassEnum, long>
            {
                [Destiny2ClassEnum.Hunter] = 0,
                [Destiny2ClassEnum.Titan] = 0,
                [Destiny2ClassEnum.Warlock] = 0
            });

        if (seasonsToCalculate.Count > 0)
        {
            var earliestRequiredDate = seasonsToCalculate.Min(x => x.StartDate);

            foreach (var character in characterClassMap.Where(x => x.Value != Destiny2ClassEnum.Unknown))
            {
                for (var page = 0; ; page++)
                {
                    var activityResult = await _dataProvider.GetCharacterActivityHistoryAsync(
                        _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId,
                        character.Key, 0, activityPageSize, page,
                        cancellationToken).ConfigureAwait(false);

                    if (activityResult is not { ErrorCode: 1 })
                    {
                        response.Code = HttpStatusCode.BadRequest;
                        response.Message = activityResult?.Message ?? "Failed to fetch activity history from Bungie API";
                        return response;
                    }

                    var activities = activityResult.Response?.Activities ?? [];
                    foreach (var activity in activities)
                    {
                        var season = seasonsToCalculate.FirstOrDefault(x =>
                            x.StartDate <= activity.Period.UtcDateTime && activity.Period.UtcDateTime < x.EndDate);
                        if (season == null ||
                            !activity.Values.TryGetValue("activityDurationSeconds", out var durationStat))
                            continue;

                        var durationSeconds = Math.Max(0, (long)(durationStat.Basic?.Value ?? 0));
                        calculatedSeconds[season.Hash][character.Value] += durationSeconds;
                    }

                    if (activities.Count < activityPageSize ||
                        activities.Min(x => x.Period.UtcDateTime) < earliestRequiredDate)
                        break;
                }
            }
        }

        var newHistories = missingCompletedSeasons
            .Select(season => new Destiny2SeasonHistory
            {
                Id = Guid.NewGuid(),
                MembershipType = _playerData.PrimaryMembershipType,
                MembershipId = _playerData.PrimaryMembershipId,
                SeasonHash = season.Hash,
                HunterSeconds = calculatedSeconds[season.Hash][Destiny2ClassEnum.Hunter],
                TitanSeconds = calculatedSeconds[season.Hash][Destiny2ClassEnum.Titan],
                WarlockSeconds = calculatedSeconds[season.Hash][Destiny2ClassEnum.Warlock]
            })
            .Where(x => x.HunterSeconds + x.TitanSeconds + x.WarlockSeconds > 0)
            .ToList();

        await _dataProvider.SavePlayerSeasonHistoryToDatabaseAsync(newHistories, cancellationToken).ConfigureAwait(false);
        foreach (var history in newHistories)
            storedHistoryMap[history.SeasonHash] = history;

        var currentSeasonHash = currentSeason?.Hash;
        var seasonStats = seasons
            .OrderByDescending(x => x.StartDate)
            .Select(season =>
            {
                var classSeconds = calculatedSeconds.GetValueOrDefault(season.Hash);
                storedHistoryMap.TryGetValue(season.Hash, out var storedHistory);

                return new Destiny2SeasonPlaytimeDto
                {
                    SeasonHash = season.Hash,
                    SeasonNumber = season.SeasonNumber,
                    Name = season.DisplayProperties.Name,
                    StartDate = season.StartDate,
                    EndDate = season.EndDate,
                    Classes =
                    [
                        new ClassPlaytimeDto
                        {
                            Class = Destiny2ClassEnum.Hunter,
                            PlaytimeSeconds = classSeconds?.GetValueOrDefault(Destiny2ClassEnum.Hunter)
                                ?? storedHistory?.HunterSeconds ?? 0
                        },
                        new ClassPlaytimeDto
                        {
                            Class = Destiny2ClassEnum.Titan,
                            PlaytimeSeconds = classSeconds?.GetValueOrDefault(Destiny2ClassEnum.Titan)
                                ?? storedHistory?.TitanSeconds ?? 0
                        },
                        new ClassPlaytimeDto
                        {
                            Class = Destiny2ClassEnum.Warlock,
                            PlaytimeSeconds = classSeconds?.GetValueOrDefault(Destiny2ClassEnum.Warlock)
                                ?? storedHistory?.WarlockSeconds ?? 0
                        }
                    ]
                };
            })
            .Where(x => x.TotalPlaytimeSeconds > 0 || x.SeasonHash == currentSeasonHash)
            .ToList();

        response.Data = new Destiny2SeasonStatsDto
        {
            TotalPlaytimeSeconds = totalPlaytimeSeconds,
            Characters = characterStats,
            Seasons = seasonStats
        };
        response.Code = HttpStatusCode.OK;
        response.Message = "Success";

        await _cacheManager.Set(cacheKey, response.Data, TimeSpan.FromMinutes(5), cancellationToken).ConfigureAwait(false);

        return response;
    }
}
