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

    Task<bool> RefreshAsync(string language, CancellationToken cancellationToken);
}

internal sealed class Destiny2WeaponDataProvider : IDestiny2WeaponDataProvider, IDisposable
{
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

    public async Task<bool> RefreshAsync(string language, CancellationToken cancellationToken)
    {
        var requestedLanguage = language?.Trim() ?? string.Empty;
        var normalizedLanguage = string.IsNullOrWhiteSpace(requestedLanguage)
            ? _setting.DefaultLanguage
            : requestedLanguage.ToLowerInvariant();
        var version = await _cacheManager.Get<string>(
            Destiny2ManifestConstants.VersionCacheKey,
            cancellationToken).ConfigureAwait(false) ?? "unversioned";

        return await BuildDerivedCacheAsync(normalizedLanguage, version, null, true, cancellationToken)
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
            var statTask = _cacheManager.Get<string>(
                $"{Destiny2ManifestConstants.StatCacheKey}:{language}",
                cancellationToken);
            var traitTask = _cacheManager.Get<string>(
                $"{Destiny2ManifestConstants.TraitCacheKey}:{language}",
                cancellationToken);
            var breakerTypeTask = _cacheManager.Get<string>(
                $"{Destiny2ManifestConstants.BreakerTypeCacheKey}:{language}",
                cancellationToken);
            var sandboxPerkTask = _cacheManager.Get<string>(
                $"{Destiny2ManifestConstants.SandboxPerkCacheKey}:{language}",
                cancellationToken);
            await Task.WhenAll(
                    inventoryTask,
                    plugSetTask,
                    seasonTask,
                    statTask,
                    traitTask,
                    breakerTypeTask,
                    sandboxPerkTask)
                .ConfigureAwait(false);

            var inventoryJson = await inventoryTask.ConfigureAwait(false);
            var plugSetJson = await plugSetTask.ConfigureAwait(false);
            var seasonJson = await seasonTask.ConfigureAwait(false);
            var statJson = await statTask.ConfigureAwait(false);
            var traitJson = await traitTask.ConfigureAwait(false);
            var breakerTypeJson = await breakerTypeTask.ConfigureAwait(false);
            var sandboxPerkJson = await sandboxPerkTask.ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(inventoryJson) || string.IsNullOrWhiteSpace(plugSetJson) ||
                string.IsNullOrWhiteSpace(seasonJson) || string.IsNullOrWhiteSpace(statJson) ||
                string.IsNullOrWhiteSpace(traitJson) || string.IsNullOrWhiteSpace(breakerTypeJson) ||
                string.IsNullOrWhiteSpace(sandboxPerkJson))
            {
                _logger.Warning(
                    "Destiny 2 weapon manifest cache is unavailable. Language: {Language}, InventoryItemAvailable: {InventoryItemAvailable}, PlugSetAvailable: {PlugSetAvailable}, SeasonAvailable: {SeasonAvailable}, StatAvailable: {StatAvailable}, TraitAvailable: {TraitAvailable}, BreakerTypeAvailable: {BreakerTypeAvailable}, SandboxPerkAvailable: {SandboxPerkAvailable}",
                    language,
                    !string.IsNullOrWhiteSpace(inventoryJson),
                    !string.IsNullOrWhiteSpace(plugSetJson),
                    !string.IsNullOrWhiteSpace(seasonJson),
                    !string.IsNullOrWhiteSpace(statJson),
                    !string.IsNullOrWhiteSpace(traitJson),
                    !string.IsNullOrWhiteSpace(breakerTypeJson),
                    !string.IsNullOrWhiteSpace(sandboxPerkJson));
                return false;
            }

            var weaponTypes = BuildWeaponTypeData(inventoryJson)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            var rollData = BuildRollData(
                inventoryJson,
                plugSetJson,
                seasonJson,
                statJson,
                traitJson,
                breakerTypeJson,
                sandboxPerkJson);

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
        string seasonJson,
        string statJson = "{}",
        string traitJson = "{}",
        string breakerTypeJson = "{}",
        string sandboxPerkJson = "{}")
    {
        var plugSets = ParsePlugSets(plugSetJson);
        var seasons = ParseSeasons(seasonJson);
        var seasonsByNumber = BuildSeasonsByNumber(seasons);
        var statDefinitions = ParseDefinitionReferences(statJson);
        var traitDefinitions = ParseDefinitionReferences(traitJson);
        var breakerTypeDefinitions = ParseBreakerTypeReferences(breakerTypeJson);
        var sandboxPerkBreakerTypes = ParseSandboxPerkBreakerTypes(sandboxPerkJson);
        using var inventoryDocument = JsonDocument.Parse(inventoryItemJson);
        if (inventoryDocument.RootElement.ValueKind != JsonValueKind.Object)
            throw new JsonException("DestinyInventoryItemDefinition must be a JSON object");

        var weaponCandidates = ParseWeaponCandidates(
            inventoryDocument.RootElement,
            plugSets,
            statDefinitions,
            traitDefinitions,
            breakerTypeDefinitions,
            sandboxPerkBreakerTypes);
        var referencedItemHashes = new HashSet<uint>();
        foreach (var candidate in weaponCandidates)
        {
            referencedItemHashes.Add(candidate.Hash);
            if (candidate.FrameHash != 0)
                referencedItemHashes.Add(candidate.FrameHash);
            foreach (var perkSocket in candidate.PerkSockets)
                referencedItemHashes.UnionWith(perkSocket.PerkHashes);
        }

        var items = BuildItems(inventoryDocument.RootElement, referencedItemHashes, seasons, seasonsByNumber);
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
        IReadOnlyDictionary<uint, HashSet<uint>> plugSets,
        IReadOnlyDictionary<uint, DefinitionReference> statDefinitions,
        IReadOnlyDictionary<uint, DefinitionReference> traitDefinitions,
        IReadOnlyDictionary<uint, BreakerTypeReference> breakerTypeDefinitions,
        IReadOnlyDictionary<uint, Destiny2BreakerTypeEnum> sandboxPerkBreakerTypes)
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
                string.IsNullOrWhiteSpace(weaponTypeName))
                continue;

            // 固定词条异域武器可能没有随机词条插槽，但仍需进入完整武器面板和属性筛选缓存。
            var sockets = definition.TryGetProperty("sockets", out var socketBlock)
                ? socketBlock
                : default;
            var perkSockets = ParsePerkSockets(sockets, plugSets);

            var equippingBlock = definition.TryGetProperty("equippingBlock", out var block)
                ? block
                : default;
            var inventory = definition.TryGetProperty("inventory", out var inventoryBlock)
                ? inventoryBlock
                : default;
            var quality = definition.TryGetProperty("quality", out var qualityBlock)
                ? qualityBlock
                : default;
            var recipeItemHash = inventory.ReadUInt32("recipeItemHash");
            var frameHash = ParseFrameHash(sockets);
            var breaker = ResolveBreakerType(
                definition,
                frameHash,
                (Destiny2TierTypeEnum)inventory.ReadInt32("tierType"),
                inventoryRoot,
                breakerTypeDefinitions,
                sandboxPerkBreakerTypes);

            candidates.Add(new WeaponCandidate(
                weaponHash,
                weaponType,
                weaponTypeName,
                (Destiny2AmmunitionTypeEnum)equippingBlock.ReadInt32("ammoType"),
                (Destiny2DamageTypeEnum)definition.ReadInt32("defaultDamageType"),
                MapWeaponSlot(equippingBlock.ReadUInt32("equipmentSlotTypeHash")),
                (Destiny2TierTypeEnum)inventory.ReadInt32("tierType"),
                inventory.ReadString("tierTypeName"),
                breaker.Type,
                breaker,
                recipeItemHash != 0,
                recipeItemHash,
                ParseWeaponStats(definition, statDefinitions),
                ParseFoundries(definition, traitDefinitions),
                ParseVersion(quality),
                definition.ReadBoolean("isAdept"),
                definition.ReadBoolean("isHolofoil"),
                (Destiny2ClassEnum)definition.ReadInt32("classType"),
                frameHash,
                ReadStatValue(definition, Destiny2WeaponConstants.RoundsPerMinuteStatHash),
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

            weapons[candidate.Hash] = new Destiny2WeaponDataDto
            {
                Hash = candidate.Hash,
                WeaponType = candidate.WeaponType,
                WeaponTypeName = candidate.WeaponTypeName,
                AmmoType = candidate.AmmoType,
                DamageType = candidate.DamageType,
                WeaponSlot = candidate.WeaponSlot,
                Rarity = candidate.Rarity,
                RarityName = candidate.RarityName,
                BreakerType = candidate.BreakerType,
                Breaker = candidate.Breaker,
                IsCraftable = candidate.IsCraftable,
                RecipeItemHash = candidate.RecipeItemHash,
                Stats = candidate.Stats,
                Foundries = candidate.Foundries,
                Version = candidate.Version,
                IsAdept = candidate.IsAdept,
                IsHolofoil = candidate.IsHolofoil,
                ClassType = candidate.ClassType,
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

    private static Destiny2WeaponSlotEnum MapWeaponSlot(uint equipmentSlotTypeHash)
    {
        return equipmentSlotTypeHash switch
        {
            Destiny2WeaponConstants.KineticWeaponSlotHash => Destiny2WeaponSlotEnum.Kinetic,
            Destiny2WeaponConstants.EnergyWeaponSlotHash => Destiny2WeaponSlotEnum.Energy,
            Destiny2WeaponConstants.PowerWeaponSlotHash => Destiny2WeaponSlotEnum.Power,
            _ => Destiny2WeaponSlotEnum.None
        };
    }

    /// <summary>
    /// 异域武器使用自身的反勇士标识；非异域武器未声明时，读取武器框架引用的沙盒特性。
    /// </summary>
    private static Destiny2WeaponBreakerTypeDto ResolveBreakerType(
        JsonElement definition,
        uint frameHash,
        Destiny2TierTypeEnum rarity,
        JsonElement inventoryRoot,
        IReadOnlyDictionary<uint, BreakerTypeReference> breakerTypeDefinitions,
        IReadOnlyDictionary<uint, Destiny2BreakerTypeEnum> sandboxPerkBreakerTypes)
    {
        var declaredBreaker = MapDeclaredBreakerType(definition, breakerTypeDefinitions);
        if (declaredBreaker.Type != Destiny2BreakerTypeEnum.None ||
            rarity == Destiny2TierTypeEnum.Exotic ||
            frameHash == 0 ||
            !inventoryRoot.TryGetProperty(frameHash.ToString(CultureInfo.InvariantCulture), out var frameDefinition) ||
            !frameDefinition.TryGetProperty("perks", out var framePerks) ||
            framePerks.ValueKind != JsonValueKind.Array)
        {
            return declaredBreaker;
        }

        foreach (var framePerk in framePerks.EnumerateArray())
        {
            var perkHash = framePerk.ReadUInt32("perkHash");
            if (!sandboxPerkBreakerTypes.TryGetValue(perkHash, out var type))
                continue;

            var reference = breakerTypeDefinitions.Values.FirstOrDefault(item => item.Type == type);
            return new Destiny2WeaponBreakerTypeDto
            {
                Type = type,
                Hash = reference?.Hash ?? 0,
                Name = reference?.Name ?? string.Empty,
                Description = reference?.Description ?? string.Empty,
                Icon = reference?.Icon ?? string.Empty
            };
        }

        return declaredBreaker;
    }

    private static Destiny2WeaponBreakerTypeDto MapDeclaredBreakerType(
        JsonElement definition,
        IReadOnlyDictionary<uint, BreakerTypeReference> breakerTypeDefinitions)
    {
        var type = (Destiny2BreakerTypeEnum)definition.ReadInt32("breakerType");
        var hash = definition.ReadUInt32("breakerTypeHash");
        BreakerTypeReference? reference = null;
        if (hash != 0)
            breakerTypeDefinitions.TryGetValue(hash, out reference);
        if (reference == null && type != Destiny2BreakerTypeEnum.None)
            reference = breakerTypeDefinitions.Values.FirstOrDefault(item => item.Type == type);

        return new Destiny2WeaponBreakerTypeDto
        {
            Type = type,
            Hash = reference?.Hash ?? hash,
            Name = reference?.Name ?? string.Empty,
            Description = reference?.Description ?? string.Empty,
            Icon = reference?.Icon ?? string.Empty
        };
    }

    /// <summary>
    /// 从沙盒特性名称中的 Bungie 图标字形识别反勇士类型。
    /// </summary>
    private static Dictionary<uint, Destiny2BreakerTypeEnum> ParseSandboxPerkBreakerTypes(
        string sandboxPerkJson)
    {
        using var document = JsonDocument.Parse(sandboxPerkJson);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new JsonException("Destiny sandbox perk manifest must be a JSON object");

        var result = new Dictionary<uint, Destiny2BreakerTypeEnum>();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            var definition = property.Value;
            var hash = definition.ReadHash(property.Name);
            if (hash == 0 || definition.ReadBoolean("redacted") || definition.ReadBoolean("blacklisted"))
                continue;

            var displayProperties = definition.TryGetProperty("displayProperties", out var display)
                ? display
                : default;
            var name = displayProperties.ReadString("name");
            var type = name switch
            {
                var value when value.Contains(Destiny2WeaponConstants.ShieldPiercingBreakerGlyph) =>
                    Destiny2BreakerTypeEnum.ShieldPiercing,
                var value when value.Contains(Destiny2WeaponConstants.DisruptionBreakerGlyph) =>
                    Destiny2BreakerTypeEnum.Disruption,
                var value when value.Contains(Destiny2WeaponConstants.StaggerBreakerGlyph) =>
                    Destiny2BreakerTypeEnum.Stagger,
                _ => Destiny2BreakerTypeEnum.None
            };
            if (type != Destiny2BreakerTypeEnum.None)
                result[hash] = type;
        }

        return result;
    }

    private static Dictionary<uint, BreakerTypeReference> ParseBreakerTypeReferences(string definitionJson)
    {
        using var document = JsonDocument.Parse(definitionJson);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new JsonException("Destiny breaker type manifest must be a JSON object");

        var result = new Dictionary<uint, BreakerTypeReference>();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            var definition = property.Value;
            var hash = definition.ReadHash(property.Name);
            var type = (Destiny2BreakerTypeEnum)definition.ReadInt32("enumValue");
            if (hash == 0 || type == Destiny2BreakerTypeEnum.None)
                continue;

            var displayProperties = definition.TryGetProperty("displayProperties", out var display)
                ? display
                : default;
            result[hash] = new BreakerTypeReference(
                hash,
                type,
                displayProperties.ReadString("name"),
                displayProperties.ReadString("description"),
                displayProperties.ReadString("icon"));
        }

        return result;
    }

    private static Dictionary<uint, DefinitionReference> ParseDefinitionReferences(string definitionJson)
    {
        using var document = JsonDocument.Parse(definitionJson);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new JsonException("Destiny definition manifest must be a JSON object");

        var result = new Dictionary<uint, DefinitionReference>();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            var hash = property.Value.ReadHash(property.Name);
            if (hash == 0)
                continue;

            var displayProperties = property.Value.TryGetProperty("displayProperties", out var display)
                ? display
                : default;
            result[hash] = new DefinitionReference(
                displayProperties.ReadString("name"),
                displayProperties.ReadString("description"),
                displayProperties.ReadString("icon"));
        }

        return result;
    }

    private static List<Destiny2WeaponStatDto> ParseWeaponStats(
        JsonElement definition,
        IReadOnlyDictionary<uint, DefinitionReference> statDefinitions)
    {
        if (!definition.TryGetProperty("stats", out var statBlock) ||
            !statBlock.TryGetProperty("stats", out var stats) ||
            stats.ValueKind != JsonValueKind.Object)
        {
            return [];
        }

        var result = new List<Destiny2WeaponStatDto>();
        foreach (var property in stats.EnumerateObject())
        {
            var statHash = property.Value.ReadUInt32("statHash");
            if (statHash == 0 && !uint.TryParse(property.Name, out statHash))
                continue;

            statDefinitions.TryGetValue(statHash, out var statDefinition);
            result.Add(new Destiny2WeaponStatDto
            {
                StatHash = statHash,
                Name = statDefinition?.Name ?? string.Empty,
                Description = statDefinition?.Description ?? string.Empty,
                Icon = statDefinition?.Icon ?? string.Empty,
                Value = property.Value.ReadInt32("value"),
                Minimum = property.Value.ReadInt32("minimum"),
                Maximum = property.Value.ReadInt32("maximum"),
                DisplayMaximum = property.Value.ReadInt32("displayMaximum")
            });
        }

        return result
            .OrderBy(stat => stat.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(stat => stat.StatHash)
            .ToList();
    }

    private static List<Destiny2WeaponFoundryDto> ParseFoundries(
        JsonElement definition,
        IReadOnlyDictionary<uint, DefinitionReference> traitDefinitions)
    {
        if (!definition.TryGetProperty("traitIds", out var traitIds) ||
            traitIds.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var traitHashes = definition.TryGetProperty("traitHashes", out var hashes) &&
                          hashes.ValueKind == JsonValueKind.Array
            ? hashes
            : default;
        var result = new List<Destiny2WeaponFoundryDto>();
        for (var index = 0; index < traitIds.GetArrayLength(); index++)
        {
            var traitIdElement = traitIds[index];
            if (traitIdElement.ValueKind != JsonValueKind.String)
                continue;

            var traitId = traitIdElement.GetString() ?? string.Empty;
            if (!traitId.Contains("foundry", StringComparison.OrdinalIgnoreCase))
                continue;

            var traitHash = traitHashes.ValueKind == JsonValueKind.Array && index < traitHashes.GetArrayLength()
                ? ReadUInt32Value(traitHashes[index])
                : 0;
            traitDefinitions.TryGetValue(traitHash, out var traitDefinition);
            result.Add(new Destiny2WeaponFoundryDto
            {
                TraitHash = traitHash,
                TraitId = traitId,
                Name = traitDefinition?.Name ?? string.Empty,
                Description = traitDefinition?.Description ?? string.Empty,
                Icon = traitDefinition?.Icon ?? string.Empty
            });
        }

        return result
            .OrderBy(foundry => foundry.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(foundry => foundry.TraitId, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static Destiny2WeaponVersionDto ParseVersion(JsonElement quality)
    {
        var version = new Destiny2WeaponVersionDto
        {
            CurrentVersion = quality.ReadUInt32("currentVersion"),
            InfusionCategoryHash = quality.ReadUInt32("infusionCategoryHash"),
            InfusionCategoryName = quality.ReadString("infusionCategoryName")
        };

        if (quality.TryGetProperty("versions", out var versions) && versions.ValueKind == JsonValueKind.Array)
        {
            version.PowerCapHashes = versions.EnumerateArray()
                .Select(itemVersion => itemVersion.ReadUInt32("powerCapHash"))
                .Where(hash => hash != 0)
                .ToList();
        }

        if (quality.TryGetProperty("displayVersionWatermarkIcons", out var watermarkIcons) &&
            watermarkIcons.ValueKind == JsonValueKind.Array)
        {
            version.DisplayWatermarkIcons = watermarkIcons.EnumerateArray()
                .Where(icon => icon.ValueKind == JsonValueKind.String)
                .Select(icon => icon.GetString() ?? string.Empty)
                .Where(icon => !string.IsNullOrWhiteSpace(icon))
                .ToList();
        }

        return version;
    }

    private static uint ReadUInt32Value(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Number && element.TryGetUInt32(out var value))
            return value;

        return element.ValueKind == JsonValueKind.String && uint.TryParse(element.GetString(), out value)
            ? value
            : 0;
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
        if (sockets.ValueKind != JsonValueKind.Object ||
            !sockets.TryGetProperty("socketEntries", out var socketEntries) ||
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
        if (sockets.ValueKind != JsonValueKind.Object)
            return 0;

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
        IReadOnlyDictionary<uint, (int Number, string Name)> seasons,
        IReadOnlyDictionary<int, (uint Hash, string Name)> seasonsByNumber)
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
            var season = ResolveWeaponSeason(definition, seasons, seasonsByNumber);
            var backgroundColor = definition.TryGetProperty("backgroundColor", out var color)
                ? color
                : default;

            result[hash] = new Destiny2WeaponItemDataDto
            {
                Hash = hash,
                Name = displayProperties.ReadString("name"),
                Description = displayProperties.ReadString("description"),
                Icon = displayProperties.ReadString("icon"),
                Appearance = new Destiny2WeaponAppearanceDto
                {
                    Screenshot = definition.ReadString("screenshot"),
                    IconWatermark = definition.ReadString("iconWatermark"),
                    IconWatermarkShelved = definition.ReadString("iconWatermarkShelved"),
                    SecondaryIcon = definition.ReadString("secondaryIcon"),
                    SecondaryOverlay = definition.ReadString("secondaryOverlay"),
                    SecondarySpecial = definition.ReadString("secondarySpecial"),
                    BackgroundColor = new Destiny2WeaponColorDto
                    {
                        Red = backgroundColor.ReadInt32("red"),
                        Green = backgroundColor.ReadInt32("green"),
                        Blue = backgroundColor.ReadInt32("blue"),
                        Alpha = backgroundColor.ReadInt32("alpha")
                    }
                },
                SeasonHash = season.Hash,
                SeasonNumber = season.Number,
                SeasonName = season.Name,
                PlugCategoryIdentifier = plugCategoryIdentifier
            };
        }

        return result;
    }

    /// <summary>
    /// 解析物品的赛季信息。
    /// </summary>
    /// <remarks>
    /// 优先使用清单中的 seasonHash 字段 (仅赛季任务道具等物品会填充);
    /// 武器因 Bungie 不填充 seasonHash, 回退到 traitIds 里的内容版本标记
    /// (releases.vXXX.*) 按映射表推断赛季。
    /// </remarks>
    private static (uint Hash, int Number, string Name) ResolveWeaponSeason(
        JsonElement definition,
        IReadOnlyDictionary<uint, (int Number, string Name)> seasons,
        IReadOnlyDictionary<int, (uint Hash, string Name)> seasonsByNumber)
    {
        var seasonHash = definition.ReadUInt32("seasonHash");
        if (seasonHash != 0 && seasons.TryGetValue(seasonHash, out var season))
            return (seasonHash, season.Number, season.Name ?? string.Empty);

        if (TryResolveReleaseTraitSeason(definition, seasonsByNumber, out var releaseSeason))
            return releaseSeason;

        return (0, 0, string.Empty);
    }

    /// <summary>
    /// 从物品 traitIds 中的 releases.vXXX.* 标记推断赛季 (武器专用回退路径)。
    /// </summary>
    private static bool TryResolveReleaseTraitSeason(
        JsonElement definition,
        IReadOnlyDictionary<int, (uint Hash, string Name)> seasonsByNumber,
        out (uint Hash, int Number, string Name) season)
    {
        season = (0, 0, string.Empty);
        if (!definition.TryGetProperty("traitIds", out var traitIds) ||
            traitIds.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var trait in traitIds.EnumerateArray())
        {
            if (trait.ValueKind != JsonValueKind.String ||
                !Destiny2WeaponReleaseSeasonMap.TryResolveSeason(trait.GetString(), out var seasonNumber))
            {
                continue;
            }

            if (seasonsByNumber.TryGetValue(seasonNumber, out var seasonByNumber))
            {
                season = (seasonByNumber.Hash, seasonNumber, seasonByNumber.Name);
                return true;
            }

            season = (0u, seasonNumber, string.Empty);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 按赛季号索引赛季定义, 供内容版本标记回退路径反查赛季名称与哈希。
    /// </summary>
    private static Dictionary<int, (uint Hash, string Name)> BuildSeasonsByNumber(
        IReadOnlyDictionary<uint, (int Number, string Name)> seasons)
    {
        var result = new Dictionary<int, (uint Hash, string Name)>();
        foreach (var pair in seasons)
            result[pair.Value.Number] = (pair.Key, pair.Value.Name ?? string.Empty);
        return result;
    }

    private sealed record WeaponCandidate(
        uint Hash,
        Destiny2ItemSubTypeEnum WeaponType,
        string WeaponTypeName,
        Destiny2AmmunitionTypeEnum AmmoType,
        Destiny2DamageTypeEnum DamageType,
        Destiny2WeaponSlotEnum WeaponSlot,
        Destiny2TierTypeEnum Rarity,
        string RarityName,
        Destiny2BreakerTypeEnum BreakerType,
        Destiny2WeaponBreakerTypeDto Breaker,
        bool IsCraftable,
        uint RecipeItemHash,
        List<Destiny2WeaponStatDto> Stats,
        List<Destiny2WeaponFoundryDto> Foundries,
        Destiny2WeaponVersionDto Version,
        bool IsAdept,
        bool IsHolofoil,
        Destiny2ClassEnum ClassType,
        uint FrameHash,
        int RoundsPerMinute,
        List<PerkSocketCandidate> PerkSockets);

    private sealed record PerkSocketCandidate(int SocketIndex, IReadOnlySet<uint> PerkHashes);

    private sealed record DefinitionReference(string Name, string Description, string Icon);

    private sealed record BreakerTypeReference(
        uint Hash,
        Destiny2BreakerTypeEnum Type,
        string Name,
        string Description,
        string Icon);
}
