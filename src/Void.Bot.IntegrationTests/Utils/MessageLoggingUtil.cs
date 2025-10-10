using Void.Bot.Core.Data;
using Void.Bot.Core.Domain;

namespace Void.Bot.IntegrationTest.Utils;

public class MessageLoggingUtil(IntegrationTestBase testBase) : IntegrationUtilBase(testBase)
{
    public async Task AddMessageLoggingAsync(Guid id, string messageType, string messageJson, string resultType, string resultJson, DateTimeOffset createAt)
    {
        await RunWithUnitOfWork<IRepository<MessageLog>>(async rep =>
        {
            await rep.InsertAsync(new MessageLog
            {
                Id = id,
                MessageType = messageType,
                MessageJson = messageJson,
                ResultType = resultType,
                ResultJson = resultJson,
                CreatedAt = createAt
            }).ConfigureAwait(false);
        });
    }
}