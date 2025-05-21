namespace Void.Bot.Core.Services.Security;

public partial interface IPermissionService : IScope
{
    
}

public partial class PermissionService : IPermissionService
{
    private readonly PermissionDataProvider _permissionDataProvider;

    public PermissionService(PermissionDataProvider permissionDataProvider)
    {
        _permissionDataProvider = permissionDataProvider;
    }
}