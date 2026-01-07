using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Security;

namespace Void.Bot.Core.Services.Security;

public partial interface IRoleDataProvider : IScope
{
    public Task<Role?> GetRoleAsync(Guid id, CancellationToken cancellationToken);
    
    public Task<List<Role>> GetRoleListAsync(List<Guid> idList, List<string> codeList, CancellationToken cancellationToken);
    
    public Task<List<Role>> GetAllRoleAsync(CancellationToken cancellationToken);
    
    public Task CreateRoleAsync(Role entity, CancellationToken cancellationToken);
    
    public Task UpdateRoleAsync(Role entity, CancellationToken cancellationToken);
    
    public Task DeleteRoleListAsync(List<Guid> idList, CancellationToken cancellationToken);
    
    public Task<bool> GetAnyAsync(List<Guid> idList, CancellationToken cancellationToken);
    
    public Task<bool> GetChildAnyAsync(List<Guid> pidList, CancellationToken cancellationToken);
    
    public Task<bool> GetBuiltAnyAsync(List<Guid> idList, CancellationToken cancellationToken);
}

public partial class RoleDataProvider : IRoleDataProvider
{
    private readonly IRepository<Role> _roleRep;
    private readonly IRepository<RoleConflictMatrix> _roleConflictMatrixRep;
    
    public RoleDataProvider(IRepository<Role> roleRep, IRepository<RoleConflictMatrix> roleConflictMatrixRep)
    {
        _roleRep = roleRep;
        _roleConflictMatrixRep = roleConflictMatrixRep;
    }
    
    public async Task<Role?> GetRoleAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _roleRep.Query(x => x.Id == id).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<Role>> GetRoleListAsync(List<Guid> idList, List<string> codeList, CancellationToken cancellationToken)
    {
        if (idList.Count == 0 && codeList.Count == 0)
        {
            return [];
        }

        var query = _roleRep.QueryNoTracking();

        if (idList.Count != 0)
        {
            query = query.Where(x => idList.Contains(x.Id));
        }

        if (codeList.Count != 0)
        {
            query = query.Where(x => codeList.Contains(x.Code));
        }
        
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
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

    public async Task<bool> GetBuiltAnyAsync(List<Guid> idList, CancellationToken cancellationToken)
    {
        return await  _roleRep.AnyAsync(x => idList.Contains(x.Id) && x.Built, cancellationToken).ConfigureAwait(false); 
    }
}