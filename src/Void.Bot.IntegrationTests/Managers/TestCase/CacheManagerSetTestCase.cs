using System.Collections;

namespace Void.Bot.IntegrationTest.Managers.TestCase;

public class CacheManagerSetTestCase
{
    public string Key { get; set; }
    public Type Type { get; set; }
    public object Data { get; set; }
}

public class CacheManagerSetDataProvider : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new CacheManagerSetTestCase
            {
                Key = "String",
                Type = typeof(string),
                Data = "Test"
            }
        };
        
        yield return new object[]
        {
            new CacheManagerSetTestCase
            {
                Key = "List",
                Type = typeof(List<>),
                Data = new List<string>
                {
                    "123", "456", "789"
                }
            }
        };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}