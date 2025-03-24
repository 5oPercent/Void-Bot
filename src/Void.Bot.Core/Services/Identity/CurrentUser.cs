namespace Void.Bot.Core.Services.Identity;

public interface ICurrentUser : IScope
{
    Guid? Id { get; }
    
    string UserName { get; }
}

public class CurrentUserProxy(ICurrentUserProvider provider) : ICurrentUser
{
    public Guid? Id => provider.CurrentUser?.Id;

    public string UserName => provider.CurrentUser?.UserName ?? string.Empty;
}