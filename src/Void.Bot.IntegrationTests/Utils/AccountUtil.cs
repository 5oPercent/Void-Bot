using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Account;

namespace Void.Bot.IntegrationTest.Utils;

public class AccountUtil(IntegrationTestBase testBase) : IntegrationUtilBase(testBase)
{
    public async Task AddUserAccountAsync(string name, string suffix)
    {
        await RunWithUnitOfWork<IRepository<UserAccount>>(async rep =>
        {
            await rep.InsertAsync(new UserAccount
            {
                Name = name,
                Suffix = suffix,
                Password = "pwd"
            }).ConfigureAwait(false);
        });
    }
}