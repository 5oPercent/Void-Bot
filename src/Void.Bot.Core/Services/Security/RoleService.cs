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
    public Task<GetRoleResponse> GetRoleAsync(GetRoleRequest request, CancellationToken cancellationToken = default);
    
    public Task<GetRoleListResponse> GetRoleListAsync(GetRoleListRequest request, CancellationToken cancellationToken = default);
    
    public Task<AddRoleResponse> AddRoleAsync(AddRoleCommand command, CancellationToken cancellationToken = default);
    
    public Task<UpdateRoleResponse> UpdateRoleAsync(UpdateRoleCommand command, CancellationToken cancellationToken = default);
    
    public Task<DeleteRolesResponse> DeleteRolesAsync(DeleteRolesCommand command, CancellationToken cancellationToken = default);
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

    public async Task<GetRoleResponse> GetRoleAsync(GetRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = await _roleDataProvider.GetRoleAsync(request.Id, cancellationToken).ConfigureAwait(false);

        return new GetRoleResponse
        {
            Data = _mapper.Map<RoleDto>(role)
        };
    }

    public async Task<GetRoleListResponse> GetRoleListAsync(GetRoleListRequest request, CancellationToken cancellationToken = default)
    {
        var roleList = await _roleDataProvider.GetRoleListAsync(request.IdList, cancellationToken).ConfigureAwait(false);

        if (request.ConvertTree)
        {
            roleList = roleList.ConvertTree<Guid, Role>();
        }
        
        return new GetRoleListResponse
        {
            Data = _mapper.Map<List<RoleDto>>(roleList)
        };
    }

    public async Task<AddRoleResponse> AddRoleAsync(AddRoleCommand command, CancellationToken cancellationToken = default)
    {
        var hasParent = await _roleDataProvider.GetAnyAsync([command.Role.Pid], cancellationToken).ConfigureAwait(false);
        if (!hasParent)
            return new AddRoleResponse
            {
                Code = HttpStatusCode.BadRequest,
                Message = "Parent role not found",
            };
        
        var role = _mapper.Map<Role>(command.Role);
        
        await _roleDataProvider.CreateRoleAsync(role, cancellationToken).ConfigureAwait(false);

        return new AddRoleResponse();
    }

    public async Task<UpdateRoleResponse> UpdateRoleAsync(UpdateRoleCommand command, CancellationToken cancellationToken = default)
    {
        var hasParent = await _roleDataProvider.GetAnyAsync([command.Role.Pid], cancellationToken).ConfigureAwait(false);
        if (!hasParent)
            return new UpdateRoleResponse
            {
                Code = HttpStatusCode.BadRequest,
                Message = "Parent role not found",
            };
        
        var role = _mapper.Map<Role>(command.Role);
        
        await _roleDataProvider.UpdateRoleAsync(role, cancellationToken).ConfigureAwait(false);

        return new UpdateRoleResponse();
    }

    public async Task<DeleteRolesResponse> DeleteRolesAsync(DeleteRolesCommand command, CancellationToken cancellationToken = default)
    {
        var hasChild = await _roleDataProvider.GetChildAnyAsync(command.IdList, cancellationToken).ConfigureAwait(false);
        if (hasChild)
            return new DeleteRolesResponse
            {
                Code = HttpStatusCode.BadRequest,
                Message = "Child role exists",
            };
        
        await _roleDataProvider.DeleteRoleListAsync(command.IdList, cancellationToken).ConfigureAwait(false);

        return new DeleteRolesResponse();
    }
}