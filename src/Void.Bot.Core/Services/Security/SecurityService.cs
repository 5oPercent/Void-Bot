using Serilog;
using Void.Bot.Core.Services.Identity;
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
    private readonly ICurrentUser _currentUser;
    
    public SecurityService(
        IUserRoleService userRoleService,
        IRolePermissionService rolePermissionService,
        IPermissionMenuService permissionMenuService,
        IPermissionEndpointService permissionEndpointService,
        ICurrentUser currentUser)
    {
        _userRoleService = userRoleService;
        _rolePermissionService = rolePermissionService;
        _permissionMenuService = permissionMenuService;
        _permissionEndpointService = permissionEndpointService;
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
            .Select(x => ToAuthorizedRoleDto(x, roleDeadlines[x.Id]))
            .ToList();

        var roleIds = roleDeadlines.Keys.ToList();
        if (!includeMenus)
        {
            var rolePermissions = await _rolePermissionService.GetPermissionsByRoleIdsAsync(
                new GetPermissionsByRoleIdsRequest { RoleIds = roleIds }, cancellationToken).ConfigureAwait(false);

            response.Data.Permissions = rolePermissions.Data?
                .Where(x => x.Status == StatusEnum.Enable)
                .Select(x => ToAuthorizedPermissionDto(x, null))
                .ToList() ?? [];
            return response;
        }

        var rolePermissions = await _rolePermissionService.GetPermissionsByRoleIdsAsync(
            new GetPermissionsByRoleIdsRequest { RoleIds = roleIds }, cancellationToken).ConfigureAwait(false);
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
            permissionMap[permission.Id] = ToAuthorizedPermissionDto(permission, deadline);
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

        foreach (var menu in (permissionMenus.Data ?? []).Where(x => x.Status == StatusEnum.Enable))
        {
            if (!permissionIdsByMenu.TryGetValue(menu.Id, out var menuPermissionIds))
            {
                continue;
            }

            var deadline = AggregateDeadline(menuPermissionIds.Select(x => permissionMap[x].Deadline));
            response.Data.Menus.Add(ToAuthorizedMenuDto(menu, deadline));
        }
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

    private static AuthorizedRoleDto ToAuthorizedRoleDto(RoleDto role, DateTimeOffset? deadline)
    {
        return new AuthorizedRoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            Code = role.Code,
            Deadline = deadline
        };
    }

    private static AuthorizedPermissionDto ToAuthorizedPermissionDto(
        PermissionDto permission, DateTimeOffset? deadline)
    {
        return new AuthorizedPermissionDto
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description,
            Code = permission.Code,
            Deadline = deadline
        };
    }

    private static AuthorizedMenuDto ToAuthorizedMenuDto(MenuDto menu, DateTimeOffset? deadline)
    {
        return new AuthorizedMenuDto
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