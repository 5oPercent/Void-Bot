using System.Collections.Concurrent;
using System.Net;
using Serilog;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Domain.Destiny2;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Services.Bungie.Destiny2.RaidDungeon;
using Void.Bot.Message.Commands.Bungie.Destiny2;
using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Constants.Bungie.Destiny2;
using Void.Bot.Message.Dto.Bungie.Destiny2;
using Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;
using Void.Bot.Message.Enum.Bungie;
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

    public Task<GetDestiny2RaidDungeonStatsResponse> GetDestiny2RaidDungeonStatsAsync(
        GetDestiny2RaidDungeonStatsRequest request, CancellationToken cancellationToken);
    
    Task<GetDestiny2SeasonResponse> GetDestiny2SeasonAsync(
        GetDestiny2SeasonCommand command, CancellationToken cancellationToken);
}

public partial class BungieService : IBungieService
{
    private static readonly ConcurrentDictionary<string, byte> ActiveSeasonCalculations = new();

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

    public async Task<GetDestiny2RaidDungeonStatsResponse> GetDestiny2RaidDungeonStatsAsync(
        GetDestiny2RaidDungeonStatsRequest request, CancellationToken cancellationToken)
    {
        const int activityPageSize = 250;
        const int activityHistoryConcurrency = 3;
        const int pgcrConcurrency = 3;
        var response = new GetDestiny2RaidDungeonStatsResponse();

        if (string.IsNullOrEmpty(_playerData.PrimaryMembershipId))
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = "Player not bound";
            return response;
        }

        if (request.MembershipType != _playerData.PrimaryMembershipType)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = $"Not found Destiny 2 profile by {request.MembershipType} platform.";
            return response;
        }

        var cacheKey = $"{Destiny2Constants.RaidDungeonStatsCachePrefix}:" +
                       $"{_playerData.PrimaryMembershipType}:{_playerData.PrimaryMembershipId}:{_playerData.Language}";
        var cached = await _cacheManager.Get<Destiny2RaidDungeonStatsDto>(cacheKey, cancellationToken)
            .ConfigureAwait(false);
        if (cached != null)
        {
            response.Data = cached;
            response.Code = HttpStatusCode.OK;
            response.Message = "Cached";
            return response;
        }

        var statsTask = _dataProvider.GetHistoricalStatsAsync(
            _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId,
            [Destiny2ActivityModeTypeEnum.Raid, Destiny2ActivityModeTypeEnum.Dungeon],
            cancellationToken);
        var metricsTask = _dataProvider.GetDestiny2ProfileAsync(
            _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId,
            [Destiny2ComponentTypeEnum.Characters, Destiny2ComponentTypeEnum.Metrics], cancellationToken);

        await Task.WhenAll(statsTask, metricsTask).ConfigureAwait(false);
        var statsResult = await statsTask.ConfigureAwait(false);
        var metricsResult = await metricsTask.ConfigureAwait(false);

        if (statsResult is not { ErrorCode: 1 } || metricsResult is not { ErrorCode: 1 })
        {
            var errorMessage = statsResult is not { ErrorCode: 1 }
                ? statsResult?.Message
                : metricsResult?.Message;
            response.Code = HttpStatusCode.BadRequest;
            response.Message = string.IsNullOrEmpty(errorMessage)
                ? "Failed to fetch raid and dungeon stats from Bungie API"
                : errorMessage;
            return response;
        }

        var manifestLanguageCode = LanguageExtension.MapI18nCodeToEnum(_playerData.Language).ToI18nCode();
        var activityModeJson = await _cacheManager.Get<string>(
            $"{Destiny2ManifestConstants.ActivityModeCacheKey}:{manifestLanguageCode}", cancellationToken)
            .ConfigureAwait(false);
        var englishActivityModeJson = string.Equals(manifestLanguageCode, "en", StringComparison.OrdinalIgnoreCase)
            ? activityModeJson
            : await _cacheManager.Get<string>(
                $"{Destiny2ManifestConstants.ActivityModeCacheKey}:en", cancellationToken).ConfigureAwait(false);
        activityModeJson ??= englishActivityModeJson;
        var metricJson = await _cacheManager.Get<string>(
            $"{Destiny2ManifestConstants.MetricCacheKey}:en", cancellationToken).ConfigureAwait(false);
        var activityJson = await _cacheManager.Get<string>(
            $"{Destiny2ManifestConstants.ActivityCacheKey}:{manifestLanguageCode}", cancellationToken)
            .ConfigureAwait(false);
        var englishActivityJson = string.Equals(manifestLanguageCode, "en", StringComparison.OrdinalIgnoreCase)
            ? activityJson
            : await _cacheManager.Get<string>(
                $"{Destiny2ManifestConstants.ActivityCacheKey}:en", cancellationToken).ConfigureAwait(false);
        activityJson ??= englishActivityJson;
        var activityModeManifest = activityModeJson?.DeserializeJson<Destiny2ManifestActivityModeDto>();
        var metricManifest = metricJson?.DeserializeJson<Destiny2ManifestMetricDto>();
        var activityManifest = activityJson?.DeserializeJson<Destiny2ManifestActivityDto>();
        var englishActivityManifest = englishActivityJson?.DeserializeJson<Destiny2ManifestActivityDto>();

        if (activityModeManifest == null || metricManifest == null ||
            activityManifest == null || englishActivityManifest == null)
        {
            response.Code = HttpStatusCode.ServiceUnavailable;
            response.Message = "Destiny 2 activity, activity mode, or metric manifest is unavailable";
            return response;
        }

        var metricValues = metricsResult.Response.Metrics.Data.Metrics;
        Dictionary<string, long> GetMentorCounts(uint parentNodeHash)
        {
            const string carriesSuffix = " Carries";

            return metricManifest
                .Where(x => x.Value.ParentNodeHashes.Contains(parentNodeHash) &&
                            x.Value.DisplayProperties.Name.EndsWith(carriesSuffix, StringComparison.OrdinalIgnoreCase))
                .GroupBy(x =>
                {
                    var activityName = x.Value.DisplayProperties.Name[..^carriesSuffix.Length];
                    const string dungeonPrefix = "Dungeon \"";
                    if (activityName.StartsWith(dungeonPrefix, StringComparison.OrdinalIgnoreCase) &&
                        activityName.EndsWith('"'))
                    {
                        activityName = activityName[dungeonPrefix.Length..^1];
                    }

                    return activityName;
                }, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => (long)group.Max(x =>
                        metricValues.GetValueOrDefault(x.Value.Hash)?.ObjectiveProgress.Progress ?? 0),
                    StringComparer.OrdinalIgnoreCase);
        }

        async Task<(Destiny2ActivityModeTypeEnum ActivityType, List<Destiny2CharacterActivity> Activities,
            string? ErrorMessage)>
            LoadActivityHistoryAsync(string characterId, Destiny2ActivityModeTypeEnum activityType)
        {
            var result = new List<Destiny2CharacterActivity>();
            for (var page = 0; ; page++)
            {
                var history = await _dataProvider.GetCharacterActivityHistoryAsync(
                    _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId,
                    characterId, (int)activityType, activityPageSize, page,
                    cancellationToken).ConfigureAwait(false);
                if (history is not { ErrorCode: 1 })
                    return (activityType, result, history?.Message ?? "Failed to fetch activity history from Bungie API");

                var activities = history.Response?.Activities ?? [];
                result.AddRange(activities.Select(activity => new Destiny2CharacterActivity(characterId, activity)));
                if (activities.Count < activityPageSize)
                    break;
            }

            return (activityType, result, null);
        }

        var historyRequests = metricsResult.Response.Characters.Data.Keys
            .SelectMany(characterId => new[]
            {
                (CharacterId: characterId, ActivityType: Destiny2ActivityModeTypeEnum.Raid),
                (CharacterId: characterId, ActivityType: Destiny2ActivityModeTypeEnum.Dungeon)
            })
            .ToList();
        var activityHistories = new Dictionary<Destiny2ActivityModeTypeEnum, List<Destiny2CharacterActivity>>
        {
            [Destiny2ActivityModeTypeEnum.Raid] = [],
            [Destiny2ActivityModeTypeEnum.Dungeon] = []
        };

        foreach (var requestBatch in historyRequests.Chunk(activityHistoryConcurrency))
        {
            var historyResults = await Task.WhenAll(requestBatch.Select(x =>
                LoadActivityHistoryAsync(x.CharacterId, x.ActivityType))).ConfigureAwait(false);
            var failedHistory = historyResults.FirstOrDefault(x => x.ErrorMessage != null);
            if (failedHistory.ErrorMessage != null)
            {
                response.Code = HttpStatusCode.BadRequest;
                response.Message = failedHistory.ErrorMessage;
                return response;
            }

            foreach (var historyResult in historyResults)
                activityHistories[historyResult.ActivityType].AddRange(historyResult.Activities);
        }

        static bool IsCompleted(Destiny2Activity activity)
        {
            return GetStatValue(activity.Values, "completed") > 0;
        }

        static long? GetStatValue(
            IReadOnlyDictionary<string, Destiny2HistoricalStatsValueDto> values, string statId)
        {
            return values.TryGetValue(statId, out var stat) && stat.Basic != null
                ? Math.Max(0, (long)stat.Basic.Value)
                : null;
        }

        static long GetStatSeconds(Destiny2Activity activity, string statId)
        {
            return GetStatValue(activity.Values, statId) ?? 0;
        }

        async Task<KeyValuePair<long, Destiny2PgcrStartSummaryDto?>> ReadPgcrCacheAsync(long instanceId)
        {
            var pgcrCacheKey = $"{Destiny2Constants.PostGameCarnageReportCachePrefix}:{instanceId}";
            try
            {
                var cachedSummary = await _cacheManager.Get<Destiny2PgcrStartSummaryDto>(
                    pgcrCacheKey, cancellationToken).ConfigureAwait(false);
                return KeyValuePair.Create(instanceId, cachedSummary);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to read Destiny 2 PGCR cache {InstanceId}", instanceId);
                return KeyValuePair.Create<long, Destiny2PgcrStartSummaryDto?>(instanceId, null);
            }
        }

        async Task WritePgcrCacheAsync(long instanceId, Destiny2PgcrStartSummaryDto summary)
        {
            var pgcrCacheKey = $"{Destiny2Constants.PostGameCarnageReportCachePrefix}:{instanceId}";
            var cacheDuration = summary.Available ? TimeSpan.FromMinutes(15) : TimeSpan.FromMinutes(5);
            try
            {
                await _cacheManager.Set(pgcrCacheKey, summary, cacheDuration, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to write Destiny 2 PGCR cache {InstanceId}", instanceId);
            }
        }

        async Task<KeyValuePair<long, Destiny2PgcrStartSummaryDto>> FetchPgcrStartSummaryAsync(long instanceId)
        {
            Destiny2PgcrStartSummaryDto summary;
            try
            {
                var pgcrResult = await _dataProvider.GetPostGameCarnageReportAsync(
                    instanceId, cancellationToken).ConfigureAwait(false);
                summary = pgcrResult is { ErrorCode: 1, Response: not null }
                    ? new Destiny2PgcrStartSummaryDto
                    {
                        Available = true,
                        DataVersion = Destiny2Constants.CurrentPgcrDataVersion,
                        Period = pgcrResult.Response.Period,
                        ReferenceId = pgcrResult.Response.ActivityDetails.ReferenceId,
                        StartingPhaseIndex = pgcrResult.Response.StartingPhaseIndex,
                        ActivityWasStartedFromBeginning = pgcrResult.Response.ActivityWasStartedFromBeginning,
                        Players = pgcrResult.Response.Entries
                            .Where(entry => !string.IsNullOrEmpty(entry.CharacterId))
                            .GroupBy(entry => entry.CharacterId, StringComparer.Ordinal)
                            .ToDictionary(
                                group => group.Key,
                                group =>
                                {
                                    var entry = group.First();
                                    var completionValue = GetStatValue(entry.Values, "completed");
                                    return new Destiny2PgcrPlayerSummaryDto
                                    {
                                        MembershipType = entry.Player.DestinyUserInfo.MembershipType,
                                        MembershipId = entry.Player.DestinyUserInfo.MembershipId,
                                        Completed = completionValue.HasValue
                                            ? completionValue.Value > 0
                                            : null,
                                        StartSeconds = GetStatValue(entry.Values, "startSeconds")
                                    };
                                },
                                StringComparer.Ordinal)
                    }
                    : new Destiny2PgcrStartSummaryDto();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to fetch Destiny 2 PGCR {InstanceId}", instanceId);
                summary = new Destiny2PgcrStartSummaryDto();
            }

            await WritePgcrCacheAsync(instanceId, summary).ConfigureAwait(false);
            return KeyValuePair.Create(instanceId, summary);
        }

        var pgcrInstanceIds = activityHistories.Values
            .SelectMany(x => x)
            .Where(characterActivity => englishActivityManifest.ContainsKey(
                characterActivity.Activity.ActivityDetails.ReferenceId.ToString()))
            .Where(characterActivity => IsCompleted(characterActivity.Activity))
            .Select(characterActivity => characterActivity.Activity.ActivityDetails.InstanceId)
            .Where(instanceId => instanceId > 0)
            .Distinct()
            .ToList();
        var pgcrStartSummaries = new Dictionary<long, Destiny2PgcrStartSummaryDto>();
        var negativeCachedInstanceIds = new HashSet<long>();
        foreach (var pgcrBatch in pgcrInstanceIds.Chunk(pgcrConcurrency))
        {
            var cachedSummaries = await Task.WhenAll(pgcrBatch.Select(ReadPgcrCacheAsync))
                .ConfigureAwait(false);
            foreach (var cachedSummary in cachedSummaries)
            {
                if (cachedSummary.Value is { Available: true } &&
                    cachedSummary.Value.DataVersion >= Destiny2Constants.CurrentPgcrDataVersion)
                {
                    pgcrStartSummaries[cachedSummary.Key] = cachedSummary.Value;
                }
                else if (cachedSummary.Value is { Available: false })
                {
                    negativeCachedInstanceIds.Add(cachedSummary.Key);
                }
            }
        }

        var databaseInstanceIds = pgcrInstanceIds
            .Where(instanceId => !pgcrStartSummaries.ContainsKey(instanceId))
            .ToList();
        try
        {
            var databaseSummaries = await _dataProvider.GetPgcrSummariesByDatabaseAsync(
                databaseInstanceIds, cancellationToken).ConfigureAwait(false);
            var databasePlayers = databaseSummaries.Players.ToLookup(x => x.InstanceId);
            foreach (var pgcr in databaseSummaries.Pgcrs)
            {
                if (pgcr.DataVersion < Destiny2Constants.CurrentPgcrDataVersion)
                    continue;

                var summary = new Destiny2PgcrStartSummaryDto
                {
                    Available = true,
                    DataVersion = pgcr.DataVersion,
                    Period = pgcr.Period,
                    ReferenceId = pgcr.ReferenceId,
                    StartingPhaseIndex = pgcr.StartingPhaseIndex,
                    ActivityWasStartedFromBeginning = pgcr.StartedFromBeginning,
                    Players = databasePlayers[pgcr.Id]
                        .GroupBy(player => player.CharacterId, StringComparer.Ordinal)
                        .ToDictionary(
                            group => group.Key,
                            group =>
                            {
                                var player = group.First();
                                return new Destiny2PgcrPlayerSummaryDto
                                {
                                    MembershipType = player.MembershipType,
                                    MembershipId = player.MembershipId,
                                    Completed = player.Completed,
                                    StartSeconds = player.StartSeconds
                                };
                            },
                            StringComparer.Ordinal)
                };
                pgcrStartSummaries[pgcr.Id] = summary;
            }

            foreach (var cacheBatch in databaseSummaries.Pgcrs
                         .Select(x => x.Id)
                         .Where(pgcrStartSummaries.ContainsKey)
                         .Chunk(pgcrConcurrency))
            {
                await Task.WhenAll(cacheBatch.Select(instanceId =>
                        WritePgcrCacheAsync(instanceId, pgcrStartSummaries[instanceId])))
                    .ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to read Destiny 2 PGCR summaries from database");
        }

        var bungieInstanceIds = pgcrInstanceIds
            .Where(instanceId => !pgcrStartSummaries.ContainsKey(instanceId) &&
                                 !negativeCachedInstanceIds.Contains(instanceId))
            .ToList();
        var fetchedSummaries = new Dictionary<long, Destiny2PgcrStartSummaryDto>();
        foreach (var pgcrBatch in bungieInstanceIds.Chunk(pgcrConcurrency))
        {
            var summaries = await Task.WhenAll(pgcrBatch.Select(FetchPgcrStartSummaryAsync))
                .ConfigureAwait(false);
            foreach (var summary in summaries)
            {
                pgcrStartSummaries[summary.Key] = summary.Value;
                if (summary.Value.Available)
                    fetchedSummaries[summary.Key] = summary.Value;
            }
        }

        if (fetchedSummaries.Count > 0)
        {
            var pgcrEntities = fetchedSummaries.Select(x => new Destiny2Pgcr
            {
                Id = x.Key,
                Period = x.Value.Period,
                ReferenceId = x.Value.ReferenceId,
                StartingPhaseIndex = x.Value.StartingPhaseIndex,
                StartedFromBeginning = x.Value.ActivityWasStartedFromBeginning,
                DataVersion = Destiny2Constants.CurrentPgcrDataVersion
            }).ToList();
            var playerEntities = fetchedSummaries.SelectMany(pgcr => pgcr.Value.Players.Select(player =>
                new Destiny2PgcrPlayer
                {
                    Id = Guid.NewGuid(),
                    InstanceId = pgcr.Key,
                    CharacterId = player.Key,
                    MembershipType = player.Value.MembershipType,
                    MembershipId = player.Value.MembershipId,
                    Completed = player.Value.Completed,
                    StartSeconds = player.Value.StartSeconds
                })).ToList();
            try
            {
                await _dataProvider.SavePgcrSummariesToDatabaseAsync(
                    pgcrEntities, playerEntities, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to save Destiny 2 PGCR summaries to database");
            }
        }

        List<Destiny2EndgameActivityDetailStatsDto> MapDetailStats(
            Destiny2ActivityModeTypeEnum activityType, IReadOnlyDictionary<string, long> mentorCounts)
        {
            var details = new Dictionary<string, Destiny2EndgameActivityDetailStatsDto>(
                StringComparer.OrdinalIgnoreCase);
            var confirmedFullCompletionDurationTotals = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            var confirmedFullCompletionCounts = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            var detailActivityNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var activityReleaseTimes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

            foreach (var characterActivity in activityHistories[activityType])
            {
                var activity = characterActivity.Activity;
                if (!englishActivityManifest.TryGetValue(activity.ActivityDetails.ReferenceId.ToString(),
                        out var englishDefinition))
                    continue;

                activityManifest.TryGetValue(activity.ActivityDetails.ReferenceId.ToString(), out var definition);
                definition ??= englishDefinition;
                var englishName = string.IsNullOrWhiteSpace(englishDefinition.OriginalDisplayProperties.Name)
                    ? englishDefinition.DisplayProperties.Name
                    : englishDefinition.OriginalDisplayProperties.Name;
                var localizedName = string.IsNullOrWhiteSpace(definition.OriginalDisplayProperties.Name)
                    ? definition.DisplayProperties.Name
                    : definition.OriginalDisplayProperties.Name;
                var difficulty = ResolveDifficulty(englishDefinition);
                var detailKey = $"{englishName}:{(int)difficulty}";
                var releaseTime = Math.Max(0, englishDefinition.ReleaseTime);
                if (releaseTime > 0 &&
                    (!activityReleaseTimes.TryGetValue(englishName, out var currentReleaseTime) ||
                     releaseTime < currentReleaseTime))
                {
                    activityReleaseTimes[englishName] = releaseTime;
                }

                if (!details.TryGetValue(detailKey, out var detail))
                {
                    detail = new Destiny2EndgameActivityDetailStatsDto
                    {
                        Name = localizedName,
                        Difficulty = difficulty,
                        ImageUrl = string.IsNullOrEmpty(definition.PgcrImage)
                            ? string.Empty
                            : $"{BungieConstants.BungieUrl}{definition.PgcrImage}",
                        MentorCount = GetDetailMentorCount(englishName, difficulty, mentorCounts)
                    };
                    details[detailKey] = detail;
                    detailActivityNames[detailKey] = englishName;
                }

                if (!detail.ActivityHashes.Contains(activity.ActivityDetails.ReferenceId))
                    detail.ActivityHashes.Add(activity.ActivityDetails.ReferenceId);
                detail.ActivityCount++;
                var timePlayed = activity.Values.GetValueOrDefault("timePlayedSeconds");
                var activityDuration = activity.Values.GetValueOrDefault("activityDurationSeconds");
                var timePlayedSeconds = Math.Max(0, (long)(timePlayed?.Basic?.Value ?? 0));
                var activityDurationSeconds = Math.Max(0, (long)(activityDuration?.Basic?.Value ?? 0));
                var isCompleted = IsCompleted(activity);
                if (isCompleted)
                {
                    detail.CompletionCount++;

                    var playerCount = Math.Max(0, (int)(activity.Values
                        .GetValueOrDefault("playerCount")?.Basic?.Value ?? 0));
                    var isTrioCompletion = playerCount == 3;
                    var isDuoCompletion = playerCount == 2;
                    var isSoloCompletion = playerCount == 1;
                    var isFlawlessCompletion = activity.Values.TryGetValue("deaths", out var deaths) &&
                                               deaths.Basic?.Value == 0;
                    pgcrStartSummaries.TryGetValue(activity.ActivityDetails.InstanceId, out var pgcrSummary);
                    var pgcrPlayer = pgcrSummary != null &&
                                     pgcrSummary.Players.TryGetValue(
                                         characterActivity.CharacterId, out var playerSummary)
                        ? playerSummary
                        : null;
                    var fullCompletionStatus = Destiny2FullCompletionResolver.Resolve(
                        isCompleted, timePlayedSeconds, activityDurationSeconds, activity.Period,
                        pgcrSummary, pgcrPlayer, GetStatValue(activity.Values, "startSeconds"));
                    var isFullCompletion = Destiny2FullCompletionResolver.IsFull(fullCompletionStatus);
                    detail.HasFullTrioCompletion |= isTrioCompletion && isFullCompletion;
                    detail.HasFullDuoCompletion |= isDuoCompletion && isFullCompletion;
                    detail.HasFullSoloCompletion |= isSoloCompletion && isFullCompletion;
                    detail.HasFlawlessCompletion |= isFlawlessCompletion && isFullCompletion;

                    if (isFullCompletion)
                    {
                        detail.FullCompletionCount++;
                    }

                    if (Destiny2FullCompletionResolver.IsConfirmedFull(fullCompletionStatus))
                    {
                        confirmedFullCompletionDurationTotals[detailKey] =
                            confirmedFullCompletionDurationTotals.GetValueOrDefault(detailKey) +
                            activityDurationSeconds;
                        confirmedFullCompletionCounts[detailKey] =
                            confirmedFullCompletionCounts.GetValueOrDefault(detailKey) + 1;

                        if (detail.FastestCompletionSeconds == null ||
                            activityDurationSeconds < detail.FastestCompletionSeconds)
                        {
                            detail.FastestCompletionSeconds = activityDurationSeconds;
                        }
                    }
                }

                var playtime = timePlayed ?? activityDuration;
                detail.PlaytimeSeconds += Math.Max(0, (long)(playtime?.Basic?.Value ?? 0));
            }

            foreach (var (detailKey, detail) in details)
            {
                if (confirmedFullCompletionCounts.TryGetValue(detailKey, out var confirmedCount) &&
                    confirmedCount > 0)
                {
                    detail.AverageFullCompletionSeconds =
                        (double)confirmedFullCompletionDurationTotals[detailKey] / confirmedCount;
                }
            }

            return details
                .OrderByDescending(x => activityReleaseTimes.GetValueOrDefault(detailActivityNames[x.Key]))
                .ThenBy(x => x.Value.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Value.Difficulty)
                .Select(x => x.Value)
                .ToList();
        }

        static Destiny2EndgameActivityDifficultyEnum ResolveDifficulty(
            Destiny2ManifestActivityDefinitionDto definition)
        {
            var name = definition.DisplayProperties.Name;
            if (name.Contains(": Master", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("Epic Raid", StringComparison.OrdinalIgnoreCase))
                return Destiny2EndgameActivityDifficultyEnum.Master;
            if (name.Contains(": Expert", StringComparison.OrdinalIgnoreCase))
                return Destiny2EndgameActivityDifficultyEnum.Expert;
            if (name.Contains(": Contest", StringComparison.OrdinalIgnoreCase))
                return Destiny2EndgameActivityDifficultyEnum.Contest;
            if (name.Contains(": Prestige", StringComparison.OrdinalIgnoreCase))
                return Destiny2EndgameActivityDifficultyEnum.Prestige;
            if (name.Contains("Challenge Mode", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("The Pantheon:", StringComparison.OrdinalIgnoreCase))
                return Destiny2EndgameActivityDifficultyEnum.Challenge;

            return Destiny2EndgameActivityDifficultyEnum.Normal;
        }

        static long GetDetailMentorCount(
            string activityName,
            Destiny2EndgameActivityDifficultyEnum difficulty,
            IReadOnlyDictionary<string, long> mentorCounts)
        {
            return difficulty switch
            {
                Destiny2EndgameActivityDifficultyEnum.Normal => mentorCounts.GetValueOrDefault(activityName),
                Destiny2EndgameActivityDifficultyEnum.Master =>
                    mentorCounts.GetValueOrDefault($"{activityName} Epic Raid") +
                    mentorCounts.GetValueOrDefault($"{activityName} Master"),
                Destiny2EndgameActivityDifficultyEnum.Expert =>
                    mentorCounts.GetValueOrDefault($"{activityName} Expert"),
                _ => 0
            };
        }

        Destiny2EndgameActivityStatsDto MapStats(
            Destiny2ActivityModeTypeEnum activityType,
            IReadOnlyDictionary<string, long> mentorCounts)
        {
            var allTime = statsResult.Response
                .FirstOrDefault(x => string.Equals(x.Key, activityType.ToString(), StringComparison.OrdinalIgnoreCase))
                .Value?.AllTime;
            var definition = activityModeManifest.Values.FirstOrDefault(x => x.ModeType == activityType);

            return new Destiny2EndgameActivityStatsDto
            {
                ActivityType = activityType,
                Name = definition?.DisplayProperties.Name ?? activityType.ToString(),
                ActivityCount = Math.Max(0, (long)(allTime?.ActivitiesEntered?.Basic?.Value ?? 0)),
                CompletionCount = Math.Max(0, (long)(allTime?.ActivitiesCleared?.Basic?.Value ?? 0)),
                MentorCount = mentorCounts.Values.Sum(),
                PlaytimeSeconds = Math.Max(0, (long)(allTime?.SecondsPlayed?.Basic?.Value ?? 0)),
                Details = MapDetailStats(activityType, mentorCounts)
            };
        }

        var raidMentorCounts = GetMentorCounts(Destiny2Constants.RaidMetricParentNodeHash);
        var dungeonMentorCounts = GetMentorCounts(Destiny2Constants.DungeonMetricParentNodeHash);

        response.Data = new Destiny2RaidDungeonStatsDto
        {
            Activities =
            [
                MapStats(Destiny2ActivityModeTypeEnum.Raid, raidMentorCounts),
                MapStats(Destiny2ActivityModeTypeEnum.Dungeon, dungeonMentorCounts)
            ]
        };
        response.Code = HttpStatusCode.OK;
        response.Message = "Success";

        await _cacheManager.Set(cacheKey, response.Data, TimeSpan.FromMinutes(5), cancellationToken)
            .ConfigureAwait(false);

        return response;
    }

    public async Task<GetDestiny2SeasonResponse> GetDestiny2SeasonAsync(
        GetDestiny2SeasonCommand command, CancellationToken cancellationToken)
    {
        const int activityPageSize = 250;
        const int activityHistoryConcurrency = 3;
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

        _logger.Information(
            "Starting Destiny 2 season playtime query for {MembershipType}:{MembershipId}",
            _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId);

        var cacheKey = $"{Destiny2Constants.SeasonStatsCachePrefix}:" +
                       $"{_playerData.PrimaryMembershipType}:{_playerData.PrimaryMembershipId}";
        var cached = await _cacheManager.Get<Destiny2SeasonStatsDto>(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cached != null)
        {
            _logger.Information(
                "Destiny 2 season playtime cache hit for {MembershipType}:{MembershipId}. SeasonCount: {SeasonCount}",
                _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId, cached.Seasons.Count);
            response.Data = cached;
            response.Code = HttpStatusCode.OK;
            response.Message = "Cached";
            return response;
        }

        var lockKey = $"{cacheKey}:Lock";
        if (!ActiveSeasonCalculations.TryAdd(lockKey, 0))
        {
            _logger.Information(
                "Destiny 2 season playtime calculation is already in progress for {MembershipType}:{MembershipId}",
                _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId);
            response.Code = HttpStatusCode.TooManyRequests;
            response.Message = "Season playtime calculation is already in progress";
            return response;
        }

        bool lockAcquired;
        try
        {
            lockAcquired = await _cacheManager.TryLockAsync(
                lockKey, TimeSpan.FromMinutes(10), cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            ActiveSeasonCalculations.TryRemove(lockKey, out _);
            throw;
        }

        if (!lockAcquired)
        {
            ActiveSeasonCalculations.TryRemove(lockKey, out _);
            _logger.Information(
                "Destiny 2 season playtime calculation is already in progress for {MembershipType}:{MembershipId}",
                _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId);
            response.Code = HttpStatusCode.TooManyRequests;
            response.Message = "Season playtime calculation is already in progress";
            return response;
        }

        await using var calculationLock = new Destiny2SeasonCalculationLock(
            _cacheManager, _logger, lockKey,
            _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId);

        cached = await _cacheManager.Get<Destiny2SeasonStatsDto>(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cached != null)
        {
            _logger.Information(
                "Destiny 2 season playtime cache filled while waiting for calculation lock for " +
                "{MembershipType}:{MembershipId}. SeasonCount: {SeasonCount}",
                _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId, cached.Seasons.Count);
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
            _logger.Warning(
                "Failed to load Destiny 2 characters for {MembershipType}:{MembershipId}. Message: {Message}",
                _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId, charactersResult?.Message);
            response.Code = HttpStatusCode.BadRequest;
            response.Message = charactersResult?.Message ?? "Failed to fetch profile data from Bungie API";
            return response;
        }

        var characterMap = charactersResult.Response.Characters.Data;
        var characterClassMap = characterMap
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ClassType);

        _logger.Information(
            "Loaded {CharacterCount} Destiny 2 characters for season playtime query",
            characterClassMap.Count);

        var accountStatsResult = await _dataProvider.GetAccountStatsAsync(
            _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId, cancellationToken).ConfigureAwait(false);

        if (accountStatsResult is not { ErrorCode: 1 })
        {
            _logger.Warning(
                "Failed to load Destiny 2 account stats for {MembershipType}:{MembershipId}. Message: {Message}",
                _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId, accountStatsResult?.Message);
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

            characterMap.TryGetValue(characterId, out var character);
            var classType = character?.ClassType ?? Destiny2ClassEnum.Unknown;
            var playtimeSeconds = (long)(charData.Merged?.AllTime?.SecondsPlayed?.Basic?.Value ?? 0);

            characterStats.Add(new CharacterPlaytimeDto
            {
                CharacterId = characterId,
                Class = classType,
                EmblemUrl = string.IsNullOrEmpty(character?.EmblemPath)
                    ? string.Empty
                    : $"{BungieConstants.BungieUrl}{character.EmblemPath}",
                EmblemBackgroundUrl = string.IsNullOrEmpty(character?.EmblemBackgroundPath)
                    ? string.Empty
                    : $"{BungieConstants.BungieUrl}{character.EmblemBackgroundPath}",
                PlaytimeSeconds = playtimeSeconds
            });
        }

        var totalPlaytimeSeconds = characterStats.Sum(c => c.PlaytimeSeconds);

        _logger.Information(
            "Loaded Destiny 2 lifetime playtime. CharacterCount: {CharacterCount}, TotalPlaytimeSeconds: {TotalPlaytimeSeconds}",
            characterStats.Count, totalPlaytimeSeconds);

        var manifestLanguageCode = LanguageExtension.MapI18nCodeToEnum(_playerData.Language).ToI18nCode();
        var seasonJson = await _cacheManager.Get<string>(
            $"{Destiny2ManifestConstants.SeasonCacheKey}:{manifestLanguageCode}", cancellationToken).ConfigureAwait(false);
        var seasonManifest = seasonJson?.DeserializeJson<Destiny2ManifestSeasonDto>();

        if (seasonManifest == null)
        {
            _logger.Warning(
                "Destiny 2 season manifest cache is unavailable for language {Language}",
                manifestLanguageCode);
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

        _logger.Information(
            "Loaded Destiny 2 season manifest for {Language}. AvailableSeasonCount: {SeasonCount}",
            manifestLanguageCode, seasons.Count);

        var storedHistories = await _dataProvider.GetPlayerSeasonHistoryByDatabaseAsync(
            _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId,
            cancellationToken).ConfigureAwait(false);
        var storedHistoryMap = storedHistories.ToDictionary(x => x.SeasonHash);

        _logger.Information(
            "Loaded {StoredSeasonCount} persisted Destiny 2 season histories for {MembershipType}:{MembershipId}",
            storedHistoryMap.Count, _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId);

        var completedSeasons = seasons.Where(x => x.EndDate <= utcNow).ToList();
        var missingCompletedSeasons = completedSeasons
            .Where(x => !storedHistoryMap.ContainsKey(x.Hash))
            .ToList();
        var currentSeason = seasons.LastOrDefault(x => x.StartDate <= utcNow && utcNow < x.EndDate);

        var seasonsToCalculate = missingCompletedSeasons.ToList();
        if (currentSeason != null)
            seasonsToCalculate.Add(currentSeason);

        _logger.Information(
            "Prepared Destiny 2 season aggregation. CompletedSeasonCount: {CompletedSeasonCount}, " +
            "MissingCompletedSeasonCount: {MissingCompletedSeasonCount}, CurrentSeasonNumber: {CurrentSeasonNumber}",
            completedSeasons.Count, missingCompletedSeasons.Count, currentSeason?.SeasonNumber);

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

            async Task<(
                string CharacterId,
                Destiny2ClassEnum Class,
                Dictionary<uint, long> Seconds,
                int? FailedPage,
                string? ErrorMessage)> CalculateCharacterActivityHistoryAsync(
                KeyValuePair<string, Destiny2ClassEnum> character)
            {
                var characterSeconds = seasonsToCalculate.ToDictionary(x => x.Hash, _ => 0L);

                for (var page = 0; ; page++)
                {
                    var activityResult = await _dataProvider.GetCharacterActivityHistoryAsync(
                        _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId,
                        character.Key, 0, activityPageSize, page,
                        cancellationToken).ConfigureAwait(false);

                    if (activityResult is not { ErrorCode: 1 })
                    {
                        return (
                            character.Key,
                            character.Value,
                            characterSeconds,
                            page,
                            activityResult?.Message ?? "Failed to fetch activity history from Bungie API");
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
                        characterSeconds[season.Hash] += durationSeconds;
                    }

                    if (activities.Count < activityPageSize ||
                        activities.Min(x => x.Period.UtcDateTime) < earliestRequiredDate)
                        break;
                }

                return (character.Key, character.Value, characterSeconds, null, null);
            }

            var charactersToCalculate = characterClassMap
                .Where(x => x.Value != Destiny2ClassEnum.Unknown)
                .ToList();

            _logger.Information(
                "Loading Destiny 2 activity histories. CharacterCount: {CharacterCount}, MaxConcurrency: {MaxConcurrency}",
                charactersToCalculate.Count, activityHistoryConcurrency);

            foreach (var characterBatch in charactersToCalculate.Chunk(activityHistoryConcurrency))
            {
                var characterResults = await Task.WhenAll(
                    characterBatch.Select(CalculateCharacterActivityHistoryAsync)).ConfigureAwait(false);
                var failedResult = characterResults.FirstOrDefault(x => x.ErrorMessage != null);
                if (failedResult.ErrorMessage != null)
                {
                    _logger.Warning(
                        "Failed to load Destiny 2 activity history for character {CharacterId}, page {Page}. Message: {Message}",
                        failedResult.CharacterId, failedResult.FailedPage, failedResult.ErrorMessage);
                    response.Code = HttpStatusCode.BadRequest;
                    response.Message = failedResult.ErrorMessage;
                    return response;
                }

                foreach (var characterResult in characterResults)
                {
                    foreach (var seasonSeconds in characterResult.Seconds)
                    {
                        calculatedSeconds[seasonSeconds.Key][characterResult.Class] += seasonSeconds.Value;
                    }
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
            .ToList();

        await _dataProvider.SavePlayerSeasonHistoryToDatabaseAsync(newHistories, cancellationToken).ConfigureAwait(false);

        _logger.Information(
            "Prepared {PersistedSeasonCount} completed Destiny 2 season histories for persistence",
            newHistories.Count);

        foreach (var history in newHistories)
            storedHistoryMap[history.SeasonHash] = history;

        var currentSeasonHash = currentSeason?.Hash;
        var seasonStats = seasons
            .OrderByDescending(x => x.StartDate)
            .Select(season =>
            {
                var classSeconds = calculatedSeconds.GetValueOrDefault(season.Hash);
                storedHistoryMap.TryGetValue(season.Hash, out var storedHistory);
                var seasonIconPath = string.IsNullOrEmpty(season.DisplayProperties.HighResIcon)
                    ? season.DisplayProperties.Icon
                    : season.DisplayProperties.HighResIcon;

                return new Destiny2SeasonPlaytimeDto
                {
                    SeasonHash = season.Hash,
                    SeasonNumber = season.SeasonNumber,
                    Name = season.DisplayProperties.Name,
                    IconUrl = string.IsNullOrEmpty(seasonIconPath)
                        ? string.Empty
                        : $"{BungieConstants.BungieUrl}{seasonIconPath}",
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

        _logger.Information(
            "Completed Destiny 2 season playtime query for {MembershipType}:{MembershipId}. " +
            "ReturnedSeasonCount: {SeasonCount}, TotalPlaytimeSeconds: {TotalPlaytimeSeconds}",
            _playerData.PrimaryMembershipType, _playerData.PrimaryMembershipId,
            response.Data.Seasons.Count, response.Data.TotalPlaytimeSeconds);

        return response;
    }

    private sealed class Destiny2SeasonCalculationLock(
        ICacheManager cacheManager,
        ILogger logger,
        string lockKey,
        BungieMembershipTypeEnum membershipType,
        string membershipId) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            try
            {
                await cacheManager.Remove(lockKey, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.Warning(
                    ex,
                    "Failed to release Destiny 2 season playtime calculation lock for " +
                    "{MembershipType}:{MembershipId}",
                    membershipType, membershipId);
            }
            finally
            {
                ActiveSeasonCalculations.TryRemove(lockKey, out _);
            }
        }
    }
}
