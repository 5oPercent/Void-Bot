namespace Void.Bot.Core.Services.Security;

public partial interface IRoleService : IScope
{
    public Task GetRoleAsync();
    
    public Task GetRolesAsync();
    
    public Task AddRoleAsync();
    
    public Task RemoveRoleAsync();
    
    public Task UpdateRoleAsync();
}

public partial class RoleService : IRoleService
{
    private readonly IRoleDataProvider _roleDataProvider;
    
    public RoleService(IRoleDataProvider roleDataProvider)
    {
        _roleDataProvider = roleDataProvider;
    }

    public Task GetRoleAsync()
    {
        throw new NotImplementedException();
    }

    public Task GetRolesAsync()
    {
        throw new NotImplementedException();
    }

    public Task AddRoleAsync()
    {
        throw new NotImplementedException();
    }

    public Task RemoveRoleAsync()
    {
        throw new NotImplementedException();
    }

    public Task UpdateRoleAsync()
    {
        throw new NotImplementedException();
    }
}