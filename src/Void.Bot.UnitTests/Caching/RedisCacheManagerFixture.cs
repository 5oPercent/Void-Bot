using NSubstitute;
using StackExchange.Redis;
using Void.Bot.Core.Caching;

namespace Void.Bot.UnitTest.Caching;

public class RedisCacheManagerFixture
{
    private readonly IDatabase _database = Substitute.For<IDatabase>();
    private readonly RedisCacheManager _cacheManager;

    public RedisCacheManagerFixture()
    {
        var connection = Substitute.For<IConnectionMultiplexer>();
        connection.GetDatabase(0).Returns(_database);
        _cacheManager = new RedisCacheManager(connection, 0);
    }

    [Fact]
    public async Task SetShouldStoreStringsWithoutJsonSerializingThemAgain()
    {
        const string json = "{\"item\":\"value\"}";

        await _cacheManager.Set("manifest", json);

        await _database.Received(1).StringSetAsync(
            "manifest",
            json,
            null,
            false,
            When.Always,
            CommandFlags.None);
    }

    [Theory]
    [InlineData("{\"item\":\"value\"}", "{\"item\":\"value\"}")]
    [InlineData("\"legacy value\"", "legacy value")]
    public async Task GetStringShouldReadRawAndLegacyJsonSerializedValues(
        string cachedValue,
        string expected)
    {
        _database.StringGetAsync("manifest").Returns(cachedValue);

        var result = await _cacheManager.Get<string>("manifest");

        Assert.Equal(expected, result);
    }
}
