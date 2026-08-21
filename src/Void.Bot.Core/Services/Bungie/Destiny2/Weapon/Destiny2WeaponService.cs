using System.Net;
using Serilog;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Services;
using Void.Bot.Core.Services.Jobs;
using Void.Bot.Core.Settings.Bungie;
using Void.Bot.Message.Commands.Bungie.Destiny2;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;
using Void.Bot.Message.Enum.Bungie.Destiny2;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Services.Bungie.Destiny2.Weapon;

public interface IDestiny2WeaponService : IScope
{
    Task<GetDestiny2WeaponTypesResponse> GetWeaponTypesAsync(
        GetDestiny2WeaponTypesRequest request,
        CancellationToken cancellationToken);

    Task<GetDestiny2WeaponPerksResponse> GetPerksAsync(
        GetDestiny2WeaponPerksRequest request,
        CancellationToken cancellationToken);

    Task<FilterDestiny2WeaponGodRollResponse> FilterGodRollAsync(
        FilterDestiny2WeaponGodRollRequest request,
        CancellationToken cancellationToken);

    Task<RebuildDestiny2WeaponCacheResponse> RebuildCacheAsync(
        RebuildDestiny2WeaponCacheCommand command,
        CancellationToken cancellationToken);

    Task RebuildCacheInBackgroundAsync(
        List<string> languages,
        CancellationToken cancellationToken);
}

internal sealed class Destiny2WeaponService : IDestiny2WeaponService
{
    private readonly IDestiny2WeaponDataProvider _dataProvider;
    private readonly Destiny2Setting _setting;
    private readonly IVoidBotBackgroundJobClient _backgroundJob;

    public Destiny2WeaponService(
        IDestiny2WeaponDataProvider dataProvider,
        Destiny2Setting setting,
        IVoidBotBackgroundJobClient backgroundJob)
    {
        _dataProvider = dataProvider;
        _setting = setting;
        _backgroundJob = backgroundJob;
    }

    public Task<RebuildDestiny2WeaponCacheResponse> RebuildCacheAsync(
        RebuildDestiny2WeaponCacheCommand command,
        CancellationToken cancellationToken)
    {
        var requestedLanguages = command.Languages is { Count: > 0 }
            ? command.Languages
            : _setting.SyncLanguages;
        var languages = requestedLanguages
            .Select(language => LanguageExtension.MapI18nCodeToEnum(language).ToI18nCode())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (languages.Count == 0)
        {
            return Task.FromResult(new RebuildDestiny2WeaponCacheResponse
            {
                Code = HttpStatusCode.BadRequest,
                Message = "No weapon cache languages were requested or configured."
            });
        }

        cancellationToken.ThrowIfCancellationRequested();
        var jobId = _backgroundJob.Enqueue<IDestiny2WeaponService>(service =>
            service.RebuildCacheInBackgroundAsync(languages, CancellationToken.None));

        return Task.FromResult(new RebuildDestiny2WeaponCacheResponse
        {
            Code = HttpStatusCode.Accepted,
            Message = "Weapon cache rebuild job queued.",
            Data = new Destiny2WeaponCacheRebuildJobDto
            {
                JobId = jobId,
                Languages = languages
            }
        });
    }

    public async Task RebuildCacheInBackgroundAsync(
        List<string> languages,
        CancellationToken cancellationToken)
    {
        foreach (var language in languages)
        {
            try
            {
                var success = await _dataProvider.RefreshAsync(language, cancellationToken).ConfigureAwait(false);
                if (success)
                {
                    Log.Information(
                        "Destiny 2 weapon cache rebuild completed for language: {Language}",
                        language);
                }
                else
                {
                    Log.Warning(
                        "Destiny 2 weapon cache rebuild skipped because required manifest components are unavailable. Language: {Language}",
                        language);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to rebuild Destiny 2 weapon cache for language: {Language}", language);
            }
        }
    }

    public async Task<GetDestiny2WeaponTypesResponse> GetWeaponTypesAsync(
        GetDestiny2WeaponTypesRequest request,
        CancellationToken cancellationToken)
    {
        var weaponTypeData = await _dataProvider.GetWeaponTypesAsync(request.Language, cancellationToken).ConfigureAwait(false);
        if (weaponTypeData == null)
            return ManifestUnavailable<GetDestiny2WeaponTypesResponse>();

        var weaponTypes = weaponTypeData.Values
            .Select(MapWeaponType)
            .Where(weaponType => !string.IsNullOrWhiteSpace(weaponType.Name))
            .OrderBy(weaponType => weaponType.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(weaponType => weaponType.WeaponType)
            .ToList();

        return new GetDestiny2WeaponTypesResponse
        {
            Data = new Destiny2WeaponTypesDto
            {
                WeaponTypes = weaponTypes
            }
        };
    }

    public async Task<GetDestiny2WeaponPerksResponse> GetPerksAsync(
        GetDestiny2WeaponPerksRequest request,
        CancellationToken cancellationToken)
    {
        var rollData = await _dataProvider.GetRollDataAsync(
            request.Language,
            request.WeaponTypes,
            cancellationToken).ConfigureAwait(false);
        if (rollData == null)
            return ManifestUnavailable<GetDestiny2WeaponPerksResponse>();

        var keyword = request.Keyword?.Trim();
        var weaponTypeResults = new List<Destiny2WeaponTypePerksDto>();
        var unmatchedWeaponTypes = new List<Destiny2ItemSubTypeEnum>();

        foreach (var requestedWeaponType in request.WeaponTypes.Distinct().Order())
        {
            if (!rollData.WeaponTypes.TryGetValue(requestedWeaponType, out var weaponType))
            {
                unmatchedWeaponTypes.Add(requestedWeaponType);
                continue;
            }

            var socketPerks = new Dictionary<int, Dictionary<uint, HashSet<uint>>>();
            foreach (var weaponHash in weaponType.WeaponHashes)
            {
                if (!rollData.Weapons.TryGetValue(weaponHash, out var weapon))
                    continue;

                foreach (var socket in weapon.PerkSockets)
                {
                    if (!socketPerks.TryGetValue(socket.SocketIndex, out var perkWeapons))
                    {
                        perkWeapons = [];
                        socketPerks[socket.SocketIndex] = perkWeapons;
                    }

                    foreach (var perkHash in socket.PerkHashes)
                    {
                        if (!rollData.Items.TryGetValue(perkHash, out var perk) || !MatchesKeyword(perk, keyword))
                            continue;

                        var representativeHash = rollData.PerkVariantGroups.TryGetValue(perkHash, out var variants)
                            ? variants.Min()
                            : perkHash;
                        if (!perkWeapons.TryGetValue(representativeHash, out var weaponHashes))
                        {
                            weaponHashes = [];
                            perkWeapons[representativeHash] = weaponHashes;
                        }

                        weaponHashes.Add(weaponHash);
                    }
                }
            }

            var perkSockets = socketPerks
                .OrderBy(pair => pair.Key)
                .Select(pair => new Destiny2WeaponPerkSocketDto
                {
                    SocketIndex = pair.Key,
                    Perks = pair.Value
                        .Select(perk => MapPerk(rollData, perk.Key, perk.Value))
                        .Where(perk => !string.IsNullOrWhiteSpace(perk.Name))
                        .OrderBy(perk => perk.Name, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(perk => perk.PerkHash)
                        .ToList()
                })
                .Where(socket => socket.Perks.Count > 0)
                .ToList();

            weaponTypeResults.Add(new Destiny2WeaponTypePerksDto
            {
                WeaponType = MapWeaponType(weaponType),
                PerkSockets = perkSockets
            });
        }

        weaponTypeResults = weaponTypeResults
            .OrderBy(result => result.WeaponType.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(result => result.WeaponType.WeaponType)
            .ToList();

        return new GetDestiny2WeaponPerksResponse
        {
            Data = new Destiny2WeaponPerksDto
            {
                WeaponTypes = weaponTypeResults,
                UnmatchedWeaponTypes = unmatchedWeaponTypes
            }
        };
    }

    public async Task<FilterDestiny2WeaponGodRollResponse> FilterGodRollAsync(
        FilterDestiny2WeaponGodRollRequest request,
        CancellationToken cancellationToken)
    {
        var rollData = await _dataProvider.GetRollDataAsync(
            request.Language,
            request.WeaponTypes,
            cancellationToken).ConfigureAwait(false);
        if (rollData == null)
            return ManifestUnavailable<FilterDestiny2WeaponGodRollResponse>();

        var requestedWeaponTypes = request.WeaponTypes.Count == 0
            ? rollData.WeaponTypes.Keys.ToHashSet()
            : request.WeaponTypes.Distinct().ToHashSet();
        var perkGroups = request.PerkGroups
            .Select(group => group.PerkHashes
                .Distinct()
                .SelectMany(hash => rollData.PerkVariantGroups.TryGetValue(hash, out var variants)
                    ? (IEnumerable<uint>)variants
                    : [hash])
                .ToHashSet())
            .ToList();
        var requestedPerkHashes = perkGroups.SelectMany(group => group).ToHashSet();
        var candidateWeaponHashes = requestedWeaponTypes
            .Where(rollData.WeaponTypes.ContainsKey)
            .SelectMany(weaponType => rollData.WeaponTypes[weaponType].WeaponHashes)
            .Distinct();
        var requestedAmmoTypes = request.AmmoTypes.Distinct().ToHashSet();
        var requestedDamageTypes = request.DamageTypes.Distinct().ToHashSet();
        var requestedWeaponSlots = request.WeaponSlots.Distinct().ToHashSet();
        var requestedRarities = request.Rarities.Distinct().ToHashSet();
        var requestedBreakerTypes = request.BreakerTypes.Distinct().ToHashSet();
        var requestedFoundryTraitIds = request.FoundryTraitIds
            .Select(traitId => traitId.Trim())
            .Where(traitId => !string.IsNullOrWhiteSpace(traitId))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var requestedClassTypes = request.ClassTypes.Distinct().ToHashSet();
        var requestedCurrentVersions = request.CurrentVersions.Distinct().ToHashSet();
        var requestedPowerCapHashes = request.PowerCapHashes.Distinct().ToHashSet();

        var matchedWeapons = new List<Destiny2WeaponGodRollDto>();
        foreach (var weaponHash in candidateWeaponHashes)
        {
            if (!rollData.Weapons.TryGetValue(weaponHash, out var weapon) ||
                requestedAmmoTypes.Count > 0 && !requestedAmmoTypes.Contains(weapon.AmmoType) ||
                requestedDamageTypes.Count > 0 && !requestedDamageTypes.Contains(weapon.DamageType) ||
                requestedWeaponSlots.Count > 0 && !requestedWeaponSlots.Contains(weapon.WeaponSlot) ||
                requestedRarities.Count > 0 && !requestedRarities.Contains(weapon.Rarity) ||
                requestedBreakerTypes.Count > 0 && !requestedBreakerTypes.Contains(weapon.BreakerType) ||
                request.IsCraftable.HasValue && weapon.IsCraftable != request.IsCraftable.Value ||
                !MatchesStatFilters(weapon, request.StatFilters) ||
                requestedFoundryTraitIds.Count > 0 && !weapon.Foundries.Any(
                    foundry => requestedFoundryTraitIds.Contains(foundry.TraitId)) ||
                requestedCurrentVersions.Count > 0 &&
                !requestedCurrentVersions.Contains(weapon.Version.CurrentVersion) ||
                requestedPowerCapHashes.Count > 0 &&
                !weapon.Version.PowerCapHashes.Any(requestedPowerCapHashes.Contains) ||
                request.IsAdept.HasValue && weapon.IsAdept != request.IsAdept.Value ||
                request.IsHolofoil.HasValue && weapon.IsHolofoil != request.IsHolofoil.Value ||
                requestedClassTypes.Count > 0 && !requestedClassTypes.Contains(weapon.ClassType) ||
                !CanAssignPerkGroups(weapon, perkGroups))
            {
                continue;
            }

            if (!requestedWeaponTypes.Contains(weapon.WeaponType) ||
                !rollData.WeaponTypes.TryGetValue(weapon.WeaponType, out var weaponType) ||
                !rollData.Items.TryGetValue(weaponHash, out var weaponItem))
            {
                continue;
            }

            var matchedPerkSockets = weapon.PerkSockets
                .Select(socket => new Destiny2WeaponMatchedPerkSocketDto
                {
                    SocketIndex = socket.SocketIndex,
                    Perks = socket.PerkHashes
                        .Where(requestedPerkHashes.Contains)
                        .Where(rollData.Items.ContainsKey)
                        .Select(perkHash => MapMatchedPerk(rollData.Items[perkHash]))
                        .OrderBy(perk => perk.Name, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(perk => perk.PerkHash)
                        .ToList()
                })
                .Where(socket => socket.Perks.Count > 0)
                .OrderBy(socket => socket.SocketIndex)
                .ToList();

            matchedWeapons.Add(new Destiny2WeaponGodRollDto
            {
                WeaponHash = weaponHash,
                Name = weaponItem.Name,
                Description = weaponItem.Description,
                Icon = weaponItem.Icon,
                Appearance = weaponItem.Appearance,
                WeaponType = MapWeaponTypeReference(weaponType),
                AmmoType = weapon.AmmoType,
                DamageType = weapon.DamageType,
                WeaponSlot = weapon.WeaponSlot,
                Rarity = weapon.Rarity,
                RarityName = weapon.RarityName,
                Breaker = weapon.Breaker,
                IsCraftable = weapon.IsCraftable,
                RecipeItemHash = weapon.RecipeItemHash,
                Stats = weapon.Stats,
                Foundries = weapon.Foundries,
                Version = weapon.Version,
                IsAdept = weapon.IsAdept,
                IsHolofoil = weapon.IsHolofoil,
                ClassType = weapon.ClassType,
                Frame = MapWeaponFrameReference(rollData.Items, weapon.FrameHash),
                RoundsPerMinute = weapon.RoundsPerMinute,
                Season = new Destiny2WeaponSeasonReferenceDto
                {
                    SeasonHash = weaponItem.SeasonHash,
                    SeasonNumber = weaponItem.SeasonNumber,
                    Name = weaponItem.SeasonName
                },
                MatchedPerkSockets = matchedPerkSockets
            });
        }

        matchedWeapons = matchedWeapons
            .OrderByDescending(weapon => weapon.Season.SeasonNumber)
            .ThenBy(weapon => weapon.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(weapon => weapon.WeaponHash)
            .ToList();
        var pagedWeapons = matchedWeapons.ToPaged(request.PageIndex, request.PageSize);

        return new FilterDestiny2WeaponGodRollResponse
        {
            Data = new PagedListDto<Destiny2WeaponGodRollDto>
            {
                PageIndex = pagedWeapons.PageIndex,
                PageSize = pagedWeapons.PageSize,
                Total = pagedWeapons.Total,
                TotalPages = pagedWeapons.TotalPages,
                HasPrevPage = pagedWeapons.HasPrevPage,
                HasNextPage = pagedWeapons.HasNextPage,
                Items = pagedWeapons.Items
            }
        };
    }

    public static bool CanAssignPerkGroups(
        Destiny2WeaponDataDto weapon,
        IReadOnlyList<HashSet<uint>> perkGroups)
    {
        var groupSockets = perkGroups
            .Select(group => new PerkGroupSocketOptions(
                weapon.PerkSockets
                    .Where(socket => socket.PerkHashes.Overlaps(group))
                    .Select(socket => socket.SocketIndex)
                    .Distinct()
                    .Order()
                    .ToList()))
            .ToList();

        if (groupSockets.Any(group => group.SocketIndexes.Count == 0))
            return false;

        var orderedGroups = groupSockets
            .OrderBy(group => group.SocketIndexes.Count)
            .ToList();

        return AssignSocket(orderedGroups, 0, []);
    }

    private static bool MatchesStatFilters(
        Destiny2WeaponDataDto weapon,
        IReadOnlyCollection<Destiny2WeaponStatFilterDto> statFilters)
    {
        foreach (var filter in statFilters)
        {
            var stat = weapon.Stats.FirstOrDefault(stat => stat.StatHash == filter.StatHash);
            if (stat == null ||
                filter.MinimumValue.HasValue && stat.Value < filter.MinimumValue.Value ||
                filter.MaximumValue.HasValue && stat.Value > filter.MaximumValue.Value)
            {
                return false;
            }
        }

        return true;
    }

    private static bool AssignSocket(
        IReadOnlyList<PerkGroupSocketOptions> groups,
        int groupPosition,
        HashSet<int> occupiedSockets)
    {
        if (groupPosition == groups.Count)
            return true;

        foreach (var socketIndex in groups[groupPosition].SocketIndexes)
        {
            if (!occupiedSockets.Add(socketIndex))
                continue;

            if (AssignSocket(groups, groupPosition + 1, occupiedSockets))
                return true;

            occupiedSockets.Remove(socketIndex);
        }

        return false;
    }

    private static bool MatchesKeyword(Destiny2WeaponItemDataDto perk, string? keyword)
    {
        return string.IsNullOrWhiteSpace(keyword) ||
               perk.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
               perk.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase);
    }

    private static Destiny2WeaponTypeDto MapWeaponType(Destiny2WeaponTypeDataDto weaponType)
    {
        return new Destiny2WeaponTypeDto
        {
            WeaponType = weaponType.WeaponType,
            Name = weaponType.Name,
            WeaponCount = weaponType.WeaponHashes.Count
        };
    }

    private static Destiny2WeaponPerkDto MapPerk(
        Destiny2WeaponRollDataDto rollData,
        uint perkHash,
        IEnumerable<uint> weaponHashes)
    {
        rollData.Items.TryGetValue(perkHash, out var item);
        var variantHashes = rollData.PerkVariantGroups.TryGetValue(perkHash, out var variants)
            ? variants
            : new HashSet<uint> { perkHash };
        return new Destiny2WeaponPerkDto
        {
            PerkHash = perkHash,
            Name = item?.Name ?? string.Empty,
            Icon = item?.Icon ?? string.Empty,
            ApplicableWeaponCount = weaponHashes.Distinct().Count(),
            Variants = variantHashes
                .Where(rollData.Items.ContainsKey)
                .Select(hash => new Destiny2WeaponPerkVariantDto
                {
                    PerkHash = hash,
                    Description = rollData.Items[hash].Description,
                    IsEnhanced = string.Equals(
                        rollData.Items[hash].PlugCategoryIdentifier,
                        "enhanced_traits",
                        StringComparison.OrdinalIgnoreCase)
                })
                .OrderBy(variant => variant.PerkHash)
                .ToList()
        };
    }

    private static Destiny2WeaponTypeReferenceDto MapWeaponTypeReference(Destiny2WeaponTypeDataDto weaponType)
    {
        return new Destiny2WeaponTypeReferenceDto
        {
            WeaponType = weaponType.WeaponType,
            Name = weaponType.Name
        };
    }

    private static Destiny2WeaponFrameReferenceDto MapWeaponFrameReference(
        IReadOnlyDictionary<uint, Destiny2WeaponItemDataDto> items,
        uint frameHash)
    {
        if (frameHash == 0 || !items.TryGetValue(frameHash, out var frame))
            return new Destiny2WeaponFrameReferenceDto();

        return new Destiny2WeaponFrameReferenceDto
        {
            FrameHash = frameHash,
            Name = frame.Name,
            Description = frame.Description
        };
    }

    private static Destiny2WeaponMatchedPerkDto MapMatchedPerk(Destiny2WeaponItemDataDto item)
    {
        return new Destiny2WeaponMatchedPerkDto
        {
            PerkHash = item.Hash,
            Name = item.Name,
            Description = item.Description,
            Icon = item.Icon,
            IsEnhanced = string.Equals(
                item.PlugCategoryIdentifier,
                "enhanced_traits",
                StringComparison.OrdinalIgnoreCase)
        };
    }

    private static TResponse ManifestUnavailable<TResponse>() where TResponse : Void.Bot.Message.Dto.VoidBotResponse, new()
    {
        return new TResponse
        {
            Code = HttpStatusCode.ServiceUnavailable,
            Message = "Destiny 2 weapon manifest is unavailable"
        };
    }

    private sealed record PerkGroupSocketOptions(IReadOnlyList<int> SocketIndexes);
}
