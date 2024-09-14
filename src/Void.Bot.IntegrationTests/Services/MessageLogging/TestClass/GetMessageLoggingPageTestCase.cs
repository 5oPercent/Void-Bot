using System.Collections;

namespace Void.Bot.IntegrationTest.Services.MessageLogging.TestClass;

public class GetMessageLoggingPageTestCase
{
    public int PageIndex { get; set; }
    
    public int PageSize { get; set; }
    
    public int SuccessesCount { get; set; }
}

public class GetMessageLoggingDataPageProvider : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new GetMessageLoggingPageTestCase
            {
                PageIndex = 1,
                PageSize = 3,
                SuccessesCount = 3
            }
        };
        
        yield return new object[]
        {
            new GetMessageLoggingPageTestCase
            {
                PageIndex = 1,
                PageSize = 0,
                SuccessesCount = 0
            }
        };
        
        yield return new object[]
        {
            new GetMessageLoggingPageTestCase
            {
                PageIndex = 2,
                PageSize = 3,
                SuccessesCount = 1
            }
        };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}