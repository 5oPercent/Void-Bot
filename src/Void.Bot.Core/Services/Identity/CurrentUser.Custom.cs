using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Services.Identity;

public class CustomCurrentUser : ICurrentUser
{
    public Guid? Id { get; }

    public string UserName { get; } = string.Empty;

    public bool IsTwoFactorAuthenticated { get; }

    public LanguageEnum Language { get; } = LanguageEnum.Default;

    public CustomCurrentUser()
    {
    }

    public CustomCurrentUser(
        Guid? id,
        string userName,
        LanguageEnum language = LanguageEnum.Default,
        bool isTwoFactorAuthenticated = false)
    {
        Id = id;
        UserName = userName;
        Language = language;
        IsTwoFactorAuthenticated = isTwoFactorAuthenticated;
    }
}
