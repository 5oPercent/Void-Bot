using System.Net;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Security;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Commands.Security;
using Void.Bot.Message.Constants.Security;
using Void.Bot.Message.Dto.Security;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Services.Security;

public interface IUserRoleService : IScope
{
    Task<GetRolesByUserIdResponse> GetRolesByUserIdAsync(
        GetRolesByUserIdRequest request, CancellationToken cancellationToken);
    
    Task<AssignRolesToUserResponse> AssignRolesToUserAsync(
        AssignRolesToUserCommand command, CancellationToken cancellationToken);
    
    Task<UnassignRolesFromUserResponse> UnassignRolesFromUserAsync(
        UnassignRolesFromUserCommand command, CancellationToken cancellationToken);
    
    Task<ReplaceUserRolesResponse> ReplaceUserRolesAsync(
        ReplaceUserRolesCommand command, CancellationToken cancellationToken);

    Task<UpdateUserRoleDeadlineResponse> UpdateUserRoleDeadlineAsync(
        UpdateUserRoleDeadlineCommand command, CancellationToken cancellationToken);

    Task<RemoveRolesCascadeUserResponse> RemoveRolesCascadeUserAsync(
        RemoveRolesCascadeUserCommand command, CancellationToken cancellationToken);
    
    Task<RemoveUsersCascadeRoleResponse> RemoveUsersCascadeRoleAsync(
        RemoveUsersCascadeRoleCommand command, CancellationToken cancellationToken);
    
    Task<bool> CheckUserIsSuperAdminAsync(Guid userId, CancellationToken cancellationToken);
}

public class UserRoleService : IUserRoleService
{
    private readonly IMapper _mapper;
    private readonly IRoleDataProvider _roleDataProvider;
    private readonly IAccountDataProvider _accountDataProvider;
    private readonly IRepository<UserRole> _userRoleRep;
    private readonly IRepository<SecurityBootstrapClaim> _securityBootstrapClaimRep;
    private readonly ICurrentUser _currentUser;
    
    public UserRoleService(
        IMapper mapper,
        IRoleDataProvider roleDataProvider,
        IAccountDataProvider accountDataProvider,
        IRepository<UserRole> userRoleRep,
        IRepository<SecurityBootstrapClaim> securityBootstrapClaimRep,
        ICurrentUser currentUser)
    {
        _mapper = mapper;
        _roleDataProvider = roleDataProvider;
        _accountDataProvider = accountDataProvider;
        _userRoleRep = userRoleRep;
        _securityBootstrapClaimRep = securityBootstrapClaimRep;
        _currentUser = currentUser;
    }
    
    public async Task<GetRolesByUserIdResponse> GetRolesByUserIdAsync(
        GetRolesByUserIdRequest request, CancellationToken cancellationToken)
    {
        var response = new GetRolesByUserIdResponse
        {
            Data = new GetRolesByUserIdData()
        };
        
        var userRoles = await _userRoleRep.QueryNoTracking()
            .Where(x => x.UserId == request.UserId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        if (userRoles.Count == 0)
        {
            return response;
        }
        
        var roleIds = userRoles.Select(x => x.RoleId).Distinct().ToList();
        var roles = await _roleDataProvider.GetRoleListAsync(roleIds, [], cancellationToken).ConfigureAwait(false);
        
        response.Data.UserRoles = _mapper.Map<List<UserRoleDto>>(userRoles);
        response.Data.Roles = _mapper.Map<List<RoleDto>>(roles);
        
        return response;
    }

    public async Task<AssignRolesToUserResponse> AssignRolesToUserAsync(
        AssignRolesToUserCommand command, CancellationToken cancellationToken)
    {
        var response = new AssignRolesToUserResponse();
        
        var user = await _accountDataProvider.GetUserAccountAsync(command.UserId, string.Empty, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = $"User with ID {command.UserId} does not exist.";
            return response;
        }
        
        var roleIds = command.RoleIds.Distinct().ToList();
        if (roleIds.Contains(RoleConstants.SuperAdminId) &&
            !await CurrentUserCanManageSuperAdminAsync(cancellationToken).ConfigureAwait(false))
        {
            response.Code = HttpStatusCode.Forbidden;
            response.Message = "Only an active SuperAdmin can assign the SuperAdmin role.";
            return response;
        }
        
        var existingRoles = await _roleDataProvider.GetRoleListAsync(roleIds, [], cancellationToken).ConfigureAwait(false);
        var existingRoleIds = existingRoles.Select(r => r.Id).ToList();
        var nonExistingRoleIds = roleIds.Except(existingRoleIds).ToList();

        if (nonExistingRoleIds.Count != 0)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = $"Role IDs do not exist: {string.Join(", ", nonExistingRoleIds)}";
            return response;
        }

        var existingRoleIdsForUser = await _userRoleRep.QueryNoTracking()
            .Where(x => x.UserId == command.UserId && roleIds.Contains(x.RoleId))
            .Select(x => x.RoleId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        
        var newRoleIds = roleIds.Except(existingRoleIdsForUser).ToList();

        if (newRoleIds.Count == 0)
        {
            return response;
        }
        
        var newUserRoles = newRoleIds.Select(roleId => new UserRole
        {
            UserId = command.UserId,
            RoleId = roleId
        }).ToList();
        
        await _userRoleRep.InsertManyAsync(newUserRoles, cancellationToken).ConfigureAwait(false);
        
        return response;
    }

    public async Task<UnassignRolesFromUserResponse> UnassignRolesFromUserAsync(
        UnassignRolesFromUserCommand command, CancellationToken cancellationToken)
    {
        var response = new UnassignRolesFromUserResponse();

        if (command.RoleIds.Contains(RoleConstants.SuperAdminId))
        {
            if (await IsBootstrapSuperAdminProtectedFromCurrentUserAsync(
                    command.UserId, cancellationToken).ConfigureAwait(false))
            {
                response.Code = HttpStatusCode.Forbidden;
                response.Message = "A later SuperAdmin cannot unassign the bootstrap SuperAdmin.";
                return response;
            }

            if (!await CurrentUserCanManageSuperAdminAsync(cancellationToken).ConfigureAwait(false))
            {
                response.Code = HttpStatusCode.Forbidden;
                response.Message = "Only an active SuperAdmin can unassign the SuperAdmin role.";
                return response;
            }

            if (await RemovingUsersWouldRemoveLastSuperAdminAsync(
                    [command.UserId], cancellationToken).ConfigureAwait(false))
            {
                response.Code = HttpStatusCode.Conflict;
                response.Message = "At least one active SuperAdmin must remain.";
                return response;
            }
        }

        await _userRoleRep.DeleteAsync(x => x.UserId == command.UserId && command.RoleIds.Contains(x.RoleId), cancellationToken).ConfigureAwait(false);
        
        return response;
    }
    
    public async Task<ReplaceUserRolesResponse> ReplaceUserRolesAsync(
        ReplaceUserRolesCommand command, CancellationToken cancellationToken)
    {
        var response = new ReplaceUserRolesResponse();
        
        var user = await _accountDataProvider.GetUserAccountAsync(command.UserId, string.Empty, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = $"User with ID {command.UserId} does not exist.";
            return response;
        }

        var currentUserRoles = await _userRoleRep.QueryNoTracking()
            .Where(x => x.UserId == command.UserId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var currentRoleIds = currentUserRoles.Select(x => x.RoleId).ToHashSet();
        var requestedRoleIds = command.RoleIds.Distinct().ToList();
        var targetHasSuperAdmin = currentRoleIds.Contains(RoleConstants.SuperAdminId);
        var targetWillHaveSuperAdmin = requestedRoleIds.Contains(RoleConstants.SuperAdminId);
        if ((targetHasSuperAdmin || targetWillHaveSuperAdmin) &&
            !await CurrentUserCanManageSuperAdminAsync(cancellationToken).ConfigureAwait(false))
        {
            response.Code = HttpStatusCode.Forbidden;
            response.Message = "Only an active SuperAdmin can replace a SuperAdmin user's roles.";
            return response;
        }

        if (targetHasSuperAdmin && !targetWillHaveSuperAdmin &&
            await IsBootstrapSuperAdminProtectedFromCurrentUserAsync(
                command.UserId, cancellationToken).ConfigureAwait(false))
        {
            response.Code = HttpStatusCode.Forbidden;
            response.Message = "A later SuperAdmin cannot replace the bootstrap SuperAdmin's role.";
            return response;
        }

        if (targetHasSuperAdmin && !targetWillHaveSuperAdmin &&
            await RemovingUsersWouldRemoveLastSuperAdminAsync(
                [command.UserId], cancellationToken).ConfigureAwait(false))
        {
            response.Code = HttpStatusCode.Conflict;
            response.Message = "At least one active SuperAdmin must remain.";
            return response;
        }

        var existingRoles = await _roleDataProvider.GetRoleListAsync(
            requestedRoleIds, [], cancellationToken).ConfigureAwait(false);
        var existingRoleIds = existingRoles.Select(x => x.Id).ToHashSet();
        var nonExistingRoleIds = requestedRoleIds.Where(x => !existingRoleIds.Contains(x)).ToList();
        if (nonExistingRoleIds.Count != 0)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = $"Role IDs do not exist: {string.Join(", ", nonExistingRoleIds)}";
            return response;
        }

        var removedRoleIds = currentRoleIds.Where(x => !existingRoleIds.Contains(x)).ToList();
        if (removedRoleIds.Count != 0)
        {
            await _userRoleRep.DeleteAsync(
                x => x.UserId == command.UserId && removedRoleIds.Contains(x.RoleId),
                cancellationToken).ConfigureAwait(false);
        }

        var addedRoleIds = existingRoleIds.Where(x => !currentRoleIds.Contains(x)).ToList();
        if (addedRoleIds.Count == 0)
        {
            return response;
        }

        var newUserRoles = addedRoleIds.Select(roleId => new UserRole
        {
            UserId = command.UserId,
            RoleId = roleId
        }).ToList();
        
        await _userRoleRep.InsertManyAsync(newUserRoles, cancellationToken).ConfigureAwait(false);
        
        return response;
    }

    public async Task<UpdateUserRoleDeadlineResponse> UpdateUserRoleDeadlineAsync(
        UpdateUserRoleDeadlineCommand command, CancellationToken cancellationToken)
    {
        var response = new UpdateUserRoleDeadlineResponse();
        var userRole = await _userRoleRep.SingleOrDefaultAsync(
            x => x.UserId == command.UserId && x.RoleId == command.RoleId,
            cancellationToken).ConfigureAwait(false);
        if (userRole == null)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = "User role assignment not found.";
            return response;
        }

        if (command.RoleId == RoleConstants.SuperAdminId)
        {
            if (await IsBootstrapSuperAdminProtectedFromCurrentUserAsync(
                    command.UserId, cancellationToken).ConfigureAwait(false))
            {
                response.Code = HttpStatusCode.Forbidden;
                response.Message = "A later SuperAdmin cannot update the bootstrap SuperAdmin's deadline.";
                return response;
            }

            if (!await CurrentUserCanManageSuperAdminAsync(cancellationToken).ConfigureAwait(false))
            {
                response.Code = HttpStatusCode.Forbidden;
                response.Message = "Only an active SuperAdmin can update a SuperAdmin deadline.";
                return response;
            }

            if (command.Deadline.HasValue &&
                await RemovingUsersWouldRemoveLastSuperAdminAsync(
                    [command.UserId], cancellationToken).ConfigureAwait(false))
            {
                response.Code = HttpStatusCode.Conflict;
                response.Message = "The last active SuperAdmin cannot have an expiration deadline.";
                return response;
            }
        }

        userRole.Deadline = command.Deadline;
        await _userRoleRep.UpdateAsync(userRole, cancellationToken).ConfigureAwait(false);

        return response;
    }

    public async Task<RemoveRolesCascadeUserResponse> RemoveRolesCascadeUserAsync(
        RemoveRolesCascadeUserCommand command, CancellationToken cancellationToken)
    {
        var response = new RemoveRolesCascadeUserResponse();

        if (command.RoleIds.Contains(RoleConstants.SuperAdminId))
        {
            response.Code = HttpStatusCode.Conflict;
            response.Message = "The built-in SuperAdmin role cannot be removed.";
            return response;
        }

        await _userRoleRep.DeleteAsync(x => command.RoleIds.Contains(x.RoleId), cancellationToken).ConfigureAwait(false);

        return response;
    }

    public async Task<RemoveUsersCascadeRoleResponse> RemoveUsersCascadeRoleAsync(
        RemoveUsersCascadeRoleCommand command, CancellationToken cancellationToken)
    {
        var response = new RemoveUsersCascadeRoleResponse();

        var removesSuperAdmin = await _userRoleRep.AnyAsync(
            x => command.UserIds.Contains(x.UserId) && x.RoleId == RoleConstants.SuperAdminId,
            cancellationToken).ConfigureAwait(false);
        if (removesSuperAdmin)
        {
            var removesProtectedBootstrapSuperAdmin = false;
            foreach (var userId in command.UserIds)
            {
                if (await IsBootstrapSuperAdminProtectedFromCurrentUserAsync(
                        userId, cancellationToken).ConfigureAwait(false))
                {
                    removesProtectedBootstrapSuperAdmin = true;
                    break;
                }
            }

            if (removesProtectedBootstrapSuperAdmin)
            {
                response.Code = HttpStatusCode.Forbidden;
                response.Message = "A later SuperAdmin cannot remove the bootstrap SuperAdmin user.";
                return response;
            }

            if (!await CurrentUserCanManageSuperAdminAsync(cancellationToken).ConfigureAwait(false))
            {
                response.Code = HttpStatusCode.Forbidden;
                response.Message = "Only an active SuperAdmin can remove a SuperAdmin user.";
                return response;
            }

            if (await RemovingUsersWouldRemoveLastSuperAdminAsync(
                    command.UserIds, cancellationToken).ConfigureAwait(false))
            {
                response.Code = HttpStatusCode.Conflict;
                response.Message = "At least one active SuperAdmin must remain.";
                return response;
            }
        }

        await _userRoleRep.DeleteAsync(x => command.UserIds.Contains(x.UserId), cancellationToken).ConfigureAwait(false);
        
        return response;
    }

    public async Task<bool> CheckUserIsSuperAdminAsync(Guid userId, CancellationToken cancellationToken)
    {
        var roles = await _roleDataProvider.GetRoleListAsync(
            [RoleConstants.SuperAdminId], [], cancellationToken).ConfigureAwait(false);
        var roleIds = roles
            .Where(x => x.Status == StatusEnum.Enable)
            .Select(x => x.Id)
            .ToList();
        if (roleIds.Count == 0)
            return false;

        var now = DateTimeOffset.Now;
        var hasRole = await _userRoleRep.AnyAsync(
            x => x.UserId == userId &&
                 roleIds.Contains(x.RoleId) &&
                 (x.Deadline == null || x.Deadline > now), cancellationToken).ConfigureAwait(false);

        return hasRole;
    }

    private Task<bool> CurrentUserCanManageSuperAdminAsync(CancellationToken cancellationToken)
    {
        return _currentUser.Id.HasValue
            ? CheckUserIsSuperAdminAsync(_currentUser.Id.Value, cancellationToken)
            : Task.FromResult(false);
    }

    private async Task<bool> IsBootstrapSuperAdminProtectedFromCurrentUserAsync(
        Guid targetUserId, CancellationToken cancellationToken)
    {
        var bootstrapClaim = await _securityBootstrapClaimRep.FirstOrDefaultAsync(
            x => x.Id == RoleConstants.SuperAdminBootstrapClaim,
            cancellationToken).ConfigureAwait(false);

        return bootstrapClaim != null &&
               bootstrapClaim.UserId == targetUserId &&
               _currentUser.Id != bootstrapClaim.UserId;
    }

    private async Task<bool> RemovingUsersWouldRemoveLastSuperAdminAsync(
        List<Guid> userIds, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.Now;
        var activeSuperAdmins = await _userRoleRep.SqlQueryAsync(
            """
            SELECT *
            FROM `user_role`
            WHERE `role_id` = {0}
              AND (`deadline` IS NULL OR `deadline` > {1})
            FOR UPDATE
            """,
            RoleConstants.SuperAdminId,
            now).ConfigureAwait(false);
        var removedUserIds = userIds.ToHashSet();

        return activeSuperAdmins.Count(x => !removedUserIds.Contains(x.UserId)) < 1;
    }
}