using System.Net;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Security;
using Void.Bot.Message.Commands.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Services.Security;

public interface IRolePermissionService : IScope
{
    Task<GetPermissionsByRoleIdResponse> GetPermissionsByRoleIdAsync(
        GetPermissionsByRoleIdRequest request, CancellationToken cancellationToken);
    
    Task<GetRoleHasPermissionResponse> GetRoleHasPermissionAsync(
        GetRoleHasPermissionRequest request, CancellationToken cancellationToken);
    
    Task<AssignPermissionToRoleResponse> AssignPermissionToRoleAsync(
        AssignPermissionToRoleCommand command, CancellationToken cancellationToken);
    
    Task<UpdateRolePermissionsResponse> UpdateRolePermissionsAsync(
        UpdateRolePermissionsCommand command, CancellationToken cancellationToken);
    
    Task<RemovePermissionFromRoleResponse> RemovePermissionFromRoleAsync(
        RemovePermissionFromRoleCommand command, CancellationToken cancellationToken);
}

public class RolePermissionService : IRolePermissionService
{
    private readonly IRoleDataProvider _roleDataProvider;
    private readonly IPermissionDataProvider _permissionDataProvider;
    private readonly IRepository<RolePermission> _rolePermissionRep;

    public RolePermissionService(
        IRoleDataProvider roleDataProvider,
        IPermissionDataProvider permissionDataProvider,
        IRepository<RolePermission> rolePermissionRep)
    {
        _roleDataProvider = roleDataProvider;
        _permissionDataProvider = permissionDataProvider;
        _rolePermissionRep = rolePermissionRep;
    }

    public async Task<GetPermissionsByRoleIdResponse> GetPermissionsByRoleIdAsync(
        GetPermissionsByRoleIdRequest request, CancellationToken cancellationToken)
    {
        var list = await _rolePermissionRep.QueryNoTracking()
            .Where(x => x.RoleId == request.RoleId)
            .Select(x => x.PermissionId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new GetPermissionsByRoleIdResponse
        {
            Data = list
        };
    }

    public async Task<GetRoleHasPermissionResponse> GetRoleHasPermissionAsync(
        GetRoleHasPermissionRequest request, CancellationToken cancellationToken)
    {
        var has = await _rolePermissionRep.QueryNoTracking()
            .AnyAsync(x => x.RoleId == request.RoleId && x.PermissionId == request.PermissionId, cancellationToken).ConfigureAwait(false);
        
        return new GetRoleHasPermissionResponse
        {
            Data = has
        };
    }

    public async Task<AssignPermissionToRoleResponse> AssignPermissionToRoleAsync(
        AssignPermissionToRoleCommand command, CancellationToken cancellationToken)
    {
        var response = new AssignPermissionToRoleResponse();
        
        var role = await _roleDataProvider.GetRoleAsync(command.RoleId, cancellationToken).ConfigureAwait(false);
        if (role == null)
        {
            response.Code = HttpStatusCode.NotFound;
            response.Message = $"Role with ID {command.RoleId} does not exist.";
            return response;
        }
        
        var permissionIds = command.PermissionIds.Distinct().ToList();
        var existingPermissions = await _permissionDataProvider.GetPermissionsAsync(permissionIds, cancellationToken).ConfigureAwait(false);
        var existingPermissionIds = existingPermissions.Select(p => p.Id).ToHashSet();
        var nonExistentPermissionIds = permissionIds.Where(id => !existingPermissionIds.Contains(id)).ToList();
    
        if (nonExistentPermissionIds.Any())
        {
            response.Code = HttpStatusCode.NotFound;
            response.Message = $"Permissions with IDs {string.Join(", ", nonExistentPermissionIds)} do not exist.";
            return response;
        }
        
        var existsIds = await _rolePermissionRep.QueryNoTracking()
            .Where(x => x.RoleId == command.RoleId && permissionIds.Contains(x.PermissionId))
            .Select(x => x.PermissionId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        
        var toAdd = permissionIds.Except(existsIds).ToList();
        if (toAdd.Count == 0)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = "All permissions already assigned to the role.";
            return response;
        }
        
        var entities = toAdd.Select(pid => new RolePermission
        {
            RoleId = command.RoleId,
            PermissionId = pid
        }).ToList();
        await _rolePermissionRep.InsertManyAsync(entities, cancellationToken).ConfigureAwait(false);
        
        return response;
    }

    public async Task<UpdateRolePermissionsResponse> UpdateRolePermissionsAsync(
        UpdateRolePermissionsCommand command, CancellationToken cancellationToken)
    {
        var response = new UpdateRolePermissionsResponse();
        
        var role = await _roleDataProvider.GetRoleAsync(command.RoleId, cancellationToken).ConfigureAwait(false);
        if (role == null)
        {
            response.Code = HttpStatusCode.NotFound;
            response.Message = $"Role with ID {command.RoleId} does not exist.";
            return response;
        }
        
        await _rolePermissionRep.DeleteAsync(x => x.RoleId == command.RoleId, cancellationToken).ConfigureAwait(false);
        if (command.PermissionIds.Count == 0)
        {
            return response;
        }
        
        var permissionIds = command.PermissionIds.Distinct().ToList();
        var existingPermissions = await _permissionDataProvider.GetPermissionsAsync(permissionIds, cancellationToken).ConfigureAwait(false);
        
        var entities = existingPermissions.Select(p => new RolePermission
        {
            RoleId = command.RoleId,
            PermissionId = p.Id
        }).ToList();
        
        await _rolePermissionRep.InsertManyAsync(entities, cancellationToken).ConfigureAwait(false);
        
        return response;
    }

    public async Task<RemovePermissionFromRoleResponse> RemovePermissionFromRoleAsync(
        RemovePermissionFromRoleCommand command, CancellationToken cancellationToken)
    {
        var response = new RemovePermissionFromRoleResponse();
        
        var permissionIds = command.PermissionIds.Distinct().ToList();
        var entities = await _rolePermissionRep.QueryNoTracking()
            .Where(x => x.RoleId == command.RoleId && permissionIds.Contains(x.PermissionId))
            .ToListAsync(cancellationToken);
        if (entities.Count == 0)
        {
            response.Code = HttpStatusCode.NotFound;
            response.Message = "No matching role-permission relations found.";
            return response;
        }
        await _rolePermissionRep.DeleteManyAsync(entities, cancellationToken);
        
        return response;
    }
}