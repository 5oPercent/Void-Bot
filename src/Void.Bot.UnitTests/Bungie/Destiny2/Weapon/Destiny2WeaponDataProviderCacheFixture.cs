using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Serilog;
using Shouldly;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Services.Bungie.Destiny2.Weapon;
using Void.Bot.Core.Settings.Bungie;
using Void.Bot.Message.Constants.Bungie.Destiny2;
using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.UnitTest.Bungie.Destiny2.Weapon;

public class Destiny2WeaponDataProviderCacheFixture
{
    private const string Version = "test-version";

    [Fact]
    public async Task RefreshShouldPersistDerivedDataAndRestoreItAfterRestart()
    {
        var cache = new SerializableCacheManager();
        await cache.Set(Destiny2ManifestConstants.VersionCacheKey, Version);
        await cache.Set(
            $"{Destiny2ManifestConstants.InventoryItemCacheKey}:en",
            Destiny2WeaponServiceFixture.InventoryItemJson);
        await cache.Set(
            $"{Destiny2ManifestConstants.PlugSetCacheKey}:en",
            Destiny2WeaponServiceFixture.PlugSetJson);
        await cache.Set(
            $"{Destiny2ManifestConstants.SeasonCacheKey}:en",
            Destiny2WeaponServiceFixture.SeasonJson);

        using (var provider = CreateProvider(cache))
            await provider.RefreshAsync("en", CancellationToken.None);

        cache.Contains(Destiny2WeaponCacheConstants.GetTypesKey(Version, "en")).ShouldBeTrue();
        cache.Contains(Destiny2WeaponCacheConstants.GetPerkVariantGroupsKey(Version, "en")).ShouldBeTrue();
        cache.Contains(Destiny2WeaponCacheConstants.GetWeaponTypeKey(
            Version,
            "en",
            (int)Destiny2ItemSubTypeEnum.AutoRifle)).ShouldBeTrue();

        await cache.Remove($"{Destiny2ManifestConstants.InventoryItemCacheKey}:en");
        await cache.Remove($"{Destiny2ManifestConstants.PlugSetCacheKey}:en");
        await cache.Remove($"{Destiny2ManifestConstants.SeasonCacheKey}:en");
        cache.ReadKeys.Clear();

        using var restartedProvider = CreateProvider(cache);
        var rollData = await restartedProvider.GetRollDataAsync(
            "en",
            [Destiny2ItemSubTypeEnum.AutoRifle],
            CancellationToken.None);

        rollData.ShouldNotBeNull();
        rollData.Items[100].SeasonHash.ShouldBe(900u);
        rollData.Items[100].SeasonNumber.ShouldBe(23);
        rollData.WeaponTypes.Keys.ShouldBe([Destiny2ItemSubTypeEnum.AutoRifle]);
        rollData.Weapons.Keys.ShouldBe([100u]);
        rollData.PerkVariantGroups[20].Order().ShouldBe([20u, 24u]);
        cache.ReadKeys.ShouldNotContain($"{Destiny2ManifestConstants.InventoryItemCacheKey}:en");
        cache.ReadKeys.ShouldNotContain($"{Destiny2ManifestConstants.PlugSetCacheKey}:en");
        cache.ReadKeys.ShouldNotContain($"{Destiny2ManifestConstants.SeasonCacheKey}:en");

        cache.ReadKeys.Clear();
        await restartedProvider.GetRollDataAsync(
            "en",
            [Destiny2ItemSubTypeEnum.AutoRifle],
            CancellationToken.None);
        cache.ReadKeys.ShouldBe([
            Destiny2ManifestConstants.VersionCacheKey,
            Destiny2WeaponCacheConstants.GetTypesKey(Version, "en"),
            Destiny2WeaponCacheConstants.GetPerkVariantGroupsKey(Version, "en"),
            Destiny2WeaponCacheConstants.GetWeaponTypeKey(
                Version,
                "en",
                (int)Destiny2ItemSubTypeEnum.AutoRifle)
        ]);
    }

    private static Destiny2WeaponDataProvider CreateProvider(ICacheManager cacheManager)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Destiny2:SyncLanguages:0"] = "en"
            })
            .Build();
        return new Destiny2WeaponDataProvider(
            cacheManager,
            new Destiny2Setting(configuration),
            new LoggerConfiguration().CreateLogger());
    }

    private sealed class SerializableCacheManager : ICacheManager
    {
        private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);

        public List<string> ReadKeys { get; } = [];

        public bool Contains(string key) => _values.ContainsKey(key);

        public Task<T?> Get<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            ReadKeys.Add(key);
            if (!_values.TryGetValue(key, out var value))
                return Task.FromResult<T?>(null);

            if (typeof(T) == typeof(string))
                return Task.FromResult((T?)(object)value);

            return Task.FromResult(JsonSerializer.Deserialize<T>(value));
        }

        public async Task<T?> Take<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            var value = await Get<T>(key, cancellationToken);
            _values.Remove(key);
            return value;
        }

        public Task Set(
            string key,
            object data,
            TimeSpan? expiry = null,
            CancellationToken cancellationToken = default)
        {
            _values[key] = data is string value ? value : JsonSerializer.Serialize(data);
            return Task.CompletedTask;
        }

        public Task Remove(string key, CancellationToken cancellationToken = default)
        {
            _values.Remove(key);
            return Task.CompletedTask;
        }

        public Task RemoveByPrefix(string pattern, CancellationToken cancellationToken = default)
        {
            foreach (var key in _values.Keys.Where(key => key.StartsWith(pattern)).ToList())
                _values.Remove(key);
            return Task.CompletedTask;
        }

        public Task<bool> TryLockAsync(
            string key,
            TimeSpan expiry,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task Clear(CancellationToken cancellationToken = default)
        {
            _values.Clear();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}
