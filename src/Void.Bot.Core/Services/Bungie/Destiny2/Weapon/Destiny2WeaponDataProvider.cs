using System.Collections.Concurrent;
using System.Diagnostics;
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
    private readonly ICacheManager _cacheManager;
    private readonly Destiny2Setting _setting;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _buildLock = new(1, 1);
    private readonly ConcurrentDictionary<string, Dictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto>> _weaponTypeDataCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, Dictionary<uint, HashSet<uint>>> _perkVariantGroupCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, Destiny2WeaponTypeRollDataDto> _weaponTypeRollDataCache = new(StringComparer.OrdinalIgnoreCase);

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
        var cacheKey = $"{version}:{normalizedLanguage}";

        if (_weaponTypeDataCache.TryGetValue(cacheKey, out var cachedWeaponTypes))
            return cachedWeaponTypes;

        var redisKey = Destiny2WeaponCacheConstants.GetTypesKey(version, normalizedLanguage);
        cachedWeaponTypes = await _cacheManager.Get<Dictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto>>(
            redisKey,
            cancellationToken).ConfigureAwait(false);
        if (cachedWeaponTypes != null)
        {
            _weaponTypeDataCache[cacheKey] = cachedWeaponTypes;
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
            if (_weaponTypeDataCache.TryGetValue(cacheKey, out cachedWeaponTypes))
                return cachedWeaponTypes;

            cachedWeaponTypes = await _cacheManager.Get<Dictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto>>(
                redisKey,
                cancellationToken).ConfigureAwait(false);
            if (cachedWeaponTypes != null)
            {
                _weaponTypeDataCache[cacheKey] = cachedWeaponTypes;
                return cachedWeaponTypes;
            }

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
                _weaponTypeDataCache[cacheKey] = cachedWeaponTypes;

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
        var cacheKey = $"{version}:{normalizedLanguage}";
        if (!_weaponTypeDataCache.TryGetValue(cacheKey, out var allWeaponTypes))
        {
            allWeaponTypes = await _cacheManager.Get<Dictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto>>(
                Destiny2WeaponCacheConstants.GetTypesKey(version, normalizedLanguage),
                cancellationToken).ConfigureAwait(false);
            if (allWeaponTypes != null)
                _weaponTypeDataCache[cacheKey] = allWeaponTypes;
        }

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

        allWeaponTypes ??= _weaponTypeDataCache[cacheKey];
        if (allWeaponTypes == null)
            return null;

        var requestedWeaponTypes = weaponTypes is { Count: > 0 }
            ? weaponTypes.Distinct().Where(allWeaponTypes.ContainsKey).ToList()
            : allWeaponTypes.Keys.ToList();
        if (requestedWeaponTypes.Count == 0)
        {
            return new Destiny2WeaponRollDataDto();
        }

        var redisLoaded = false;
        if (!_perkVariantGroupCache.TryGetValue(cacheKey, out var perkVariantGroups))
        {
            perkVariantGroups = await _cacheManager.Get<Dictionary<uint, HashSet<uint>>>(
                Destiny2WeaponCacheConstants.GetPerkVariantGroupsKey(version, normalizedLanguage),
                cancellationToken).ConfigureAwait(false);
            if (perkVariantGroups != null)
            {
                _perkVariantGroupCache[cacheKey] = perkVariantGroups;
                redisLoaded = true;
            }
        }

        var typeData = new Dictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeRollDataDto>();
        foreach (var weaponType in requestedWeaponTypes)
        {
            var typeCacheKey = $"{cacheKey}:{(int)weaponType}";
            if (!_weaponTypeRollDataCache.TryGetValue(typeCacheKey, out var cachedTypeData))
            {
                cachedTypeData = await _cacheManager.Get<Destiny2WeaponTypeRollDataDto>(
                    Destiny2WeaponCacheConstants.GetWeaponTypeKey(version, normalizedLanguage, (int)weaponType),
                    cancellationToken).ConfigureAwait(false);
                if (cachedTypeData != null)
                {
                    _weaponTypeRollDataCache[typeCacheKey] = cachedTypeData;
                    redisLoaded = true;
                }
            }

            if (cachedTypeData != null)
                typeData[weaponType] = cachedTypeData;
        }

        if (perkVariantGroups == null || typeData.Count != requestedWeaponTypes.Count)
        {
            if (!await BuildDerivedCacheAsync(
                    normalizedLanguage,
                    version,
                    requestedWeaponTypes,
                    false,
                    cancellationToken).ConfigureAwait(false))
                return null;

            perkVariantGroups = _perkVariantGroupCache[cacheKey];
            typeData = requestedWeaponTypes.ToDictionary(
                weaponType => weaponType,
                weaponType => _weaponTypeRollDataCache[$"{cacheKey}:{(int)weaponType}"]);
        }

        if (redisLoaded)
        {
            stopwatch.Stop();
            _logger.Information(
                "Loaded Destiny 2 weapon roll data. Source: Redis, Version: {Version}, Language: {Language}, WeaponTypeCount: {WeaponTypeCount}, ElapsedMilliseconds: {ElapsedMilliseconds}",
                version,
                normalizedLanguage,
                typeData.Count,
                stopwatch.ElapsedMilliseconds);
        }
        else
        {
            stopwatch.Stop();
            _logger.Debug(
                "Loaded Destiny 2 weapon roll data. Source: Memory, Version: {Version}, Language: {Language}, WeaponTypeCount: {WeaponTypeCount}, ElapsedMilliseconds: {ElapsedMilliseconds}",
                version,
                normalizedLanguage,
                typeData.Count,
                stopwatch.ElapsedMilliseconds);
        }

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
        var cacheKey = $"{version}:{language}";
        await _buildLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!force &&
                _weaponTypeDataCache.TryGetValue(cacheKey, out var cachedWeaponTypes) &&
                _perkVariantGroupCache.ContainsKey(cacheKey) &&
                (requiredWeaponTypes ?? cachedWeaponTypes.Keys).All(type =>
                    _weaponTypeRollDataCache.ContainsKey($"{cacheKey}:{(int)type}")))
            {
                return true;
            }

            var stopwatch = Stopwatch.StartNew();
            var inventoryTask = _cacheManager.Get<string>(
                $"{Destiny2ManifestConstants.InventoryItemCacheKey}:{language}",
                cancellationToken);
            var plugSetTask = _cacheManager.Get<string>(
                $"{Destiny2ManifestConstants.PlugSetCacheKey}:{language}",
                cancellationToken);
            await Task.WhenAll(inventoryTask, plugSetTask).ConfigureAwait(false);

            var inventoryJson = await inventoryTask.ConfigureAwait(false);
            var plugSetJson = await plugSetTask.ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(inventoryJson) || string.IsNullOrWhiteSpace(plugSetJson))
            {
                _logger.Warning(
                    "Destiny 2 weapon manifest cache is unavailable. Language: {Language}, InventoryItemAvailable: {InventoryItemAvailable}, PlugSetAvailable: {PlugSetAvailable}",
                    language,
                    !string.IsNullOrWhiteSpace(inventoryJson),
                    !string.IsNullOrWhiteSpace(plugSetJson));
                return false;
            }

            var weaponTypes = BuildWeaponTypeData(inventoryJson)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            var rollData = BuildRollData(inventoryJson, plugSetJson);
            var typeData = new Dictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeRollDataDto>();
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
                    .Concat(perkHashes)
                    .Concat(perkHashes.SelectMany(hash => rollData.PerkVariantGroups[hash]))
                    .ToHashSet();
                typeData[weaponType.Key] = new Destiny2WeaponTypeRollDataDto
                {
                    WeaponType = weaponType.Value,
                    Weapons = weapons,
                    Items = itemHashes
                        .Where(rollData.Items.ContainsKey)
                        .ToDictionary(hash => hash, hash => rollData.Items[hash])
                };
            }

            var writes = new List<Task>
            {
                _cacheManager.Set(
                    Destiny2WeaponCacheConstants.GetTypesKey(version, language),
                    weaponTypes,
                    cancellationToken: cancellationToken),
                _cacheManager.Set(
                    Destiny2WeaponCacheConstants.GetPerkVariantGroupsKey(version, language),
                    rollData.PerkVariantGroups,
                    cancellationToken: cancellationToken)
            };
            writes.AddRange(typeData.Select(pair => _cacheManager.Set(
                Destiny2WeaponCacheConstants.GetWeaponTypeKey(version, language, (int)pair.Key),
                pair.Value,
                cancellationToken: cancellationToken)));
            await Task.WhenAll(writes).ConfigureAwait(false);

            foreach (var existingKey in _weaponTypeRollDataCache.Keys.Where(key =>
                         key.StartsWith($"{cacheKey}:", StringComparison.OrdinalIgnoreCase)))
            {
                _weaponTypeRollDataCache.TryRemove(existingKey, out _);
            }

            _weaponTypeDataCache[cacheKey] = weaponTypes;
            _perkVariantGroupCache[cacheKey] = rollData.PerkVariantGroups;
            foreach (var pair in typeData)
                _weaponTypeRollDataCache[$"{cacheKey}:{(int)pair.Key}"] = pair.Value;

            stopwatch.Stop();
            _logger.Information(
                "Built and cached Destiny 2 weapon data. Source: Manifest, Version: {Version}, Language: {Language}, WeaponTypeCount: {WeaponTypeCount}, WeaponCount: {WeaponCount}, ElapsedMilliseconds: {ElapsedMilliseconds}",
                version,
                language,
                typeData.Count,
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
        string plugSetJson)
    {
        var plugSets = ParsePlugSets(plugSetJson);
        using var inventoryDocument = JsonDocument.Parse(inventoryItemJson);
        if (inventoryDocument.RootElement.ValueKind != JsonValueKind.Object)
            throw new JsonException("DestinyInventoryItemDefinition must be a JSON object");

        var weaponCandidates = ParseWeaponCandidates(inventoryDocument.RootElement, plugSets);
        var referencedItemHashes = new HashSet<uint>();
        foreach (var candidate in weaponCandidates)
        {
            referencedItemHashes.Add(candidate.Hash);
            foreach (var perkSocket in candidate.PerkSockets)
                referencedItemHashes.UnionWith(perkSocket.PerkHashes);
        }

        var items = BuildItems(inventoryDocument.RootElement, referencedItemHashes);
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

            candidates.Add(new WeaponCandidate(weaponHash, weaponType, weaponTypeName, perkSockets));
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

    private static Dictionary<uint, Destiny2WeaponItemDataDto> BuildItems(
        JsonElement inventoryRoot,
        IReadOnlySet<uint> referencedItemHashes)
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

            result[hash] = new Destiny2WeaponItemDataDto
            {
                Hash = hash,
                Name = displayProperties.ReadString("name"),
                Description = displayProperties.ReadString("description"),
                Icon = displayProperties.ReadString("icon"),
                PlugCategoryIdentifier = plugCategoryIdentifier
            };
        }

        return result;
    }

    private sealed record WeaponCandidate(
        uint Hash,
        Destiny2ItemSubTypeEnum WeaponType,
        string WeaponTypeName,
        List<PerkSocketCandidate> PerkSockets);

    private sealed record PerkSocketCandidate(int SocketIndex, IReadOnlySet<uint> PerkHashes);
}
