using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Security;

namespace Void.Bot.Core.Services.Security;

public interface IRoleDataProvider : IScope
{
    public Task<Role?> GetRoleAsync(Guid id, CancellationToken cancellationToken);
    
    public Task<List<Role>> GetRoleListAsync(List<Guid> idList, CancellationToken cancellationToken);
    
    public Task<List<Role>> GetAllRoleAsync(CancellationToken cancellationToken);
    
    public Task CreateRoleAsync(Role entity, CancellationToken cancellationToken);
    
    public Task UpdateRoleAsync(Role entity, CancellationToken cancellationToken);
    
    public Task DeleteRoleListAsync(List<Guid> idList, CancellationToken cancellationToken);
    
    public Task<bool> GetAnyAsync(List<Guid> idList, CancellationToken cancellationToken);
    
    public Task<bool> GetChildAnyAsync(List<Guid> pidList, CancellationToken cancellationToken);
}

public class RoleDataProvider : IRoleDataProvider
{
    private readonly IRepository<Role> _roleRep;
    
    public RoleDataProvider(IRepository<Role> roleRep)
    {
        _roleRep = roleRep;
    }
    
    public async Task<Role?> GetRoleAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _roleRep.QueryNoTracking(x => x.Id == id).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<Role>> GetRoleListAsync(List<Guid> idList, CancellationToken cancellationToken)
    {
        return await _roleRep.QueryNoTracking(x => idList.Contains(x.Id)).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<Role>> GetAllRoleAsync(CancellationToken cancellationToken)
    {
        return await _roleRep.QueryNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task CreateRoleAsync(Role entity, CancellationToken cancellationToken)
    {
        await _roleRep.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateRoleAsync(Role entity, CancellationToken cancellationToken)
    {
        await _roleRep.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteRoleListAsync(List<Guid> idList, CancellationToken cancellationToken)
    {
        await _roleRep.DeleteAsync(x => idList.Contains(x.Id), cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> GetAnyAsync(List<Guid> idList, CancellationToken cancellationToken)
    {
        return await _roleRep.AnyAsync(x => idList.Contains(x.Id), cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> GetChildAnyAsync(List<Guid> pidList, CancellationToken cancellationToken)
    {
        return await _roleRep.AnyAsync(x => pidList.Contains(x.Pid), cancellationToken).ConfigureAwait(false);
    }
}