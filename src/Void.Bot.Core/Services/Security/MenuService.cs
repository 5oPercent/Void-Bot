namespace Void.Bot.Core.Services.Security;

public interface IMenuService : IScope
{
    
}

public class MenuService : IMenuService
{
    private readonly MenuDataProvider _menuDataProvider;

    public MenuService(MenuDataProvider menuDataProvider)
    {
        _menuDataProvider = menuDataProvider;
    }
}