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
    Task<GetMenusByPermissionIdResponse> GetMenusByPermissionIdAsync(
        GetMenusByPermissionIdRequest request, CancellationToken cancellationToken);
    
    Task<AssignMenusToPermissionResponse> AssignMenusToPermissionAsync(
        AssignMenusToPermissionCommand command, CancellationToken cancellationToken);
    
    Task<UpdatePermissionMenusResponse> UpdatePermissionMenusAsync(
        UpdatePermissionMenusCommand command, CancellationToken cancellationToken);
    
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

    public async Task<GetMenusByPermissionIdResponse> GetMenusByPermissionIdAsync(
        GetMenusByPermissionIdRequest request, CancellationToken cancellationToken)
    {
        var response = new GetMenusByPermissionIdResponse();
        
        var menuIds = await _permissionMenuRep.QueryNoTracking()
            .Where(pm => pm.PermissionId == request.PermissionId)
            .Select(pm => pm.MenuId)
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
            .Where(pm => pm.PermissionId == command.PermissionId && menuIds.Contains(pm.MenuId))
            .Select(pm => pm.MenuId)
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

    public async Task<UpdatePermissionMenusResponse> UpdatePermissionMenusAsync(
        UpdatePermissionMenusCommand command, CancellationToken cancellationToken)
    {
        var response = new UpdatePermissionMenusResponse();
        
        var permission = await _permissionDataProvider.GetPermissionAsync(command.PermissionId, cancellationToken).ConfigureAwait(false);
        if (permission == null)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = $"Permission with ID {command.PermissionId} does not exist.";
            return response;
        }
        
        await _permissionMenuRep.DeleteAsync(pm => pm.PermissionId == command.PermissionId, cancellationToken).ConfigureAwait(false);
        
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

        await _permissionMenuRep.DeleteAsync(pm => command.MenuIds.Contains(pm.MenuId), cancellationToken).ConfigureAwait(false);
        
        return response;
    }

    public async Task<RemovePermissionsCascadeMenuResponse> RemovePermissionsCascadeMenuAsync(
        RemovePermissionsCascadeMenuCommand command, CancellationToken cancellationToken)
    {
        var response = new RemovePermissionsCascadeMenuResponse();
        
        await _permissionMenuRep.DeleteAsync(pm => command.PermissionIds.Contains(pm.PermissionId), cancellationToken).ConfigureAwait(false);
        
        return response;
    }
}