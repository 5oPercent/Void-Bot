using System.Net;
using Void.Bot.Message.Commands.Security;
using Void.Bot.Message.Dto.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Services.Security;

public partial interface IRoleService : IScope
{
    public Task<GetRoleConflictMatrixListResponse> GetRoleConflictMatrixList(
        GetRoleConflictMatrixListRequest request, CancellationToken cancellationToken = default);
 
    public Task<AddRoleConflictMatrixResponse> AddRoleConflictMatrix(
        AddRoleConflictMatrixCommand command, CancellationToken cancellationToken = default);
    
    public Task<DeleteRoleConflictMatrixResponse> DeleteRoleConflictMatrix(
        DeleteRoleConflictMatrixCommand command, CancellationToken cancellationToken = default);
    
    public Task<CheckRoleConflictExistsResponse> CheckRoleConflictExistsAsync(
        CheckRoleConflictExistsRequest request, CancellationToken cancellationToken = default);
}

public partial class RoleService : IRoleService
{
    public async Task<GetRoleConflictMatrixListResponse> GetRoleConflictMatrixList(
        GetRoleConflictMatrixListRequest request, CancellationToken cancellationToken = default)
    {
        var roleIdList = request.RoleIdList.Distinct().ToList();
        
        var list = await _roleDataProvider.GetRoleConflictMatrixListAsync(
            roleIdList, request.PageIndex, request.PageSize, cancellationToken).ConfigureAwait(false);

        return new GetRoleConflictMatrixListResponse
        {
            Data = _mapper.Map<List<RoleConflictMatrixDto>>(list)
        };
    }

    public async Task<AddRoleConflictMatrixResponse> AddRoleConflictMatrix(
        AddRoleConflictMatrixCommand command, CancellationToken cancellationToken = default)
    {
        var response = new AddRoleConflictMatrixResponse();

        var checkResponse = await CheckRoleConflictExistsAsync(
            new CheckRoleConflictExistsRequest
            {
                RoleAId = command.RoleAId,
                RoleBId = command.RoleBId
            }, cancellationToken).ConfigureAwait(false);
        
        if (checkResponse.Data)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = "Conflict matrix already exists";
            return response;
        }
        
        await _roleDataProvider.AddRoleConflictMatrixAsync(
            command.RoleAId, command.RoleBId, cancellationToken).ConfigureAwait(false);

        return response;
    }

    public async Task<DeleteRoleConflictMatrixResponse> DeleteRoleConflictMatrix(
        DeleteRoleConflictMatrixCommand command, CancellationToken cancellationToken = default)
    {
        await _roleDataProvider.DeleteRoleConflictMatrixAsync([command.Id], cancellationToken).ConfigureAwait(false);
        
        return new DeleteRoleConflictMatrixResponse();
    }

    public async Task<CheckRoleConflictExistsResponse> CheckRoleConflictExistsAsync(
        CheckRoleConflictExistsRequest request, CancellationToken cancellationToken = default)
    {
        var roleConflictMatrixList = await _roleDataProvider
            .GetRoleConflictMatrixListAsync([request.RoleAId], cancellationToken: cancellationToken).ConfigureAwait(false);
    
        var hasConflict = roleConflictMatrixList.Any(x => 
            (x.RoleAId == request.RoleAId && x.RoleBId == request.RoleBId) || 
            (x.RoleAId == request.RoleBId && x.RoleBId == request.RoleAId));
    
        return new CheckRoleConflictExistsResponse
        {
            Data = hasConflict
        };
    }
}