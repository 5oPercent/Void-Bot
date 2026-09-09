using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Services.Identity;

public interface ICurrentUser
{
    Guid? Id { get; }
    
    string UserName { get; }

    bool IsTwoFactorAuthenticated { get; }

    LanguageEnum Language { get; }
}

public class CurrentUserProxy(ICurrentUserProvider provider) : ICurrentUser
{
    public Guid? Id => provider.CurrentUser?.Id;

    public string UserName => provider.CurrentUser?.UserName ?? string.Empty;

    public bool IsTwoFactorAuthenticated => provider.CurrentUser?.IsTwoFactorAuthenticated ?? false;

    public LanguageEnum Language => provider.CurrentUser?.Language ?? LanguageEnum.Default;
}
