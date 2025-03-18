namespace Void.Bot.Core.Services.Identity;

public interface ICurrentUser : IScope
{
    Guid? Id { get; }
    
    string UserName { get; }
}