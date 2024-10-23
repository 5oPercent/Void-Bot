using Shouldly;
using Void.Bot.Core.Caching;
using Void.Bot.IntegrationTest.Managers.TestCase;
using Void.Bot.IntegrationTest.TestBaseClasses;

namespace Void.Bot.IntegrationTest.Managers;

public class CacheManagerTest : ManagerFixtureBase
{
    [Theory]
    [ClassData(typeof(CacheManagerSetDataProvider))]
    public async Task ShouldSet(CacheManagerSetTestCase testCase)
    {
        await Run<ICacheManager>(async cache =>
        {
            await cache.Set(testCase.Key, testCase.Data);

            if (testCase.Type == typeof(string))
            {
                var value = await cache.Get<string>(testCase.Key);
                value.ShouldBeOfType(testCase.Type);
                value.ShouldBe(testCase.Data);
            }
            else if (testCase.Type == typeof(List<string>))
            {
                var value = await cache.Get<List<string>>(testCase.Key);
                value.ShouldBeOfType(testCase.Type);
                value.ShouldBe(testCase.Data);
            }
        });
    }
    
    [Fact]
    public async Task ShouldRemove()
    {
        await Run<ICacheManager>(async cache =>
        {
            await cache.Set("Test1", "TA");
            await cache.Remove("Test1");
            
            var value1 = await cache.Get<object>("Test1");
            
            value1.ShouldBeNull();
        });
    }
    
    [Fact]
    public async Task ShouldRemoveByPattern()
    {
        await Run<ICacheManager>(async cache =>
        {
            await cache.Set("Test1", "TA");
            await cache.Set("Test2", "TB");
            await cache.Set("Test3", "TC");
            await cache.Set("ABCDF", "TD");
            
            await cache.RemoveByPrefix("Test*");
            
            await Task.Delay(100);
            
            var value1 = await cache.Get<string>("Test1");
            var value2 = await cache.Get<string>("Test2");
            var value3 = await cache.Get<string>("Test3");
            var value6 = await cache.Get<string>("ABCDF");
            
            value1.ShouldBeNull();
            value2.ShouldBeNull();
            value3.ShouldBeNull();
            value6.ShouldBe("TD");
        });
    }
    
    [Fact]
    public async Task ShouldClear()
    {
        await Run<ICacheManager>(async cache =>
        {
            await cache.Set("Test1", "TA");
            await cache.Set("Test2", "TB");
            await cache.Set("Test3", "TC");
            await cache.Clear();
            
            await Task.Delay(100);
            
            var value1 = await cache.Get<object>("Test1");
            var value2 = await cache.Get<object>("Test2");
            var value3 = await cache.Get<object>("Test3");
            
            value1.ShouldBeNull();
            value2.ShouldBeNull();
            value3.ShouldBeNull();
        });
    }
}