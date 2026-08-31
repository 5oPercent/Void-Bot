using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "Security")]
public sealed class SecurityEndpointContextTemplate : IContextTemplate
{
    public string GetUserAccountPageAsync { get; set; } = "获取用户账户分页列表";
    public string GetCurrentUserPermissionsAsync { get; set; } = "获取当前用户权限";
    public string GetRoleAsync { get; set; } = "获取角色";
    public string GetRoleListAsync { get; set; } = "获取角色列表";
    public string GetRoleTreeAsync { get; set; } = "获取角色树";
    public string AddRoleAsync { get; set; } = "新增角色";
    public string UpdateRoleAsync { get; set; } = "更新角色";
    public string DeleteRoleAsync { get; set; } = "删除角色";
    public string GetPermissionAsync { get; set; } = "获取权限";
    public string GetPermissionListAsync { get; set; } = "获取权限列表";
    public string GetPermissionTreeAsync { get; set; } = "获取权限树";
    public string AddPermissionAsync { get; set; } = "新增权限";
    public string UpdatePermissionAsync { get; set; } = "更新权限";
    public string DeletePermissionAsync { get; set; } = "删除权限";
    public string GetMenuAsync { get; set; } = "获取菜单";
    public string GetMenuListAsync { get; set; } = "获取菜单列表";
    public string GetMenuTreeAsync { get; set; } = "获取菜单树";
    public string AddMenuAsync { get; set; } = "新增菜单";
    public string UpdateMenuAsync { get; set; } = "更新菜单";
    public string DeleteMenuAsync { get; set; } = "删除菜单";
    public string GetEndpointListAsync { get; set; } = "获取接口列表";
    public string GetRolesByUserIdAsync { get; set; } = "获取用户角色";
    public string AssignRolesToUserAsync { get; set; } = "为用户分配角色";
    public string UnassignRolesFromUserAsync { get; set; } = "取消用户角色分配";
    public string ReplaceUserRolesAsync { get; set; } = "替换用户角色";
    public string UpdateUserRoleDeadlineAsync { get; set; } = "更新用户角色截止时间";
    public string GetPermissionsByRoleIdsAsync { get; set; } = "获取角色权限";
    public string AssignPermissionsToRoleAsync { get; set; } = "为角色分配权限";
    public string UnassignPermissionsFromRoleAsync { get; set; } = "取消角色权限分配";
    public string ReplaceRolePermissionsAsync { get; set; } = "替换角色权限";
    public string GetMenusByPermissionIdAsync { get; set; } = "获取权限菜单";
    public string AssignMenusToPermissionAsync { get; set; } = "为权限分配菜单";
    public string UnassignMenusFromPermissionAsync { get; set; } = "取消权限菜单分配";
    public string ReplacePermissionMenusAsync { get; set; } = "替换权限菜单";
    public string GetEndpointsByPermissionIdsAsync { get; set; } = "获取权限接口";
    public string AssignEndpointsToPermissionAsync { get; set; } = "为权限分配接口";
    public string UnassignEndpointsFromPermissionAsync { get; set; } = "取消权限接口分配";
    public string ReplacePermissionEndpointsAsync { get; set; } = "替换权限接口";

}
