using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Security;

namespace Void.Bot.Core.Services.Security;

public interface IPermissionDataProvider : IScope
{
    public Task<Permission?> GetPermissionAsync(Guid id, CancellationToken cancellationToken);
    
    public Task<List<Permission>> GetPermissionsAsync(List<Guid> idList, CancellationToken cancellationToken);
    
    public Task<List<Permission>> GetAllPermissionAsync(CancellationToken cancellationToken);
    
    public Task CreatePermissionAsync(Permission entity, CancellationToken cancellationToken);
    
    public Task UpdatePermissionAsync(Permission entity, CancellationToken cancellationToken);
    
    public Task DeletePermissionsAsync(List<Guid> idList, CancellationToken cancellationToken);
}

public class PermissionDataProvider : IPermissionDataProvider
{
    private readonly IRepository<Permission> _permissionRep;
    
    public PermissionDataProvider(IRepository<Permission> permissionRep)
    {
        _permissionRep = permissionRep;
    }
    
    public async Task<Permission?> GetPermissionAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _permissionRep.QueryNoTracking(x => x.Id == id).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<Permission>> GetPermissionsAsync(List<Guid> idList, CancellationToken cancellationToken)
    {
        return await _permissionRep.QueryNoTracking(x => idList.Contains(x.Id)).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<Permission>> GetAllPermissionAsync(CancellationToken cancellationToken)
    {
        return await _permissionRep.QueryNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task CreatePermissionAsync(Permission entity, CancellationToken cancellationToken)
    {
        await _permissionRep.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdatePermissionAsync(Permission entity, CancellationToken cancellationToken)
    {
        await _permissionRep.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeletePermissionsAsync(List<Guid> idList, CancellationToken cancellationToken)
    {
        await _permissionRep.DeleteAsync(x => idList.Contains(x.Id), cancellationToken).ConfigureAwait(false);
    }
}