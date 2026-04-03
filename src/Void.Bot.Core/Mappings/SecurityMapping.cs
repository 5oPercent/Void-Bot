using AutoMapper;
using Void.Bot.Core.Domain.Security;
using Void.Bot.Message.Commands.Security;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Core.Mappings;

public class SecurityMapping : Profile
{
    public SecurityMapping()
    {
        CreateMap<UserRole, UserRoleDto>().ReverseMap();
        
        CreateMap<Role, RoleDto>().ReverseMap();
        CreateMap<RoleConflictMatrix, RoleConflictMatrixDto>().ReverseMap();
        CreateMap<RolePermission, RolePermissionDto>().ReverseMap();
        
        CreateMap<Permission, PermissionDto>().ReverseMap();
        CreateMap<PermissionMenu, PermissionMenuDto>().ReverseMap();
        CreateMap<PermissionEndpoint, PermissionEndpointDto>().ReverseMap();
        
        CreateMap<Menu, MenuDto>().ReverseMap();
        
        CreateMap<AddRoleCommand, Role>().ReverseMap();
        CreateMap<AddMenuCommand, Menu>().ReverseMap();
        CreateMap<AddPermissionCommand, Permission>().ReverseMap();
        
        CreateMap<UpdateRoleCommand, Role>().ReverseMap();
        CreateMap<UpdateMenuCommand, Menu>().ReverseMap();
        CreateMap<UpdatePermissionCommand, Permission>().ReverseMap();
    }
}