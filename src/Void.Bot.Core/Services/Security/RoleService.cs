using System.Net;
using AutoMapper;
using Void.Bot.Core.Domain.Security;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Commands.Security;
using Void.Bot.Message.Dto.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Services.Security;

public partial interface IRoleService : IScope
{
    public Task<GetRoleResponse> GetRoleAsync(
        GetRoleRequest request, CancellationToken cancellationToken);
    
    public Task<GetRoleListResponse> GetRoleListAsync(
        GetRoleListRequest request, CancellationToken cancellationToken);
    
    public Task<GetRoleTreeResponse> GetRoleTreeAsync(
        GetRoleTreeRequest request, CancellationToken cancellationToken);
    
    public Task<AddRoleResponse> AddRoleAsync(
        AddRoleCommand command, CancellationToken cancellationToken);
    
    public Task<UpdateRoleResponse> UpdateRoleAsync(
        UpdateRoleCommand command, CancellationToken cancellationToken);
    
    public Task<DeleteRoleCascadeResponse> DeleteRoleCascadeAsync(
        DeleteRoleCascadeCommand command, CancellationToken cancellationToken);
}

public partial class RoleService : IRoleService
{
    private readonly IMapper _mapper;
    private readonly IRoleDataProvider _roleDataProvider;
    private readonly IUserRoleService _userRoleService;
    private readonly IRolePermissionService _rolePermissionService;
    
    public RoleService(
        IMapper mapper,
        IRoleDataProvider roleDataProvider,
        IUserRoleService userRoleService,
        IRolePermissionService rolePermissionService)
    {
        _mapper = mapper;
        _roleDataProvider = roleDataProvider;
        _userRoleService = userRoleService;
        _rolePermissionService = rolePermissionService;
    }

    public async Task<GetRoleResponse> GetRoleAsync(
        GetRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await _roleDataProvider.GetRoleAsync(request.Id, cancellationToken).ConfigureAwait(false);

        return new GetRoleResponse
        {
            Data = _mapper.Map<RoleDto>(role)
        };
    }

    public async Task<GetRoleListResponse> GetRoleListAsync(
        GetRoleListRequest request, CancellationToken cancellationToken)
    {
        var roleList = await _roleDataProvider.GetRoleListAsync(request.IdList, [], cancellationToken).ConfigureAwait(false);
        
        return new GetRoleListResponse
        {
            Data = _mapper.Map<List<RoleDto>>(roleList)
        };
    }

    public async Task<GetRoleTreeResponse> GetRoleTreeAsync(
        GetRoleTreeRequest request, CancellationToken cancellationToken)
    {
        var list = await _roleDataProvider.GetAllRoleAsync(cancellationToken).ConfigureAwait(false);

        var tree = list
            .OrderBy(x => x.Sequence)
            .ThenBy(x => x.CreatedAt)
            .ConvertTree<Guid, Role>();

        return new GetRoleTreeResponse
        {
            Data = _mapper.Map<List<RoleDto>>(tree)
        };
    }

    public async Task<AddRoleResponse> AddRoleAsync(
        AddRoleCommand command, CancellationToken cancellationToken)
    {
        var response = new AddRoleResponse();

        if (command.Pid != Guid.Empty)
        {
            var hasParent = await _roleDataProvider.GetAnyAsync([command.Pid], cancellationToken).ConfigureAwait(false);
            if (!hasParent)
            {
                response.Code = HttpStatusCode.BadRequest;
                response.Message = "Parent role not found";
                return response;
            }
        }
        
        var role = _mapper.Map<Role>(command);
        
        await _roleDataProvider.CreateRoleAsync(role, cancellationToken).ConfigureAwait(false);

        return response;
    }

    public async Task<UpdateRoleResponse> UpdateRoleAsync(
        UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        var response = new UpdateRoleResponse();
        
        var hasBuilt = await _roleDataProvider.GetBuiltAnyAsync([command.Id], cancellationToken).ConfigureAwait(false);
        if (hasBuilt)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = "System built-in role cannot be modified.";
            return response;
        }
        
        var role = await _roleDataProvider.GetRoleAsync(command.Id, cancellationToken).ConfigureAwait(false);
        if (role == null)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = "Role not found";
            return response;
        }

        if (command.Pid != Guid.Empty && command.Pid != role.Pid)
        {
            var hasParent = await _roleDataProvider.GetAnyAsync([command.Pid], cancellationToken).ConfigureAwait(false);
            if (!hasParent)
            {
                response.Code = HttpStatusCode.BadRequest;
                response.Message = "Parent role not found";
                return response;
            }
        }
        
        _mapper.Map(command, role);
        await _roleDataProvider.UpdateRoleAsync(role, cancellationToken).ConfigureAwait(false);

        return response;
    }

    public async Task<DeleteRoleCascadeResponse> DeleteRoleCascadeAsync(
        DeleteRoleCascadeCommand command, CancellationToken cancellationToken)
    {
        var response = new DeleteRoleCascadeResponse();

        var roleList = await _roleDataProvider.GetAllRoleAsync(cancellationToken).ConfigureAwait(false);
        var roleIds = command.Cascade
            ? roleList.GetDescendantIds<Guid, Role>(command.IdList)
            : command.IdList.Distinct().ToList();

        var hasBuilt = await _roleDataProvider.GetBuiltAnyAsync(roleIds, cancellationToken).ConfigureAwait(false);
        if (hasBuilt)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = "System built-in roles cannot be deleted.";
            return response;
        }

        var roleIdSet = roleIds.ToHashSet();
        var hasChild = roleList.Any(x => roleIdSet.Contains(x.Pid) && !roleIdSet.Contains(x.Id));
        if (hasChild)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = "Child role exists";
            return response;
        }

        // Remove user-role data
        await _userRoleService.RemoveRolesCascadeUserAsync(
            new RemoveRolesCascadeUserCommand { RoleIds = roleIds }, cancellationToken).ConfigureAwait(false);
        
        // Remove role-permission data
        await _rolePermissionService.RemoveRoleCascadePermissionAsync(
            new RemoveRoleCascadePermissionCommand { RoleIds = roleIds }, cancellationToken).ConfigureAwait(false);
        
        await _roleDataProvider.DeleteRoleListAsync(roleIds, cancellationToken).ConfigureAwait(false);
        
        return response;
    }
}