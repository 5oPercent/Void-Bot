using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Enum.Account;

namespace Void.Bot.IntegrationTest;

public class IntegrationTestUser  : ICurrentUser
{
    public Guid? Id => Guid.Parse("33333333-3333-3333-3333-333333333333");
    
    public string UserName => "IntegrationTestUser";
    
    public AccountLevelEnum Level { get; set; } = AccountLevelEnum.User;
    
    public AccountSourceEnum Source { get; set; } = AccountSourceEnum.Local;
}