using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "Security")]
public sealed class SecurityEndpointContextTemplate : IContextTemplate
{
    public string GetUserAccountPage { get; set; } = "获取用户账户分页列表";
    public string GetCurrentUserPermissions { get; set; } = "获取当前用户权限";
    public string GetRole { get; set; } = "获取角色";
    public string GetRoleList { get; set; } = "获取角色列表";
    public string GetRoleTree { get; set; } = "获取角色树";
    public string AddRole { get; set; } = "新增角色";
    public string UpdateRole { get; set; } = "更新角色";
    public string DeleteRole { get; set; } = "删除角色";
    public string GetPermission { get; set; } = "获取权限";
    public string GetPermissionList { get; set; } = "获取权限列表";
    public string GetPermissionTree { get; set; } = "获取权限树";
    public string AddPermission { get; set; } = "新增权限";
    public string UpdatePermission { get; set; } = "更新权限";
    public string DeletePermission { get; set; } = "删除权限";
    public string GetMenu { get; set; } = "获取菜单";
    public string GetMenuList { get; set; } = "获取菜单列表";
    public string GetMenuTree { get; set; } = "获取菜单树";
    public string AddMenu { get; set; } = "新增菜单";
    public string UpdateMenu { get; set; } = "更新菜单";
    public string DeleteMenu { get; set; } = "删除菜单";
    public string GetEndpointList { get; set; } = "获取接口列表";
    public string GetRolesByUserId { get; set; } = "获取用户角色";
    public string AssignRolesToUser { get; set; } = "为用户分配角色";
    public string UnassignRolesFromUser { get; set; } = "取消用户角色分配";
    public string ReplaceUserRoles { get; set; } = "替换用户角色";
    public string UpdateUserRoleDeadline { get; set; } = "更新用户角色截止时间";
    public string GetPermissionsByRoleIds { get; set; } = "获取角色权限";
    public string AssignPermissionsToRole { get; set; } = "为角色分配权限";
    public string UnassignPermissionsFromRole { get; set; } = "取消角色权限分配";
    public string ReplaceRolePermissions { get; set; } = "替换角色权限";
    public string GetMenusByPermissionId { get; set; } = "获取权限菜单";
    public string AssignMenusToPermission { get; set; } = "为权限分配菜单";
    public string UnassignMenusFromPermission { get; set; } = "取消权限菜单分配";
    public string ReplacePermissionMenus { get; set; } = "替换权限菜单";
    public string GetEndpointsByPermissionIds { get; set; } = "获取权限接口";
    public string AssignEndpointsToPermission { get; set; } = "为权限分配接口";
    public string UnassignEndpointsFromPermission { get; set; } = "取消权限接口分配";
    public string ReplacePermissionEndpoints { get; set; } = "替换权限接口";

}
