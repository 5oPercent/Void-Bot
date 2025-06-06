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
    
    public Task<DeleteRoleListResponse> DeleteRoleListAsync(
        DeleteRoleListCommand command, CancellationToken cancellationToken);
}

public partial class RoleService : IRoleService
{
    private readonly IMapper _mapper;
    private readonly IRoleDataProvider _roleDataProvider;
    
    public RoleService(IMapper mapper, IRoleDataProvider roleDataProvider)
    {
        _mapper = mapper;
        _roleDataProvider = roleDataProvider;
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
        var roleList = await _roleDataProvider.GetRoleListAsync(request.IdList, cancellationToken).ConfigureAwait(false);
        
        return new GetRoleListResponse
        {
            Data = _mapper.Map<List<RoleDto>>(roleList)
        };
    }

    public async Task<GetRoleTreeResponse> GetRoleTreeAsync(
        GetRoleTreeRequest request, CancellationToken cancellationToken)
    {
        var list = await _roleDataProvider.GetAllRoleAsync(cancellationToken).ConfigureAwait(false);

        var tree = list.ConvertTree<Guid, Role>();

        return new GetRoleTreeResponse
        {
            Data = _mapper.Map<List<RoleDto>>(tree)
        };
    }

    public async Task<AddRoleResponse> AddRoleAsync(
        AddRoleCommand command, CancellationToken cancellationToken)
    {
        var response = new AddRoleResponse();
        
        var hasParent = await _roleDataProvider.GetAnyAsync([command.Role.Pid], cancellationToken).ConfigureAwait(false);
        if (!hasParent)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = "Parent role not found";
            return response;
        }
        
        var role = _mapper.Map<Role>(command.Role);
        
        await _roleDataProvider.CreateRoleAsync(role, cancellationToken).ConfigureAwait(false);

        return response;
    }

    public async Task<UpdateRoleResponse> UpdateRoleAsync(
        UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        var response = new UpdateRoleResponse();
        
        var roleSource = await _roleDataProvider.GetRoleAsync(command.Role.Id, cancellationToken).ConfigureAwait(false);
        if (roleSource == null)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = "Role not found";
            return response;
        }

        if (command.Role.Pid != Guid.Empty)
        {
            var hasParent = await _roleDataProvider.GetAnyAsync([command.Role.Pid], cancellationToken).ConfigureAwait(false);
            if (!hasParent)
            {
                response.Code = HttpStatusCode.BadRequest;
                response.Message = "Parent role not found";
                return response;
            }
        }
        
        var role = _mapper.Map<Role>(command.Role);
        
        await _roleDataProvider.UpdateRoleAsync(role, cancellationToken).ConfigureAwait(false);

        return response;
    }

    public async Task<DeleteRoleListResponse> DeleteRoleListAsync(
        DeleteRoleListCommand command, CancellationToken cancellationToken)
    {
        var response = new DeleteRoleListResponse();
        
        var hasChild = await _roleDataProvider.GetChildAnyAsync(command.IdList, cancellationToken).ConfigureAwait(false);
        if (hasChild)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = "Child role exists";
            return response;
        }
        
        await _roleDataProvider.DeleteRoleListAsync(command.IdList, cancellationToken).ConfigureAwait(false);

        return response;
    }
}