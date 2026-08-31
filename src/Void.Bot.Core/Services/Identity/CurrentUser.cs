using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Services.Identity;

public interface ICurrentUser
{
    Guid? Id { get; }
    
    string UserName { get; }

    LanguageEnum Language { get; }
}

public class CurrentUserProxy(ICurrentUserProvider provider) : ICurrentUser
{
    public Guid? Id => provider.CurrentUser?.Id;

    public string UserName => provider.CurrentUser?.UserName ?? string.Empty;

    public LanguageEnum Language => provider.CurrentUser?.Language ?? LanguageEnum.Default;
}
