using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Account;
using Void.Bot.Core.Extension;

namespace Void.Bot.IntegrationTest.Utils;

public class AccountUtil(IntegrationTestBase testBase) : IntegrationUtilBase(testBase)
{
    public async Task<Guid> AddUserAccountAsync(string name = "", string suffix = "")
    {
        if (name.IsNullOrEmpty())
            name = "".GenerateRandomString();
        
        if (suffix.IsNullOrEmpty())
            suffix = "".GenerateNumberSuffix();
        
        var id = Guid.NewGuid();
        
        await RunWithUnitOfWork<IRepository<UserAccount>>(async rep =>
        {
            await rep.InsertAsync(new UserAccount
            {
                Id = id,
                Name = name,
                Suffix = suffix,
                Password = "pwd"
            }).ConfigureAwait(false);
        });
        
        return id;
    }
}