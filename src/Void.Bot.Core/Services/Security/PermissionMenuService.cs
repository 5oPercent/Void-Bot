using System.Net;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Security;
using Void.Bot.Message.Commands.Security;
using Void.Bot.Message.Dto.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Services.Security;

public interface IPermissionMenuService : IScope
{
    Task<List<PermissionMenuDto>> GetPermissionMenusAsync(
        List<Guid> permissionIds, CancellationToken cancellationToken);

    Task<GetMenusByPermissionIdsResponse> GetMenusByPermissionIdsAsync(
        GetMenusByPermissionIdsRequest request, CancellationToken cancellationToken);
    
    Task<AssignMenusToPermissionResponse> AssignMenusToPermissionAsync(
        AssignMenusToPermissionCommand command, CancellationToken cancellationToken);
    
    Task<UnassignMenusFromPermissionResponse> UnassignMenusFromPermissionAsync(
        UnassignMenusFromPermissionCommand command, CancellationToken cancellationToken);
    
    Task<ReplacePermissionMenusResponse> ReplacePermissionMenusAsync(
        ReplacePermissionMenusCommand command, CancellationToken cancellationToken);
    
    Task<RemoveMenusCascadePermissionResponse> RemoveMenusCascadePermissionAsync(
        RemoveMenusCascadePermissionCommand command, CancellationToken cancellationToken);
    
    Task<RemovePermissionsCascadeMenuResponse> RemovePermissionsCascadeMenuAsync(
        RemovePermissionsCascadeMenuCommand command, CancellationToken cancellationToken);
}

public class PermissionMenuService : IPermissionMenuService
{
    private readonly IMapper _mapper;
    private readonly IPermissionDataProvider _permissionDataProvider;
    private readonly IMenuDataProvider _menuDataProvider;
    private readonly IRepository<PermissionMenu> _permissionMenuRep;

    public PermissionMenuService(
        IMapper mapper,
        IPermissionDataProvider permissionDataProvider,
        IMenuDataProvider menuDataProvider,
        IRepository<PermissionMenu> permissionMenuRep)
    {
        _mapper = mapper;
        _permissionDataProvider = permissionDataProvider;
        _menuDataProvider = menuDataProvider;
        _permissionMenuRep = permissionMenuRep;
    }

    public async Task<List<PermissionMenuDto>> GetPermissionMenusAsync(
        List<Guid> permissionIds, CancellationToken cancellationToken)
    {
        var permissionIdSet = permissionIds.ToHashSet();
        var permissionMenus = await _permissionMenuRep.QueryNoTracking()
            .Where(x => permissionIdSet.Contains(x.PermissionId))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return _mapper.Map<List<PermissionMenuDto>>(permissionMenus);
    }

    public async Task<GetMenusByPermissionIdsResponse> GetMenusByPermissionIdsAsync(
        GetMenusByPermissionIdsRequest request, CancellationToken cancellationToken)
    {
        var response = new GetMenusByPermissionIdsResponse();
        
        var permissionIdSet = new HashSet<Guid>(request.PermissionIds);
        
        var menuIds = await _permissionMenuRep.QueryNoTracking()
            .Where(x => permissionIdSet.Contains(x.PermissionId))
            .Select(x => x.MenuId).Distinct()
            .ToListAsync(cancellationToken).ConfigureAwait(false);
            
        if (menuIds.Count == 0)
        {
            response.Data = [];
            return response;
        }

        var menus = await _menuDataProvider.GetMenuListAsync(menuIds, cancellationToken).ConfigureAwait(false);
        response.Data = _mapper.Map<List<MenuDto>>(menus);
        
        return response;
    }

    public async Task<AssignMenusToPermissionResponse> AssignMenusToPermissionAsync(
        AssignMenusToPermissionCommand command, CancellationToken cancellationToken)
    {
        var response = new AssignMenusToPermissionResponse();
        
        var permission = await _permissionDataProvider.GetPermissionAsync(command.PermissionId, cancellationToken).ConfigureAwait(false);
        if (permission == null)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = $"Permission with ID {command.PermissionId} does not exist.";
            return response;
        }

        var menuIds = command.MenuIds.Distinct().ToList();
        
        var existingMenus = await _menuDataProvider.GetMenuListAsync(menuIds, cancellationToken).ConfigureAwait(false);
        var existingMenuIds = existingMenus.Select(m => m.Id).ToHashSet();
        var nonExistentMenuIds = menuIds.Where(id => !existingMenuIds.Contains(id)).ToList();
    
        if (nonExistentMenuIds.Count != 0)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = $"Menu IDs do not exist: {string.Join(", ", nonExistentMenuIds)}";
            return response;
        }
        
        var existingMenuIdsForPermission = await _permissionMenuRep.QueryNoTracking()
            .Where(x => x.PermissionId == command.PermissionId && menuIds.Contains(x.MenuId))
            .Select(x => x.MenuId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        
        var newMenuIds = menuIds.Except(existingMenuIdsForPermission).ToList();
        
        if (newMenuIds.Count == 0)
        {
            return response;
        }

        var newPermissionMenus = newMenuIds.Select(menuId => new PermissionMenu
        {
            Id = Guid.NewGuid(),
            PermissionId = command.PermissionId,
            MenuId = menuId
        }).ToList();

        await _permissionMenuRep.InsertManyAsync(newPermissionMenus, cancellationToken).ConfigureAwait(false);
        
        return response;
    }

    public async Task<UnassignMenusFromPermissionResponse> UnassignMenusFromPermissionAsync(
        UnassignMenusFromPermissionCommand command, CancellationToken cancellationToken)
    {
        var response = new UnassignMenusFromPermissionResponse();
        
        await _permissionMenuRep.DeleteAsync(x => x.PermissionId == command.PermissionId && command.MenuIds.Contains(x.MenuId), cancellationToken)
            .ConfigureAwait(false);
        
        return response;
    }

    public async Task<ReplacePermissionMenusResponse> ReplacePermissionMenusAsync(
        ReplacePermissionMenusCommand command, CancellationToken cancellationToken)
    {
        var response = new ReplacePermissionMenusResponse();
        
        var permission = await _permissionDataProvider.GetPermissionAsync(command.PermissionId, cancellationToken).ConfigureAwait(false);
        if (permission == null)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = $"Permission with ID {command.PermissionId} does not exist.";
            return response;
        }
        
        await _permissionMenuRep.DeleteAsync(x => x.PermissionId == command.PermissionId, cancellationToken).ConfigureAwait(false);
        
        if (command.MenuIds.Count == 0)
        {
            return response;
        }
        
        var menuIds = command.MenuIds.Distinct().ToList();
        var existingMenus = await _menuDataProvider.GetMenuListAsync(menuIds, cancellationToken).ConfigureAwait(false);
        
        var newPermissionMenus = existingMenus.Select(m => new PermissionMenu
        {
            PermissionId = command.PermissionId,
            MenuId = m.Id
        }).ToList();

        await _permissionMenuRep.InsertManyAsync(newPermissionMenus, cancellationToken).ConfigureAwait(false);
        
        return response;
    }

    public async Task<RemoveMenusCascadePermissionResponse> RemoveMenusCascadePermissionAsync(
        RemoveMenusCascadePermissionCommand command, CancellationToken cancellationToken)
    {
        var response = new RemoveMenusCascadePermissionResponse();

        await _permissionMenuRep.DeleteAsync(x => command.MenuIds.Contains(x.MenuId), cancellationToken).ConfigureAwait(false);
        
        return response;
    }

    public async Task<RemovePermissionsCascadeMenuResponse> RemovePermissionsCascadeMenuAsync(
        RemovePermissionsCascadeMenuCommand command, CancellationToken cancellationToken)
    {
        var response = new RemovePermissionsCascadeMenuResponse();
        
        await _permissionMenuRep.DeleteAsync(x => command.PermissionIds.Contains(x.PermissionId), cancellationToken).ConfigureAwait(false);
        
        return response;
    }
}