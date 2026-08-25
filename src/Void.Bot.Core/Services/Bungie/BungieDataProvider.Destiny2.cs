using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Domain.Destiny2;
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

    Task<Destiny2StatsResponse?> GetAccountStatsAsync(
        BungieMembershipTypeEnum membershipType, string membershipId,
        CancellationToken cancellationToken, string accessToken = "");

    Task<Destiny2HistoricalStatsResponse?> GetHistoricalStatsAsync(
        BungieMembershipTypeEnum membershipType, string membershipId,
        IReadOnlyCollection<Destiny2ActivityModeTypeEnum> modes,
        CancellationToken cancellationToken, string accessToken = "");

    Task<Destiny2ActivityHistoryResponse?> GetCharacterActivityHistoryAsync(
        BungieMembershipTypeEnum membershipType, string membershipId, string characterId,
        int mode, int count, int page,
        CancellationToken cancellationToken, string accessToken = "");

    Task<Destiny2PostGameCarnageReportResponse?> GetPostGameCarnageReportAsync(
        long activityId, CancellationToken cancellationToken, string accessToken = "");

    Task<(List<Destiny2Pgcr> Pgcrs, List<Destiny2PgcrPlayer> Players)>
        GetPgcrSummariesByDatabaseAsync(
            IReadOnlyCollection<long> instanceIds, CancellationToken cancellationToken);

    Task SavePgcrSummariesToDatabaseAsync(
        IReadOnlyCollection<Destiny2Pgcr> pgcrs,
        IReadOnlyCollection<Destiny2PgcrPlayer> players,
        CancellationToken cancellationToken);

    Task<List<Destiny2SeasonHistory>> GetPlayerSeasonHistoryByDatabaseAsync(
        BungieMembershipTypeEnum primaryMembershipType, string primaryMembershipId,
        CancellationToken cancellationToken);

    Task SavePlayerSeasonHistoryToDatabaseAsync(
        List<Destiny2SeasonHistory> seasonHistories, CancellationToken cancellationToken);
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

    public async Task<Destiny2StatsResponse?> GetAccountStatsAsync(
        BungieMembershipTypeEnum membershipType, string membershipId,
        CancellationToken cancellationToken, string accessToken = "")
    {
        var url = Destiny2Constants.AccountStatsUrl
            .Replace("{membershipType}", ((int)membershipType).ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("{membershipId}", membershipId, StringComparison.OrdinalIgnoreCase);

        var result = await GetBungieHttpClient(accessToken)
            .GetAsync<Destiny2StatsResponse>(url, cancellationToken).ConfigureAwait(false);

        return result.Data;
    }

    public async Task<Destiny2HistoricalStatsResponse?> GetHistoricalStatsAsync(
        BungieMembershipTypeEnum membershipType, string membershipId,
        IReadOnlyCollection<Destiny2ActivityModeTypeEnum> modes,
        CancellationToken cancellationToken, string accessToken = "")
    {
        var url = Destiny2Constants.CharacterStatsUrl
            .Replace("{membershipType}", ((int)membershipType).ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("{membershipId}", membershipId, StringComparison.OrdinalIgnoreCase)
            .Replace("{characterId}", "0", StringComparison.OrdinalIgnoreCase);

        url += $"?modes={string.Join(",", modes.Select(x => (int)x))}";

        var result = await GetBungieHttpClient(accessToken)
            .GetAsync<Destiny2HistoricalStatsResponse>(url, cancellationToken).ConfigureAwait(false);

        return result.Data;
    }

    public async Task<Destiny2ActivityHistoryResponse?> GetCharacterActivityHistoryAsync(
        BungieMembershipTypeEnum membershipType, string membershipId, string characterId,
        int mode, int count, int page,
        CancellationToken cancellationToken, string accessToken = "")
    {
        var url = Destiny2Constants.CharacterActivityHistoryUrl
            .Replace("{membershipType}", ((int)membershipType).ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("{membershipId}", membershipId, StringComparison.OrdinalIgnoreCase)
            .Replace("{characterId}", characterId, StringComparison.OrdinalIgnoreCase);

        url += $"?mode={mode}&count={count}&page={page}";

        var result = await GetBungieHttpClient(accessToken)
            .GetAsync<Destiny2ActivityHistoryResponse>(url, cancellationToken).ConfigureAwait(false);

        return result.Data;
    }

    public async Task<Destiny2PostGameCarnageReportResponse?> GetPostGameCarnageReportAsync(
        long activityId, CancellationToken cancellationToken, string accessToken = "")
    {
        var url = Destiny2Constants.PostGameCarnageReportUrl
            .Replace("{activityId}", activityId.ToString(), StringComparison.OrdinalIgnoreCase);

        var result = await GetBungieHttpClient(accessToken)
            .GetAsync<Destiny2PostGameCarnageReportResponse>(url, cancellationToken).ConfigureAwait(false);

        return result.Data;
    }

    public async Task<(List<Destiny2Pgcr> Pgcrs, List<Destiny2PgcrPlayer> Players)>
        GetPgcrSummariesByDatabaseAsync(
            IReadOnlyCollection<long> instanceIds, CancellationToken cancellationToken)
    {
        var pgcrs = new List<Destiny2Pgcr>();
        var players = new List<Destiny2PgcrPlayer>();
        foreach (var instanceIdBatch in instanceIds.Distinct().Chunk(500))
        {
            pgcrs.AddRange(await _pgcrRepository.QueryNoTracking()
                .Where(x => instanceIdBatch.Contains(x.Id))
                .ToListAsync(cancellationToken).ConfigureAwait(false));
            players.AddRange(await _pgcrPlayerRepository.QueryNoTracking()
                .Where(x => instanceIdBatch.Contains(x.InstanceId))
                .ToListAsync(cancellationToken).ConfigureAwait(false));
        }

        return (pgcrs, players);
    }

    public async Task SavePgcrSummariesToDatabaseAsync(
        IReadOnlyCollection<Destiny2Pgcr> pgcrs,
        IReadOnlyCollection<Destiny2PgcrPlayer> players,
        CancellationToken cancellationToken)
    {
        if (pgcrs.Count == 0)
            return;

        var instanceIds = pgcrs.Select(x => x.Id).Distinct().ToList();
        var existingPgcrs = new List<Destiny2Pgcr>();
        var existingPlayers = new List<Destiny2PgcrPlayer>();
        foreach (var instanceIdBatch in instanceIds.Chunk(500))
        {
            existingPgcrs.AddRange(await _pgcrRepository.QueryNoTracking()
                .Where(x => instanceIdBatch.Contains(x.Id))
                .ToListAsync(cancellationToken).ConfigureAwait(false));
            existingPlayers.AddRange(await _pgcrPlayerRepository.QueryNoTracking()
                .Where(x => instanceIdBatch.Contains(x.InstanceId))
                .ToListAsync(cancellationToken).ConfigureAwait(false));
        }

        var existingPgcrById = existingPgcrs.ToDictionary(x => x.Id);
        var newPgcrs = new List<Destiny2Pgcr>();
        var updatedPgcrs = new List<Destiny2Pgcr>();
        foreach (var pgcr in pgcrs.GroupBy(x => x.Id).Select(x => x.Last()))
        {
            if (!existingPgcrById.TryGetValue(pgcr.Id, out var existingPgcr))
            {
                newPgcrs.Add(pgcr);
                continue;
            }

            existingPgcr.Period = pgcr.Period;
            existingPgcr.ReferenceId = pgcr.ReferenceId;
            existingPgcr.StartingPhaseIndex = pgcr.StartingPhaseIndex;
            existingPgcr.StartedFromBeginning = pgcr.StartedFromBeginning;
            existingPgcr.DataVersion = pgcr.DataVersion;
            updatedPgcrs.Add(existingPgcr);
        }

        var existingPlayerByKey = existingPlayers.ToDictionary(
            x => (x.InstanceId, x.CharacterId));
        var newPlayers = new List<Destiny2PgcrPlayer>();
        var updatedPlayers = new List<Destiny2PgcrPlayer>();
        foreach (var player in players
                     .GroupBy(x => (x.InstanceId, x.CharacterId))
                     .Select(x => x.Last()))
        {
            if (!existingPlayerByKey.TryGetValue(
                    (player.InstanceId, player.CharacterId), out var existingPlayer))
            {
                newPlayers.Add(player);
                continue;
            }

            existingPlayer.MembershipType = player.MembershipType;
            existingPlayer.MembershipId = player.MembershipId;
            existingPlayer.Completed = player.Completed;
            existingPlayer.StartSeconds = player.StartSeconds;
            updatedPlayers.Add(existingPlayer);
        }

        if (newPgcrs.Count > 0)
            await _pgcrRepository.InsertManyAsync(newPgcrs, cancellationToken).ConfigureAwait(false);
        if (updatedPgcrs.Count > 0)
            await _pgcrRepository.UpdateManyAsync(updatedPgcrs, cancellationToken).ConfigureAwait(false);
        if (newPlayers.Count > 0)
            await _pgcrPlayerRepository.InsertManyAsync(newPlayers, cancellationToken).ConfigureAwait(false);
        if (updatedPlayers.Count > 0)
            await _pgcrPlayerRepository.UpdateManyAsync(updatedPlayers, cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<Destiny2SeasonHistory>> GetPlayerSeasonHistoryByDatabaseAsync(
        BungieMembershipTypeEnum primaryMembershipType, string primaryMembershipId,
        CancellationToken cancellationToken)
    {
        return await _seasonHistoryRepository.QueryNoTracking()
            .Where(x => x.MembershipType == primaryMembershipType && x.MembershipId == primaryMembershipId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task SavePlayerSeasonHistoryToDatabaseAsync(
        List<Destiny2SeasonHistory> seasonHistories, CancellationToken cancellationToken)
    {
        if (seasonHistories.Count == 0)
            return;

        await _seasonHistoryRepository.InsertManyAsync(seasonHistories, cancellationToken).ConfigureAwait(false);
    }
}
