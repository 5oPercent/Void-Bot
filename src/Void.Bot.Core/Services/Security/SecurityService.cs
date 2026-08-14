using Serilog;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Constants.Security;
using Void.Bot.Message.Dto.Security;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Services.Security;

public interface ISecurityService : IScope
{
    Task<GetCurrentUserPermissionsResponse> GetCurrentUserPermissionsAsync(
        CancellationToken cancellationToken);

    Task<bool> VerifyPermissionEndpointAsync(
        Guid userId, string endpoint, string method, CancellationToken cancellationToken);
}

public class SecurityService : ISecurityService
{
    private readonly IUserRoleService _userRoleService;
    private readonly IRolePermissionService _rolePermissionService;
    private readonly IPermissionMenuService _permissionMenuService;
    private readonly IPermissionEndpointService _permissionEndpointService;
    private readonly IPermissionDataProvider _permissionDataProvider;
    private readonly IMenuDataProvider _menuDataProvider;
    private readonly ICurrentUser _currentUser;
    
    public SecurityService(
        IUserRoleService userRoleService,
        IRolePermissionService rolePermissionService,
        IPermissionMenuService permissionMenuService,
        IPermissionEndpointService permissionEndpointService,
        IPermissionDataProvider permissionDataProvider,
        IMenuDataProvider menuDataProvider,
        ICurrentUser currentUser)
    {
        _userRoleService = userRoleService;
        _rolePermissionService = rolePermissionService;
        _permissionMenuService = permissionMenuService;
        _permissionEndpointService = permissionEndpointService;
        _permissionDataProvider = permissionDataProvider;
        _menuDataProvider = menuDataProvider;
        _currentUser = currentUser;
    }

    public async Task<GetCurrentUserPermissionsResponse> GetCurrentUserPermissionsAsync(
        CancellationToken cancellationToken)
    {
        return await GetUserSecurityContextAsync(
            _currentUser.Id.GetValueOrDefault(), true, cancellationToken).ConfigureAwait(false);
    }

    private async Task<GetCurrentUserPermissionsResponse> GetUserSecurityContextAsync(
        Guid userId, bool includeMenus, CancellationToken cancellationToken)
    {
        var response = new GetCurrentUserPermissionsResponse
        {
            Data = new GetCurrentUserPermissionsData()
        };

        if (userId == Guid.Empty)
        {
            return response;
        }

        var userRoles = await _userRoleService.GetRolesByUserIdAsync(
            new GetRolesByUserIdRequest { UserId = userId }, cancellationToken).ConfigureAwait(false);
        if (userRoles.Data == null || userRoles.Data.UserRoles.Count == 0)
        {
            return response;
        }

        var enabledRoleIds = userRoles.Data.Roles
            .Where(x => x.Status == StatusEnum.Enable)
            .Select(x => x.Id)
            .ToHashSet();
        var activeUserRoles = userRoles.Data.UserRoles
            .Where(x => enabledRoleIds.Contains(x.RoleId))
            .Where(x => x.Deadline == null || x.Deadline > DateTimeOffset.Now)
            .ToList();

        if (activeUserRoles.Count == 0)
        {
            return response;
        }

        var roleDeadlines = new Dictionary<Guid, DateTimeOffset?>();
        foreach (var userRole in activeUserRoles)
        {
            roleDeadlines[userRole.RoleId] = roleDeadlines.TryGetValue(userRole.RoleId, out var deadline)
                ? MergeDeadline(deadline, userRole.Deadline)
                : userRole.Deadline;
        }

        response.Data.Roles = userRoles.Data.Roles
            .Where(x => roleDeadlines.ContainsKey(x.Id))
            .Select(x => new AuthorizedRoleDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Code = x.Code,
                Deadline = roleDeadlines[x.Id]
            })
            .ToList();

        var roleIds = roleDeadlines.Keys.ToList();
        var isSuperAdmin = roleDeadlines.TryGetValue(RoleConstants.SuperAdminId, out var superAdminDeadline);
        var rolePermissions = await _rolePermissionService.GetPermissionsByRoleIdsAsync(
            new GetPermissionsByRoleIdsRequest { RoleIds = roleIds }, cancellationToken).ConfigureAwait(false);
        if (!includeMenus)
        {
            if (isSuperAdmin)
            {
                var permissions = await _permissionDataProvider.GetAllPermissionAsync(cancellationToken).ConfigureAwait(false);
                response.Data.Permissions = permissions
                    .Where(x => x.Status == StatusEnum.Enable)
                    .Select(x => new AuthorizedPermissionDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        Code = x.Code,
                        Deadline = superAdminDeadline
                    })
                    .ToList();
            }
            else
            {
                response.Data.Permissions = rolePermissions.Data?
                    .Where(x => x.Status == StatusEnum.Enable)
                    .Select(x => new AuthorizedPermissionDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        Code = x.Code
                    })
                    .ToList() ?? [];
            }
            return response;
        }

        var rolePermissionLinks = await _rolePermissionService.GetRolePermissionsAsync(
            roleIds, cancellationToken).ConfigureAwait(false);
        var roleIdsByPermission = rolePermissionLinks
            .GroupBy(x => x.PermissionId)
            .ToDictionary(x => x.Key, x => x.Select(y => y.RoleId));

        var permissionMap = new Dictionary<Guid, AuthorizedPermissionDto>();
        foreach (var permission in (rolePermissions.Data ?? []).Where(x => x.Status == StatusEnum.Enable))
        {
            if (!roleIdsByPermission.TryGetValue(permission.Id, out var permissionRoleIds))
            {
                continue;
            }

            var deadline = AggregateDeadline(permissionRoleIds.Select(x => roleDeadlines[x]));
            permissionMap[permission.Id] = new AuthorizedPermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Description = permission.Description,
                Code = permission.Code,
                Deadline = deadline
            };
        }

        if (isSuperAdmin)
        {
            var permissions = await _permissionDataProvider.GetAllPermissionAsync(cancellationToken).ConfigureAwait(false);
            foreach (var permission in permissions.Where(x => x.Status == StatusEnum.Enable))
            {
                permissionMap[permission.Id] = new AuthorizedPermissionDto
                {
                    Id = permission.Id,
                    Name = permission.Name,
                    Description = permission.Description,
                    Code = permission.Code,
                    Deadline = permissionMap.TryGetValue(permission.Id, out var existingPermission)
                        ? MergeDeadline(existingPermission.Deadline, superAdminDeadline)
                        : superAdminDeadline
                };
            }
        }

        response.Data.Permissions = permissionMap.Values.ToList();
        if (response.Data.Permissions.Count == 0)
        {
            return response;
        }

        var permissionIds = permissionMap.Keys.ToList();
        var permissionMenus = await _permissionMenuService.GetMenusByPermissionIdsAsync(
            new GetMenusByPermissionIdsRequest { PermissionIds = permissionIds }, cancellationToken).ConfigureAwait(false);
        var permissionMenuLinks = await _permissionMenuService.GetPermissionMenusAsync(
            permissionIds, cancellationToken).ConfigureAwait(false);
        var permissionIdsByMenu = permissionMenuLinks
            .GroupBy(x => x.MenuId)
            .ToDictionary(x => x.Key, x => x.Select(y => y.PermissionId));

        var menuMap = new Dictionary<Guid, AuthorizedMenuDto>();
        foreach (var menu in (permissionMenus.Data ?? []).Where(x => x.Status == StatusEnum.Enable))
        {
            if (!permissionIdsByMenu.TryGetValue(menu.Id, out var menuPermissionIds))
            {
                continue;
            }

            var deadline = AggregateDeadline(menuPermissionIds.Select(x => permissionMap[x].Deadline));
            menuMap[menu.Id] = new AuthorizedMenuDto
            {
                Id = menu.Id,
                Pid = menu.Pid,
                Type = menu.Type,
                Name = menu.Name,
                Description = menu.Description,
                Path = menu.Path,
                Icon = menu.Icon,
                Sequence = menu.Sequence,
                Deadline = deadline
            };
        }

        if (isSuperAdmin)
        {
            var menus = await _menuDataProvider.GetAllMenuAsync(cancellationToken).ConfigureAwait(false);
            foreach (var menu in menus.Where(x => x.Status == StatusEnum.Enable))
            {
                menuMap[menu.Id] = new AuthorizedMenuDto
                {
                    Id = menu.Id,
                    Pid = menu.Pid,
                    Type = menu.Type,
                    Name = menu.Name,
                    Description = menu.Description,
                    Path = menu.Path,
                    Icon = menu.Icon,
                    Sequence = menu.Sequence,
                    Deadline = menuMap.TryGetValue(menu.Id, out var existingMenu)
                        ? MergeDeadline(existingMenu.Deadline, superAdminDeadline)
                        : superAdminDeadline
                };
            }
        }

        response.Data.Menus = menuMap.Values.ToList();
        return response;
    }

    private static DateTimeOffset? AggregateDeadline(IEnumerable<DateTimeOffset?> deadlines)
    {
        using var enumerator = deadlines.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return null;
        }

        var deadline = enumerator.Current;
        while (enumerator.MoveNext())
        {
            deadline = MergeDeadline(deadline, enumerator.Current);
        }

        return deadline;
    }

    private static DateTimeOffset? MergeDeadline(DateTimeOffset? current, DateTimeOffset? candidate)
    {
        if (current == null || candidate == null)
        {
            return null;
        }

        return current > candidate ? current : candidate;
    }

    public async Task<bool> VerifyPermissionEndpointAsync(
        Guid userId, string endpoint, string method, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty || string.IsNullOrEmpty(endpoint))
        {
            return false;
        }

        var permissions = await GetUserSecurityContextAsync(userId, false, cancellationToken).ConfigureAwait(false);
        if (permissions.Data == null || permissions.Data.Permissions.Count == 0)
        {
            Log.Warning("User with ID {UserId} has no permissions assigned to their roles.", userId);
            return false;
        }

        var permissionIds = permissions.Data.Permissions.Select(p => p.Id).ToList();
        var permissionEndpoints = await _permissionEndpointService.GetEndpointsByPermissionIdsAsync(
            new GetEndpointsByPermissionIdsRequest { PermissionIds = permissionIds }, cancellationToken).ConfigureAwait(false);
        if (permissionEndpoints.Data == null || permissionEndpoints.Data.Count == 0)
        {
            Log.Warning("User with ID {UserId} has no endpoints assigned to their permissions.", userId);
            return false;
        }

        var isAllowed = permissionEndpoints.Data.Any(ep => 
            string.Equals(ep.Endpoint, endpoint, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(ep.Method, method, StringComparison.OrdinalIgnoreCase));

        return isAllowed;
    }
}