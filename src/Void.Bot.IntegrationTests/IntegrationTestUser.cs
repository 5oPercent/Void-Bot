using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Enum;

namespace Void.Bot.IntegrationTest;

public class IntegrationTestUser  : ICurrentUser
{
    public Guid? Id => Guid.Parse("33333333-3333-3333-3333-333333333333");
    
    public string UserName => "IntegrationTestUser";

    public bool IsTwoFactorAuthenticated => false;

    public LanguageEnum Language => LanguageEnum.Default;
}
