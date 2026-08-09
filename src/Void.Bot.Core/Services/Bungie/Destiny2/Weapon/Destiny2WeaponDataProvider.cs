using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Serilog;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Settings.Bungie;
using Void.Bot.Message.Constants.Bungie.Destiny2;
using Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;
using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Core.Services.Bungie.Destiny2.Weapon;

public interface IDestiny2WeaponDataProvider : ISingleton
{
    Task<IReadOnlyDictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto>?> GetWeaponTypesAsync(
        string language,
        CancellationToken cancellationToken);

    Task<Destiny2WeaponRollDataDto?> GetRollDataAsync(
        string language,
        IReadOnlyCollection<Destiny2ItemSubTypeEnum>? weaponTypes,
        CancellationToken cancellationToken);

    Task RefreshAsync(string language, CancellationToken cancellationToken);
}

internal sealed class Destiny2WeaponDataProvider : IDestiny2WeaponDataProvider, IDisposable
{
    private const uint RoundsPerMinuteStatHash = 4284893193;

    private readonly ICacheManager _cacheManager;
    private readonly Destiny2Setting _setting;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _buildLock = new(1, 1);

    public Destiny2WeaponDataProvider(
        ICacheManager cacheManager,
        Destiny2Setting setting,
        ILogger logger)
    {
        _cacheManager = cacheManager;
        _setting = setting;
        _logger = logger;
    }

    public async Task<IReadOnlyDictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto>?> GetWeaponTypesAsync(
        string language,
        CancellationToken cancellationToken)
    {
        var requestedLanguage = language?.Trim() ?? string.Empty;
        var normalizedLanguage = string.IsNullOrWhiteSpace(requestedLanguage)
            ? _setting.DefaultLanguage
            : requestedLanguage.ToLowerInvariant();
        var version = await _cacheManager.Get<string>(
            Destiny2ManifestConstants.VersionCacheKey,
            cancellationToken).ConfigureAwait(false) ?? "unversioned";
        var redisKey = Destiny2WeaponCacheConstants.GetTypesKey(version, normalizedLanguage);
        var cachedWeaponTypes = await _cacheManager.Get<Dictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto>>(
            redisKey,
            cancellationToken).ConfigureAwait(false);
        if (cachedWeaponTypes != null)
        {
            _logger.Information(
                "Loaded Destiny 2 weapon types from Redis. Version: {Version}, Language: {Language}, WeaponTypeCount: {WeaponTypeCount}",
                version,
                normalizedLanguage,
                cachedWeaponTypes.Count);
            return cachedWeaponTypes;
        }

        await _buildLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cachedWeaponTypes = await _cacheManager.Get<Dictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto>>(
                redisKey,
                cancellationToken).ConfigureAwait(false);
            if (cachedWeaponTypes != null)
                return cachedWeaponTypes;

            var inventoryJson = await _cacheManager.Get<string>(
                $"{Destiny2ManifestConstants.InventoryItemCacheKey}:{normalizedLanguage}",
                cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(inventoryJson))
            {
                _logger.Warning(
                    "Destiny 2 inventory item manifest cache is unavailable. Language: {Language}",
                    normalizedLanguage);
                return null;
            }

            try
            {
                cachedWeaponTypes = BuildWeaponTypeData(inventoryJson)
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
                await _cacheManager.Set(redisKey, cachedWeaponTypes, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                _logger.Information(
                    "Built Destiny 2 weapon type data. Version: {Version}, Language: {Language}, WeaponTypeCount: {WeaponTypeCount}",
                    version,
                    normalizedLanguage,
                    cachedWeaponTypes.Count);
                return cachedWeaponTypes;
            }
            catch (JsonException ex)
            {
                _logger.Error(
                    ex,
                    "Failed to parse Destiny 2 weapon type manifest. Version: {Version}, Language: {Language}",
                    version,
                    normalizedLanguage);
                return null;
            }
        }
        finally
        {
            _buildLock.Release();
        }
    }

    public async Task<Destiny2WeaponRollDataDto?> GetRollDataAsync(
        string language,
        IReadOnlyCollection<Destiny2ItemSubTypeEnum>? weaponTypes,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestedLanguage = language?.Trim() ?? string.Empty;
        var normalizedLanguage = string.IsNullOrWhiteSpace(requestedLanguage)
            ? _setting.DefaultLanguage
            : requestedLanguage.ToLowerInvariant();
        var version = await _cacheManager.Get<string>(
            Destiny2ManifestConstants.VersionCacheKey,
            cancellationToken).ConfigureAwait(false) ?? "unversioned";
        var allWeaponTypes = await _cacheManager.Get<Dictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto>>(
            Destiny2WeaponCacheConstants.GetTypesKey(version, normalizedLanguage),
            cancellationToken).ConfigureAwait(false);

        if (allWeaponTypes == null &&
            !await BuildDerivedCacheAsync(
                normalizedLanguage,
                version,
                null,
                false,
                cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        allWeaponTypes ??= await _cacheManager.Get<Dictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto>>(
            Destiny2WeaponCacheConstants.GetTypesKey(version, normalizedLanguage),
            cancellationToken).ConfigureAwait(false);
        if (allWeaponTypes == null)
            return null;

        var requestedWeaponTypes = weaponTypes is { Count: > 0 }
            ? weaponTypes.Distinct().Where(allWeaponTypes.ContainsKey).ToList()
            : allWeaponTypes.Keys.ToList();
        if (requestedWeaponTypes.Count == 0)
        {
            return new Destiny2WeaponRollDataDto();
        }

        var perkVariantGroups = await _cacheManager.Get<Dictionary<uint, HashSet<uint>>>(
            Destiny2WeaponCacheConstants.GetPerkVariantGroupsKey(version, normalizedLanguage),
            cancellationToken).ConfigureAwait(false);

        var typeData = new Dictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeRollDataDto>();
        foreach (var weaponType in requestedWeaponTypes)
        {
            var cachedTypeData = await _cacheManager.Get<Destiny2WeaponTypeRollDataDto>(
                Destiny2WeaponCacheConstants.GetWeaponTypeKey(version, normalizedLanguage, (int)weaponType),
                cancellationToken).ConfigureAwait(false);

            if (cachedTypeData != null)
                typeData[weaponType] = cachedTypeData;
        }

        if (perkVariantGroups == null || typeData.Count != requestedWeaponTypes.Count)
        {
            _logger.Information(
                "Destiny 2 weapon roll cache is incomplete. Rebuilding from manifest. Version: {Version}, Language: {Language}, PerkVariantGroupsAvailable: {PerkVariantGroupsAvailable}, RequestedWeaponTypeCount: {RequestedWeaponTypeCount}, LoadedWeaponTypeCount: {LoadedWeaponTypeCount}",
                version,
                normalizedLanguage,
                perkVariantGroups != null,
                requestedWeaponTypes.Count,
                typeData.Count);

            perkVariantGroups = null;
            typeData.Clear();

            if (!await BuildDerivedCacheAsync(
                    normalizedLanguage,
                    version,
                    requestedWeaponTypes,
                    false,
                    cancellationToken).ConfigureAwait(false))
                return null;

            perkVariantGroups = await _cacheManager.Get<Dictionary<uint, HashSet<uint>>>(
                Destiny2WeaponCacheConstants.GetPerkVariantGroupsKey(version, normalizedLanguage),
                cancellationToken).ConfigureAwait(false);
            if (perkVariantGroups == null)
                return null;

            foreach (var weaponType in requestedWeaponTypes)
            {
                var cachedTypeData = await _cacheManager.Get<Destiny2WeaponTypeRollDataDto>(
                    Destiny2WeaponCacheConstants.GetWeaponTypeKey(version, normalizedLanguage, (int)weaponType),
                    cancellationToken).ConfigureAwait(false);
                if (cachedTypeData == null)
                    return null;

                typeData[weaponType] = cachedTypeData;
            }
        }

        stopwatch.Stop();
        _logger.Information(
            "Loaded Destiny 2 weapon roll data from Redis. Version: {Version}, Language: {Language}, WeaponTypeCount: {WeaponTypeCount}, ElapsedMilliseconds: {ElapsedMilliseconds}",
            version,
            normalizedLanguage,
            typeData.Count,
            stopwatch.ElapsedMilliseconds);

        return new Destiny2WeaponRollDataDto
        {
            Items = typeData.Values
                .SelectMany(data => data.Items)
                .GroupBy(pair => pair.Key)
                .ToDictionary(group => group.Key, group => group.First().Value),
            Weapons = typeData.Values.SelectMany(data => data.Weapons).ToDictionary(pair => pair.Key, pair => pair.Value),
            WeaponTypes = typeData.ToDictionary(pair => pair.Key, pair => pair.Value.WeaponType),
            PerkVariantGroups = perkVariantGroups
        };
    }

    public async Task RefreshAsync(string language, CancellationToken cancellationToken)
    {
        var requestedLanguage = language?.Trim() ?? string.Empty;
        var normalizedLanguage = string.IsNullOrWhiteSpace(requestedLanguage)
            ? _setting.DefaultLanguage
            : requestedLanguage.ToLowerInvariant();
        var version = await _cacheManager.Get<string>(
            Destiny2ManifestConstants.VersionCacheKey,
            cancellationToken).ConfigureAwait(false) ?? "unversioned";

        await BuildDerivedCacheAsync(normalizedLanguage, version, null, true, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<bool> BuildDerivedCacheAsync(
        string language,
        string version,
        IReadOnlyCollection<Destiny2ItemSubTypeEnum>? requiredWeaponTypes,
        bool force,
        CancellationToken cancellationToken)
    {
        await _buildLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!force && await HasDerivedCacheAsync(
                    language,
                    version,
                    requiredWeaponTypes,
                    cancellationToken).ConfigureAwait(false))
            {
                return true;
            }

            var stopwatch = Stopwatch.StartNew();
            _logger.Information(
                "Building Destiny 2 weapon cache from manifest. Version: {Version}, Language: {Language}, Force: {Force}, RequiredWeaponTypeCount: {RequiredWeaponTypeCount}",
                version,
                language,
                force,
                requiredWeaponTypes?.Count ?? 0);

            var inventoryTask = _cacheManager.Get<string>(
                $"{Destiny2ManifestConstants.InventoryItemCacheKey}:{language}",
                cancellationToken);
            var plugSetTask = _cacheManager.Get<string>(
                $"{Destiny2ManifestConstants.PlugSetCacheKey}:{language}",
                cancellationToken);
            var seasonTask = _cacheManager.Get<string>(
                $"{Destiny2ManifestConstants.SeasonCacheKey}:{language}",
                cancellationToken);
            await Task.WhenAll(inventoryTask, plugSetTask, seasonTask).ConfigureAwait(false);

            var inventoryJson = await inventoryTask.ConfigureAwait(false);
            var plugSetJson = await plugSetTask.ConfigureAwait(false);
            var seasonJson = await seasonTask.ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(inventoryJson) || string.IsNullOrWhiteSpace(plugSetJson) ||
                string.IsNullOrWhiteSpace(seasonJson))
            {
                _logger.Warning(
                    "Destiny 2 weapon manifest cache is unavailable. Language: {Language}, InventoryItemAvailable: {InventoryItemAvailable}, PlugSetAvailable: {PlugSetAvailable}, SeasonAvailable: {SeasonAvailable}",
                    language,
                    !string.IsNullOrWhiteSpace(inventoryJson),
                    !string.IsNullOrWhiteSpace(plugSetJson),
                    !string.IsNullOrWhiteSpace(seasonJson));
                return false;
            }

            var weaponTypes = BuildWeaponTypeData(inventoryJson)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            var rollData = BuildRollData(inventoryJson, plugSetJson, seasonJson);

            _logger.Information(
                "Parsed Destiny 2 weapon manifest. Version: {Version}, Language: {Language}, InventoryCharacterCount: {InventoryCharacterCount}, PlugSetCharacterCount: {PlugSetCharacterCount}, SeasonCharacterCount: {SeasonCharacterCount}, WeaponTypeCount: {WeaponTypeCount}, WeaponCount: {WeaponCount}, ItemCount: {ItemCount}, PerkVariantGroupCount: {PerkVariantGroupCount}",
                version,
                language,
                inventoryJson.Length,
                plugSetJson.Length,
                seasonJson.Length,
                weaponTypes.Count,
                rollData.Weapons.Count,
                rollData.Items.Count,
                rollData.PerkVariantGroups.Count);

            await _cacheManager.Set(
                Destiny2WeaponCacheConstants.GetTypesKey(version, language),
                weaponTypes,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            await _cacheManager.Set(
                Destiny2WeaponCacheConstants.GetPerkVariantGroupsKey(version, language),
                rollData.PerkVariantGroups,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            foreach (var weaponType in weaponTypes)
            {
                var weapons = rollData.Weapons.Values
                    .Where(weapon => weapon.WeaponType == weaponType.Key)
                    .ToDictionary(weapon => weapon.Hash);
                var perkHashes = weapons.Values
                    .SelectMany(weapon => weapon.PerkSockets)
                    .SelectMany(socket => socket.PerkHashes)
                    .ToHashSet();
                var itemHashes = weapons.Values
                    .Select(weapon => weapon.Hash)
                    .Concat(weapons.Values.Select(weapon => weapon.FrameHash).Where(hash => hash != 0))
                    .Concat(perkHashes)
                    .Concat(perkHashes.SelectMany(hash => rollData.PerkVariantGroups[hash]))
                    .ToHashSet();
                var typeData = new Destiny2WeaponTypeRollDataDto
                {
                    WeaponType = weaponType.Value,
                    Weapons = weapons,
                    Items = itemHashes
                        .Where(rollData.Items.ContainsKey)
                        .ToDictionary(hash => hash, hash => rollData.Items[hash])
                };
                await _cacheManager.Set(
                    Destiny2WeaponCacheConstants.GetWeaponTypeKey(version, language, (int)weaponType.Key),
                    typeData,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            stopwatch.Stop();
            _logger.Information(
                "Built and cached Destiny 2 weapon data. Source: Manifest, Version: {Version}, Language: {Language}, WeaponTypeCount: {WeaponTypeCount}, WeaponCount: {WeaponCount}, ElapsedMilliseconds: {ElapsedMilliseconds}",
                version,
                language,
                weaponTypes.Count,
                rollData.Weapons.Count,
                stopwatch.ElapsedMilliseconds);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.Error(
                ex,
                "Failed to build Destiny 2 weapon cache. Version: {Version}, Language: {Language}",
                version,
                language);
            return false;
        }
        finally
        {
            _buildLock.Release();
        }
    }

    private async Task<bool> HasDerivedCacheAsync(
        string language,
        string version,
        IReadOnlyCollection<Destiny2ItemSubTypeEnum>? requiredWeaponTypes,
        CancellationToken cancellationToken)
    {
        var weaponTypes = await _cacheManager.Get<Dictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto>>(
            Destiny2WeaponCacheConstants.GetTypesKey(version, language),
            cancellationToken).ConfigureAwait(false);
        if (weaponTypes == null)
            return false;

        if (await _cacheManager.Get<Dictionary<uint, HashSet<uint>>>(
                Destiny2WeaponCacheConstants.GetPerkVariantGroupsKey(version, language),
                cancellationToken).ConfigureAwait(false) == null)
            return false;

        foreach (var weaponType in requiredWeaponTypes ?? weaponTypes.Keys)
        {
            if (await _cacheManager.Get<Destiny2WeaponTypeRollDataDto>(
                    Destiny2WeaponCacheConstants.GetWeaponTypeKey(version, language, (int)weaponType),
                    cancellationToken).ConfigureAwait(false) == null)
                return false;
        }

        return true;
    }

    public void Dispose()
    {
        _buildLock.Dispose();
    }

    internal static IReadOnlyDictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto> BuildWeaponTypeData(
        string inventoryItemJson)
    {
        using var inventoryDocument = JsonDocument.Parse(inventoryItemJson);
        if (inventoryDocument.RootElement.ValueKind != JsonValueKind.Object)
            throw new JsonException("DestinyInventoryItemDefinition must be a JSON object");

        var weaponTypes = new Dictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto>();

        foreach (var property in inventoryDocument.RootElement.EnumerateObject())
        {
            var weaponDefinition = property.Value;
            if (weaponDefinition.ReadInt32("itemType") != (int)Destiny2ItemTypeEnum.Weapon ||
                weaponDefinition.ReadBoolean("redacted") ||
                weaponDefinition.ReadBoolean("blacklisted"))
            {
                continue;
            }

            var weaponHash = weaponDefinition.ReadHash(property.Name);
            var weaponType = (Destiny2ItemSubTypeEnum)weaponDefinition.ReadInt32("itemSubType");
            var weaponTypeName = weaponDefinition.ReadString("itemTypeDisplayName");
            if (weaponHash == 0 || weaponType == Destiny2ItemSubTypeEnum.None ||
                string.IsNullOrWhiteSpace(weaponTypeName))
            {
                continue;
            }

            if (!weaponTypes.TryGetValue(weaponType, out var weaponTypeData))
            {
                weaponTypeData = new Destiny2WeaponTypeDataDto
                {
                    WeaponType = weaponType,
                    Name = weaponTypeName,
                    WeaponHashes = new HashSet<uint>()
                };
                weaponTypes[weaponType] = weaponTypeData;
            }

            weaponTypeData.WeaponHashes.Add(weaponHash);
        }

        return weaponTypes;
    }

    internal static Destiny2WeaponRollDataDto BuildRollData(
        string inventoryItemJson,
        string plugSetJson,
        string seasonJson)
    {
        var plugSets = ParsePlugSets(plugSetJson);
        var seasons = ParseSeasons(seasonJson);
        using var inventoryDocument = JsonDocument.Parse(inventoryItemJson);
        if (inventoryDocument.RootElement.ValueKind != JsonValueKind.Object)
            throw new JsonException("DestinyInventoryItemDefinition must be a JSON object");

        var weaponCandidates = ParseWeaponCandidates(inventoryDocument.RootElement, plugSets);
        var referencedItemHashes = new HashSet<uint>();
        foreach (var candidate in weaponCandidates)
        {
            referencedItemHashes.Add(candidate.Hash);
            if (candidate.FrameHash != 0)
                referencedItemHashes.Add(candidate.FrameHash);
            foreach (var perkSocket in candidate.PerkSockets)
                referencedItemHashes.UnionWith(perkSocket.PerkHashes);
        }

        var items = BuildItems(inventoryDocument.RootElement, referencedItemHashes, seasons);
        var weapons = BuildWeapons(weaponCandidates, items);
        var weaponTypes = BuildWeaponTypes(weapons);
        var perkVariantGroups = new Dictionary<uint, HashSet<uint>>();
        var perkHashes = weapons.Values
            .SelectMany(weapon => weapon.PerkSockets)
            .SelectMany(socket => socket.PerkHashes)
            .Distinct();
        foreach (var group in perkHashes.GroupBy(
                     hash => $"{items[hash].Name.Trim().Normalize()}\0{items[hash].Icon}",
                     StringComparer.OrdinalIgnoreCase))
        {
            var variants = group.ToHashSet();
            foreach (var hash in variants)
                perkVariantGroups[hash] = variants;
        }

        return new Destiny2WeaponRollDataDto
        {
            Items = items,
            Weapons = weapons,
            WeaponTypes = weaponTypes,
            PerkVariantGroups = perkVariantGroups
        };
    }

    private static List<WeaponCandidate> ParseWeaponCandidates(
        JsonElement inventoryRoot,
        IReadOnlyDictionary<uint, HashSet<uint>> plugSets)
    {
        var candidates = new List<WeaponCandidate>();

        foreach (var property in inventoryRoot.EnumerateObject())
        {
            var definition = property.Value;
            if (definition.ReadInt32("itemType") != (int)Destiny2ItemTypeEnum.Weapon ||
                definition.ReadBoolean("redacted") ||
                definition.ReadBoolean("blacklisted"))
            {
                continue;
            }

            var weaponHash = definition.ReadHash(property.Name);
            var weaponType = (Destiny2ItemSubTypeEnum)definition.ReadInt32("itemSubType");
            var weaponTypeName = definition.ReadString("itemTypeDisplayName");
            if (weaponHash == 0 || weaponType == Destiny2ItemSubTypeEnum.None ||
                string.IsNullOrWhiteSpace(weaponTypeName) ||
                !definition.TryGetProperty("sockets", out var sockets))
                continue;

            var perkSockets = ParsePerkSockets(sockets, plugSets);
            if (perkSockets.Count == 0)
                continue;

            candidates.Add(new WeaponCandidate(
                weaponHash,
                weaponType,
                weaponTypeName,
                ParseFrameHash(sockets),
                ReadStatValue(definition, RoundsPerMinuteStatHash),
                perkSockets));
        }

        return candidates;
    }

    private static Dictionary<uint, Destiny2WeaponDataDto> BuildWeapons(
        IEnumerable<WeaponCandidate> candidates,
        IReadOnlyDictionary<uint, Destiny2WeaponItemDataDto> items)
    {
        var weapons = new Dictionary<uint, Destiny2WeaponDataDto>();

        foreach (var candidate in candidates)
        {
            if (!items.TryGetValue(candidate.Hash, out var weaponItem) || string.IsNullOrWhiteSpace(weaponItem.Name))
                continue;

            var perkSockets = candidate.PerkSockets
                .Select(socket => new Destiny2WeaponPerkSocketDataDto
                {
                    SocketIndex = socket.SocketIndex,
                    PerkHashes = socket.PerkHashes
                        .Where(hash => items.TryGetValue(hash, out var item) && !string.IsNullOrWhiteSpace(item.Name))
                        .ToHashSet()
                })
                .Where(socket => socket.PerkHashes.Count > 0)
                .OrderBy(socket => socket.SocketIndex)
                .ToList();

            if (perkSockets.Count == 0)
                continue;

            weapons[candidate.Hash] = new Destiny2WeaponDataDto
            {
                Hash = candidate.Hash,
                WeaponType = candidate.WeaponType,
                WeaponTypeName = candidate.WeaponTypeName,
                FrameHash = candidate.FrameHash,
                RoundsPerMinute = candidate.RoundsPerMinute,
                PerkSockets = perkSockets
            };
        }

        return weapons;
    }

    private static Dictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto> BuildWeaponTypes(
        IReadOnlyDictionary<uint, Destiny2WeaponDataDto> weapons)
    {
        var weaponTypes = new Dictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto>();
        foreach (var weapon in weapons.Values)
        {
            if (!weaponTypes.TryGetValue(weapon.WeaponType, out var weaponType))
            {
                weaponType = new Destiny2WeaponTypeDataDto
                {
                    WeaponType = weapon.WeaponType,
                    Name = weapon.WeaponTypeName,
                    WeaponHashes = new HashSet<uint>()
                };
                weaponTypes[weapon.WeaponType] = weaponType;
            }

            weaponType.WeaponHashes.Add(weapon.Hash);
        }

        return weaponTypes;
    }

    private static Dictionary<uint, HashSet<uint>> ParsePlugSets(string plugSetJson)
    {
        using var document = JsonDocument.Parse(plugSetJson);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new JsonException("DestinyPlugSetDefinition must be a JSON object");

        var result = new Dictionary<uint, HashSet<uint>>();

        foreach (var property in document.RootElement.EnumerateObject())
        {
            var definition = property.Value;
            var plugSetHash = definition.ReadHash(property.Name);
            if (plugSetHash == 0 || !definition.TryGetProperty("reusablePlugItems", out var plugItems) ||
                plugItems.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var hashes = new HashSet<uint>();
            foreach (var plugItem in plugItems.EnumerateArray())
            {
                if (plugItem.TryGetProperty("currentlyCanRoll", out var currentlyCanRoll) &&
                    currentlyCanRoll.ValueKind == JsonValueKind.False)
                {
                    continue;
                }

                var plugItemHash = plugItem.ReadUInt32("plugItemHash");
                if (plugItemHash != 0)
                    hashes.Add(plugItemHash);
            }

            if (hashes.Count > 0)
                result[plugSetHash] = hashes;
        }

        return result;
    }

    private static Dictionary<uint, (int Number, string Name)> ParseSeasons(string seasonJson)
    {
        using var document = JsonDocument.Parse(seasonJson);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new JsonException("DestinySeasonDefinition must be a JSON object");

        var result = new Dictionary<uint, (int Number, string Name)>();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            var definition = property.Value;
            var hash = definition.ReadHash(property.Name);
            if (hash == 0)
                continue;

            var displayProperties = definition.TryGetProperty("displayProperties", out var display)
                ? display
                : default;
            result[hash] = (definition.ReadInt32("seasonNumber"), displayProperties.ReadString("name"));
        }

        return result;
    }

    private static List<PerkSocketCandidate> ParsePerkSockets(
        JsonElement sockets,
        IReadOnlyDictionary<uint, HashSet<uint>> plugSets)
    {
        if (!sockets.TryGetProperty("socketEntries", out var socketEntries) ||
            socketEntries.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var result = new List<PerkSocketCandidate>();
        var socketIndex = 0;

        foreach (var socketEntry in socketEntries.EnumerateArray())
        {
            var randomizedPlugSetHash = socketEntry.ReadUInt32("randomizedPlugSetHash");
            if (randomizedPlugSetHash != 0 && plugSets.TryGetValue(randomizedPlugSetHash, out var perkHashes) &&
                perkHashes.Count > 0)
            {
                result.Add(new PerkSocketCandidate(socketIndex, perkHashes));
            }

            socketIndex++;
        }

        return result;
    }

    private static uint ParseFrameHash(JsonElement sockets)
    {
        if (sockets.TryGetProperty("socketEntries", out var socketEntries) &&
            socketEntries.ValueKind == JsonValueKind.Array)
        {
            foreach (var socketEntry in socketEntries.EnumerateArray())
            {
                var frameHash = socketEntry.ReadUInt32("singleInitialItemHash");
                if (frameHash != 0)
                    return frameHash;
                break;
            }
        }

        if (sockets.TryGetProperty("intrinsicSockets", out var intrinsicSockets) &&
            intrinsicSockets.ValueKind == JsonValueKind.Array)
        {
            foreach (var intrinsicSocket in intrinsicSockets.EnumerateArray())
                return intrinsicSocket.ReadUInt32("plugItemHash");
        }

        return 0;
    }

    private static int ReadStatValue(JsonElement definition, uint statHash)
    {
        if (!definition.TryGetProperty("stats", out var statBlock) ||
            !statBlock.TryGetProperty("stats", out var stats) ||
            stats.ValueKind != JsonValueKind.Object ||
            !stats.TryGetProperty(statHash.ToString(CultureInfo.InvariantCulture), out var stat))
        {
            return 0;
        }

        return stat.ReadInt32("value");
    }

    private static Dictionary<uint, Destiny2WeaponItemDataDto> BuildItems(
        JsonElement inventoryRoot,
        IReadOnlySet<uint> referencedItemHashes,
        IReadOnlyDictionary<uint, (int Number, string Name)> seasons)
    {
        var result = new Dictionary<uint, Destiny2WeaponItemDataDto>();

        foreach (var property in inventoryRoot.EnumerateObject())
        {
            var definition = property.Value;
            var hash = definition.ReadHash(property.Name);
            if (hash == 0 || !referencedItemHashes.Contains(hash) ||
                definition.ReadBoolean("redacted") || definition.ReadBoolean("blacklisted"))
            {
                continue;
            }

            var displayProperties = definition.TryGetProperty("displayProperties", out var display)
                ? display
                : default;
            var plugCategoryIdentifier = definition.TryGetProperty("plug", out var plug)
                ? plug.ReadString("plugCategoryIdentifier")
                : string.Empty;
            var seasonHash = definition.ReadUInt32("seasonHash");
            seasons.TryGetValue(seasonHash, out var season);

            result[hash] = new Destiny2WeaponItemDataDto
            {
                Hash = hash,
                Name = displayProperties.ReadString("name"),
                Description = displayProperties.ReadString("description"),
                Icon = displayProperties.ReadString("icon"),
                SeasonHash = seasonHash,
                SeasonNumber = season.Number,
                SeasonName = season.Name ?? string.Empty,
                PlugCategoryIdentifier = plugCategoryIdentifier
            };
        }

        return result;
    }

    private sealed record WeaponCandidate(
        uint Hash,
        Destiny2ItemSubTypeEnum WeaponType,
        string WeaponTypeName,
        uint FrameHash,
        int RoundsPerMinute,
        List<PerkSocketCandidate> PerkSockets);

    private sealed record PerkSocketCandidate(int SocketIndex, IReadOnlySet<uint> PerkHashes);
}
