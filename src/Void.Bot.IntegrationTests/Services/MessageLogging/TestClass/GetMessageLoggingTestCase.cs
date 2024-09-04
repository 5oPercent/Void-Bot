using System.Collections;

namespace Void.Bot.IntegrationTest.Services.MessageLogging.TestClass;

public class GetMessageLoggingTestCase
{
    public List<Guid> Ids { get; set; }
    
    public DateTimeOffset? StartDate { get; set; }
    
    public DateTimeOffset? EndDate { get; set; }
    
    public int SuccessesCount { get; set; }
}

public class GetMessageLoggingDataProvider : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new GetMessageLoggingTestCase
            {
                Ids = new List<Guid>
                {
                    Guid.Parse("ffee8b77-40d8-4dfd-8a7a-4b4cf936ab35")
                },
                StartDate = DateTimeOffset.Parse("2024-08-01"),
                EndDate = DateTimeOffset.Parse("2024-09-01"),
                SuccessesCount = 1
            }
        };
        
        yield return new object[]
        {
            new GetMessageLoggingTestCase
            {
                Ids = new List<Guid>(),
                StartDate = DateTimeOffset.Parse("2024-08-01"),
                EndDate = DateTimeOffset.Parse("2024-09-01"),
                SuccessesCount = 3
            }
        };
        
        yield return new object[]
        {
            new GetMessageLoggingTestCase
            {
                Ids = new List<Guid>(),
                StartDate = DateTimeOffset.Parse("2023-01-01"),
                EndDate = DateTimeOffset.Parse("2023-03-01"),
                SuccessesCount = 0
            }
        };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}