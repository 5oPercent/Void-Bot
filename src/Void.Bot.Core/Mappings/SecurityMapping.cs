using AutoMapper;
using Void.Bot.Core.Domain.Security;
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
    }
}