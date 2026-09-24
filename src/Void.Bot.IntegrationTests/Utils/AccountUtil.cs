using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Account;
using Void.Bot.Core.Extension;

namespace Void.Bot.IntegrationTest.Utils;

public class AccountUtil(IntegrationTestBase testBase) : IntegrationUtilBase(testBase)
{
    public async Task<Guid> AddUserAccountAsync(
        string name = "", string suffix = "", string? email = null,
        DateTimeOffset? emailVerifiedAt = null, DateTimeOffset? twoFactorEnabledAt = null)
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
                Password = "pwd",
                Email = email,
                EmailVerifiedAt = emailVerifiedAt,
                TwoFactorEnabledAt = twoFactorEnabledAt
            }).ConfigureAwait(false);
        });

        return id;
    }

    public async Task<List<Guid>> AddUserAccountsAsync(int count, string prefix = "")
    {
        var ids = new List<Guid>();

        for (var i = 0; i < count; i++)
        {
            var id = Guid.NewGuid();
            var name = prefix.IsNullOrEmpty() ? "".GenerateRandomString() : $"{prefix}{i}";
            var suffix = "".GenerateNumberSuffix();

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

            ids.Add(id);
        }

        return ids;
    }

    public async Task DeleteUserAccountAsync(Guid id)
    {
        await RunWithUnitOfWork<IRepository<UserAccount>>(async rep =>
        {
            await rep.DeleteAsync(x => x.Id == id).ConfigureAwait(false);
        });
    }
}
