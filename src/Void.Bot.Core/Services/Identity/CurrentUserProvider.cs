using Serilog;

namespace Void.Bot.Core.Services.Identity;

public interface ICurrentUserProvider : IScope
{
    ICurrentUser? CurrentUser { get; }
    
    void SetCurrentUser(ICurrentUser currentUser);
}

public class CurrentUserProvider : ICurrentUserProvider
{
    public ICurrentUser? CurrentUser { get; private set; }

    public void SetCurrentUser(ICurrentUser currentUser)
    {
        CurrentUser = currentUser;
        Log.Error("=== Set Type {Type} Current User {UserId} {UserName}", currentUser, CurrentUser?.Id, CurrentUser?.UserName ?? "Unknown");
    }
}