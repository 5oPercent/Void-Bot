using System.Net;
using Void.Bot.Core.Services;
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
}

internal sealed class Destiny2WeaponService : IDestiny2WeaponService
{
    private readonly IDestiny2WeaponDataProvider _dataProvider;

    public Destiny2WeaponService(IDestiny2WeaponDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
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

        var matchedWeapons = new List<Destiny2WeaponGodRollDto>();
        foreach (var weaponHash in candidateWeaponHashes)
        {
            if (!rollData.Weapons.TryGetValue(weaponHash, out var weapon) ||
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
                WeaponType = MapWeaponTypeReference(weaponType),
                MatchedPerkSockets = matchedPerkSockets
            });
        }

        matchedWeapons = matchedWeapons
            .OrderBy(weapon => weapon.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(weapon => weapon.WeaponHash)
            .ToList();

        return new FilterDestiny2WeaponGodRollResponse
        {
            Data = new Destiny2WeaponGodRollListDto
            {
                Weapons = matchedWeapons
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

    private static Destiny2WeaponMatchedPerkDto MapMatchedPerk(Destiny2WeaponItemDataDto item)
    {
        return new Destiny2WeaponMatchedPerkDto
        {
            PerkHash = item.Hash,
            Name = item.Name,
            Description = item.Description,
            Icon = item.Icon
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
