using Autofac;
using Mediator.Net;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Services.Security;
using Void.Bot.IntegrationTest.Utils;
using Void.Bot.Message.Commands.Security;
using Void.Bot.Message.Constants.Security;
using Void.Bot.Message.Dto.Security;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.IntegrationTest.Services.Security;

public class SecurityFixture(IntegrationTestBase testBase) : IntegrationFixture(testBase)
{
    private static readonly Guid TestUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private readonly SecurityUtil _util = new(testBase);
    private readonly AccountUtil _accountUtil = new(testBase);

    #region RoleService Tests

    [Fact]
    public async Task GetRoleAsync_WithExistingRole_ReturnsRole()
    {
        var roleId = await _util.AddRoleAsync();

        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.RequestAsync<GetRoleRequest, GetRoleResponse>(
                new GetRoleRequest { Id = roleId }).ConfigureAwait(false);

            response.ShouldNotBeNull();
            response.Data.ShouldNotBeNull();
            response.Data.Id.ShouldBe(roleId);
        });
    }

    [Fact]
    public async Task AddRoleAsync_WithValidParent_CreatesRole()
    {
        var parentId = await _util.AddRoleAsync();

        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.SendAsync<AddRoleCommand, AddRoleResponse>(
                new AddRoleCommand
                {
                    Pid = parentId,
                    Name = "ChildRole",
                    Code = "CHILD",
                    Status = StatusEnum.Enable
                }).ConfigureAwait(false);

            response.ShouldNotBeNull();
            response.Code.ShouldBe(System.Net.HttpStatusCode.OK);
        });
    }

    [Fact]
    public async Task GetRoleTreeAsync_WithHierarchy_ReturnsTree()
    {
        var parentId = await _util.AddRoleAsync();
        await _util.AddRoleAsync(pid: parentId);
        await _util.AddRoleAsync(pid: parentId);

        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.RequestAsync<GetRoleTreeRequest, GetRoleTreeResponse>(
                new GetRoleTreeRequest()).ConfigureAwait(false);

            response.ShouldNotBeNull();
            response.Data.ShouldNotBeNull();
            response.Data.Count.ShouldBe(1);
            response.Data[0].Children.Count.ShouldBe(2);
        });
    }

    #endregion

    #region PermissionService Tests

    [Fact]
    public async Task GetPermissionAsync_WithExistingPermission_ReturnsPermission()
    {
        var permissionId = await _util.AddPermissionAsync();

        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.RequestAsync<GetPermissionRequest, GetPermissionResponse>(
                new GetPermissionRequest { Id = permissionId }).ConfigureAwait(false);

            response.ShouldNotBeNull();
            response.Data.ShouldNotBeNull();
            response.Data.Id.ShouldBe(permissionId);
        });
    }

    [Fact]
    public async Task AddPermissionAsync_WithValidParent_CreatesPermission()
    {
        var parentId = await _util.AddPermissionAsync();

        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.SendAsync<AddPermissionCommand, AddPermissionResponse>(
                new AddPermissionCommand
                {
                    Pid = parentId,
                    Name = "ChildPermission",
                    Code = "CHILD",
                    Type = Message.Enum.Security.PermissionTypeEnum.Action,
                    Status = StatusEnum.Enable
                }).ConfigureAwait(false);

            response.ShouldNotBeNull();
            response.Code.ShouldBe(System.Net.HttpStatusCode.OK);
        });
    }

    #endregion

    #region UserRoleService Tests

    [Fact]
    public async Task AssignRolesToUserAsync_WithExistingUserAndRole_CreatesUserRole()
    {
        var userId = await _accountUtil.AddUserAccountAsync();
        var roleId = await _util.AddRoleAsync();

        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.SendAsync<AssignRolesToUserCommand, AssignRolesToUserResponse>(
                new AssignRolesToUserCommand
                {
                    UserId = userId,
                    RoleIds = [roleId]
                }).ConfigureAwait(false);

            response.ShouldNotBeNull();
            response.Code.ShouldBe(System.Net.HttpStatusCode.OK);

            var rolesResponse = await mediator.RequestAsync<GetRolesByUserIdRequest, GetRolesByUserIdResponse>(
                new GetRolesByUserIdRequest { UserId = userId }).ConfigureAwait(false);
            
            rolesResponse.Data.ShouldNotBeNull();
            rolesResponse.Data.UserRoles.Count.ShouldBe(1);
        });
    }

    [Fact]
    public async Task CheckUserIsSuperAdminAsync_WithSuperAdminRole_ReturnsTrue()
    {
        var userId = TestUserId;
        await _util.AddRoleAsync(
            RoleConstants.SuperAdmin,
            RoleConstants.SuperAdmin,
            built: true,
            id: RoleConstants.SuperAdminId);
        await _util.AddUserRoleAsync(userId, RoleConstants.SuperAdminId);

        await Run<IUserRoleService>(async service =>
        {
            var result = await service.CheckUserIsSuperAdminAsync(userId, CancellationToken.None);

            result.ShouldBeTrue();
        });
    }

    [Fact]
    public async Task CheckUserIsSuperAdminAsync_WithNormalRole_ReturnsFalse()
    {
        var userId = await _accountUtil.AddUserAccountAsync();
        var roleId = await _util.AddRoleAsync();

        await Run<IMediator>(async mediator =>
        {
            await mediator.SendAsync<AssignRolesToUserCommand, AssignRolesToUserResponse>(
                new AssignRolesToUserCommand
                {
                    UserId = userId,
                    RoleIds = [roleId]
                }).ConfigureAwait(false);
        });

        await Run<IUserRoleService>(async service =>
        {
            var result = await service.CheckUserIsSuperAdminAsync(userId, CancellationToken.None);

            result.ShouldBeFalse();
        });
    }

    [Fact]
    public async Task AssignSuperAdminRoleAsync_RequiresActiveSuperAdmin()
    {
        var targetUserId = await _accountUtil.AddUserAccountAsync();
        await _util.AddRoleAsync(
            RoleConstants.SuperAdmin,
            RoleConstants.SuperAdmin,
            built: true,
            id: RoleConstants.SuperAdminId);

        await Run<IMediator>(async mediator =>
        {
            var forbiddenResponse = await mediator.SendAsync<AssignRolesToUserCommand, AssignRolesToUserResponse>(
                new AssignRolesToUserCommand
                {
                    UserId = targetUserId,
                    RoleIds = [RoleConstants.SuperAdminId]
                }).ConfigureAwait(false);

            forbiddenResponse.Code.ShouldBe(System.Net.HttpStatusCode.Forbidden);
        });

        await _util.AddUserRoleAsync(TestUserId, RoleConstants.SuperAdminId);

        await Run<IMediator, IUserRoleService>(async (mediator, userRoleService) =>
        {
            var response = await mediator.SendAsync<AssignRolesToUserCommand, AssignRolesToUserResponse>(
                new AssignRolesToUserCommand
                {
                    UserId = targetUserId,
                    RoleIds = [RoleConstants.SuperAdminId]
                }).ConfigureAwait(false);

            response.Code.ShouldBe(System.Net.HttpStatusCode.OK);
            var roles = await userRoleService.GetRolesByUserIdAsync(
                new GetRolesByUserIdRequest { UserId = targetUserId }, CancellationToken.None);
            roles.Data.ShouldNotBeNull();
            roles.Data.UserRoles.ShouldContain(x => x.RoleId == RoleConstants.SuperAdminId);
        });
    }

    [Fact]
    public async Task UnassignSuperAdminRoleAsync_PreservesLastActiveSuperAdmin()
    {
        var otherSuperAdminId = await _accountUtil.AddUserAccountAsync();
        await _util.AddRoleAsync(
            RoleConstants.SuperAdmin,
            RoleConstants.SuperAdmin,
            built: true,
            id: RoleConstants.SuperAdminId);
        await _util.AddUserRoleAsync(otherSuperAdminId, RoleConstants.SuperAdminId);

        await Run<IMediator>(async mediator =>
        {
            var forbiddenResponse = await mediator.SendAsync<UnassignRolesFromUserCommand, UnassignRolesFromUserResponse>(
                new UnassignRolesFromUserCommand
                {
                    UserId = otherSuperAdminId,
                    RoleIds = [RoleConstants.SuperAdminId]
                }).ConfigureAwait(false);
            forbiddenResponse.Code.ShouldBe(System.Net.HttpStatusCode.Forbidden);
        });

        await _util.AddUserRoleAsync(TestUserId, RoleConstants.SuperAdminId);

        await Run<IMediator, IUserRoleService>(async (mediator, userRoleService) =>
        {
            var removeOtherResponse = await mediator.SendAsync<UnassignRolesFromUserCommand, UnassignRolesFromUserResponse>(
                new UnassignRolesFromUserCommand
                {
                    UserId = otherSuperAdminId,
                    RoleIds = [RoleConstants.SuperAdminId]
                }).ConfigureAwait(false);
            removeOtherResponse.Code.ShouldBe(System.Net.HttpStatusCode.OK);

            var removeLastResponse = await mediator.SendAsync<UnassignRolesFromUserCommand, UnassignRolesFromUserResponse>(
                new UnassignRolesFromUserCommand
                {
                    UserId = TestUserId,
                    RoleIds = [RoleConstants.SuperAdminId]
                }).ConfigureAwait(false);
            removeLastResponse.Code.ShouldBe(System.Net.HttpStatusCode.Conflict);
            (await userRoleService.CheckUserIsSuperAdminAsync(
                TestUserId, CancellationToken.None)).ShouldBeTrue();
        });
    }

    [Fact]
    public async Task UpdateUserRoleDeadlineAsync_ProtectsSuperAdminAssignments()
    {
        var otherSuperAdminId = await _accountUtil.AddUserAccountAsync();
        await _util.AddRoleAsync(
            RoleConstants.SuperAdmin,
            RoleConstants.SuperAdmin,
            built: true,
            id: RoleConstants.SuperAdminId);
        await _util.AddUserRoleAsync(otherSuperAdminId, RoleConstants.SuperAdminId);
        var deadline = DateTimeOffset.Now.AddDays(30);

        await Run<IMediator>(async mediator =>
        {
            var forbiddenResponse = await mediator.SendAsync<UpdateUserRoleDeadlineCommand, UpdateUserRoleDeadlineResponse>(
                new UpdateUserRoleDeadlineCommand
                {
                    UserId = otherSuperAdminId,
                    RoleId = RoleConstants.SuperAdminId,
                    Deadline = deadline
                }).ConfigureAwait(false);
            forbiddenResponse.Code.ShouldBe(System.Net.HttpStatusCode.Forbidden);
        });

        await _util.AddUserRoleAsync(TestUserId, RoleConstants.SuperAdminId);

        await Run<IMediator, IUserRoleService>(async (mediator, userRoleService) =>
        {
            var response = await mediator.SendAsync<UpdateUserRoleDeadlineCommand, UpdateUserRoleDeadlineResponse>(
                new UpdateUserRoleDeadlineCommand
                {
                    UserId = otherSuperAdminId,
                    RoleId = RoleConstants.SuperAdminId,
                    Deadline = deadline
                }).ConfigureAwait(false);
            response.Code.ShouldBe(System.Net.HttpStatusCode.OK);

            var roles = await userRoleService.GetRolesByUserIdAsync(
                new GetRolesByUserIdRequest { UserId = otherSuperAdminId }, CancellationToken.None);
            roles.Data.ShouldNotBeNull();
            roles.Data.UserRoles.Single(x => x.RoleId == RoleConstants.SuperAdminId)
                .Deadline.ShouldNotBeNull();
        });
    }

    [Fact]
    public async Task UpdateUserRoleDeadlineAsync_RejectsExpirationForLastActiveSuperAdmin()
    {
        await _util.AddRoleAsync(
            RoleConstants.SuperAdmin,
            RoleConstants.SuperAdmin,
            built: true,
            id: RoleConstants.SuperAdminId);
        await _util.AddUserRoleAsync(TestUserId, RoleConstants.SuperAdminId);

        await Run<IMediator, IUserRoleService>(async (mediator, userRoleService) =>
        {
            var response = await mediator.SendAsync<UpdateUserRoleDeadlineCommand, UpdateUserRoleDeadlineResponse>(
                new UpdateUserRoleDeadlineCommand
                {
                    UserId = TestUserId,
                    RoleId = RoleConstants.SuperAdminId,
                    Deadline = DateTimeOffset.Now.AddDays(30)
                }).ConfigureAwait(false);
            response.Code.ShouldBe(System.Net.HttpStatusCode.Conflict);

            var roles = await userRoleService.GetRolesByUserIdAsync(
                new GetRolesByUserIdRequest { UserId = TestUserId }, CancellationToken.None);
            roles.Data.ShouldNotBeNull();
            roles.Data.UserRoles.Single(x => x.RoleId == RoleConstants.SuperAdminId)
                .Deadline.ShouldBeNull();
        });
    }

    [Fact]
    public async Task LaterSuperAdminCannotModifyBootstrapSuperAdminAssignment()
    {
        var bootstrapSuperAdminId = await _accountUtil.AddUserAccountAsync();
        await _util.AddRoleAsync(
            RoleConstants.SuperAdmin,
            RoleConstants.SuperAdmin,
            built: true,
            id: RoleConstants.SuperAdminId);
        await _util.AddSuperAdminBootstrapClaimAsync(bootstrapSuperAdminId);
        await _util.AddUserRoleAsync(bootstrapSuperAdminId, RoleConstants.SuperAdminId);
        await _util.AddUserRoleAsync(TestUserId, RoleConstants.SuperAdminId);

        await Run<IMediator, IUserRoleService>(async (mediator, userRoleService) =>
        {
            var unassignResponse = await mediator.SendAsync<UnassignRolesFromUserCommand, UnassignRolesFromUserResponse>(
                new UnassignRolesFromUserCommand
                {
                    UserId = bootstrapSuperAdminId,
                    RoleIds = [RoleConstants.SuperAdminId]
                }).ConfigureAwait(false);
            unassignResponse.Code.ShouldBe(System.Net.HttpStatusCode.Forbidden);

            var deadlineResponse = await mediator.SendAsync<UpdateUserRoleDeadlineCommand, UpdateUserRoleDeadlineResponse>(
                new UpdateUserRoleDeadlineCommand
                {
                    UserId = bootstrapSuperAdminId,
                    RoleId = RoleConstants.SuperAdminId,
                    Deadline = DateTimeOffset.Now.AddDays(30)
                }).ConfigureAwait(false);
            deadlineResponse.Code.ShouldBe(System.Net.HttpStatusCode.Forbidden);

            var replaceResponse = await mediator.SendAsync<ReplaceUserRolesCommand, ReplaceUserRolesResponse>(
                new ReplaceUserRolesCommand
                {
                    UserId = bootstrapSuperAdminId,
                    RoleIds = []
                }).ConfigureAwait(false);
            replaceResponse.Code.ShouldBe(System.Net.HttpStatusCode.Forbidden);

            var roles = await userRoleService.GetRolesByUserIdAsync(
                new GetRolesByUserIdRequest { UserId = bootstrapSuperAdminId }, CancellationToken.None);
            roles.Data.ShouldNotBeNull();
            roles.Data.UserRoles.Single(x => x.RoleId == RoleConstants.SuperAdminId)
                .Deadline.ShouldBeNull();
        });
    }

    [Fact]
    public async Task ReplaceUserRolesAsync_PreservesDeadlineForUnchangedRole()
    {
        var userId = await _accountUtil.AddUserAccountAsync();
        var existingRoleId = await _util.AddRoleAsync();
        var addedRoleId = await _util.AddRoleAsync();
        await _util.AddUserRoleAsync(userId, existingRoleId, DateTimeOffset.Now.AddDays(30));

        await Run<IMediator, IUserRoleService>(async (mediator, userRoleService) =>
        {
            var response = await mediator.SendAsync<ReplaceUserRolesCommand, ReplaceUserRolesResponse>(
                new ReplaceUserRolesCommand
                {
                    UserId = userId,
                    RoleIds = [existingRoleId, addedRoleId]
                }).ConfigureAwait(false);
            response.Code.ShouldBe(System.Net.HttpStatusCode.OK);

            var roles = await userRoleService.GetRolesByUserIdAsync(
                new GetRolesByUserIdRequest { UserId = userId }, CancellationToken.None);
            roles.Data.ShouldNotBeNull();
            roles.Data.UserRoles.Single(x => x.RoleId == existingRoleId)
                .Deadline.ShouldNotBeNull();
            roles.Data.UserRoles.Single(x => x.RoleId == addedRoleId)
                .Deadline.ShouldBeNull();
        });
    }

    [Fact]
    public async Task UnassignRolesFromUserAsync_WithExistingUserRole_DeletesUserRole()
    {
        var userId = await _accountUtil.AddUserAccountAsync();
        var roleId = await _util.AddRoleAsync();

        await Run<IMediator>(async mediator =>
        {
            await mediator.SendAsync<AssignRolesToUserCommand, AssignRolesToUserResponse>(
                new AssignRolesToUserCommand
                {
                    UserId = userId,
                    RoleIds = [roleId]
                }).ConfigureAwait(false);

            var response = await mediator.SendAsync<UnassignRolesFromUserCommand, UnassignRolesFromUserResponse>(
                new UnassignRolesFromUserCommand
                {
                    UserId = userId,
                    RoleIds = [roleId]
                }).ConfigureAwait(false);

            response.ShouldNotBeNull();
            response.Code.ShouldBe(System.Net.HttpStatusCode.OK);
        });
    }

    #endregion

    #region RolePermissionService Tests

    [Fact]
    public async Task AssignPermissionsToRoleAsync_WithExistingRoleAndPermission_CreatesRolePermission()
    {
        var roleId = await _util.AddRoleAsync();
        var permissionId = await _util.AddPermissionAsync();

        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.SendAsync<AssignPermissionsToRoleCommand, AssignPermissionsToRoleResponse>(
                new AssignPermissionsToRoleCommand
                {
                    RoleId = roleId,
                    PermissionIds = [permissionId]
                }).ConfigureAwait(false);

            response.ShouldNotBeNull();
            response.Code.ShouldBe(System.Net.HttpStatusCode.OK);

            var hasPermission = await mediator.RequestAsync<GetRoleHasPermissionRequest, GetRoleHasPermissionResponse>(
                new GetRoleHasPermissionRequest { RoleId = roleId, PermissionId = permissionId }).ConfigureAwait(false);
            hasPermission.Data.ShouldBeTrue();
        });
    }

    [Fact]
    public async Task ReplaceRolePermissionsAsync_WithNewPermissions_ReplacesRolePermissions()
    {
        var roleId = await _util.AddRoleAsync();
        var oldPermissionId = await _util.AddPermissionAsync();
        var newPermissionId = await _util.AddPermissionAsync();

        await Run<IMediator>(async mediator =>
        {
            await mediator.SendAsync<AssignPermissionsToRoleCommand, AssignPermissionsToRoleResponse>(
                new AssignPermissionsToRoleCommand
                {
                    RoleId = roleId,
                    PermissionIds = [oldPermissionId]
                }).ConfigureAwait(false);

            var response = await mediator.SendAsync<ReplaceRolePermissionsCommand, ReplaceRolePermissionsResponse>(
                new ReplaceRolePermissionsCommand
                {
                    RoleId = roleId,
                    PermissionIds = [newPermissionId]
                }).ConfigureAwait(false);

            response.ShouldNotBeNull();
            response.Code.ShouldBe(System.Net.HttpStatusCode.OK);

            var hasOldPermission = await mediator.RequestAsync<GetRoleHasPermissionRequest, GetRoleHasPermissionResponse>(
                new GetRoleHasPermissionRequest { RoleId = roleId, PermissionId = oldPermissionId }).ConfigureAwait(false);
            hasOldPermission.Data.ShouldBeFalse();

            var hasNewPermission = await mediator.RequestAsync<GetRoleHasPermissionRequest, GetRoleHasPermissionResponse>(
                new GetRoleHasPermissionRequest { RoleId = roleId, PermissionId = newPermissionId }).ConfigureAwait(false);
            hasNewPermission.Data.ShouldBeTrue();
        });
    }

    #endregion

    #region SecurityService Tests

    [Fact]
    public async Task GetCurrentUserPermissionsAsync_ReturnsSecurityContextFromActiveRoles()
    {
        var activeRoleId = await _util.AddRoleAsync();
        var earlierRoleId = await _util.AddRoleAsync();
        var expiredRoleId = await _util.AddRoleAsync();
        var activePermissionId = await _util.AddPermissionAsync();
        var expiredPermissionId = await _util.AddPermissionAsync();
        var menuId = await _util.AddMenuAsync();
        var disabledMenuId = await _util.AddMenuAsync(status: StatusEnum.Disable);
        var activeDeadline = DateTimeOffset.UtcNow.AddDays(30);
        var earlierDeadline = DateTimeOffset.UtcNow.AddDays(10);

        await _util.AddUserRoleAsync(TestUserId, activeRoleId, activeDeadline);
        await _util.AddUserRoleAsync(TestUserId, earlierRoleId, earlierDeadline);
        await _util.AddUserRoleAsync(TestUserId, expiredRoleId, DateTimeOffset.Now.AddDays(-1));
        await _util.AddRolePermissionAsync(activeRoleId, activePermissionId);
        await _util.AddRolePermissionAsync(earlierRoleId, activePermissionId);
        await _util.AddRolePermissionAsync(expiredRoleId, expiredPermissionId);
        await _util.AddPermissionMenuAsync(activePermissionId, menuId);
        await _util.AddPermissionMenuAsync(activePermissionId, disabledMenuId);

        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.RequestAsync<GetCurrentUserPermissionsRequest, GetCurrentUserPermissionsResponse>(
                new GetCurrentUserPermissionsRequest()).ConfigureAwait(false);

            response.Data.ShouldNotBeNull();
            response.Data.Roles.Count.ShouldBe(2);
            var activeRole = response.Data.Roles.Single(x => x.Id == activeRoleId);
            activeRole.Deadline.ShouldNotBeNull();
            Math.Abs(activeRole.Deadline.Value.ToUnixTimeSeconds() - activeDeadline.ToUnixTimeSeconds())
                .ShouldBeLessThanOrEqualTo(2);
            var earlierRole = response.Data.Roles.Single(x => x.Id == earlierRoleId);
            earlierRole.Deadline.ShouldNotBeNull();
            Math.Abs(earlierRole.Deadline.Value.ToUnixTimeSeconds() - earlierDeadline.ToUnixTimeSeconds())
                .ShouldBeLessThanOrEqualTo(2);
            response.Data.Permissions.Count.ShouldBe(1);
            response.Data.Permissions[0].Id.ShouldBe(activePermissionId);
            var permissionDeadline = response.Data.Permissions[0].Deadline;
            permissionDeadline.ShouldNotBeNull();
            Math.Abs(permissionDeadline.Value.ToUnixTimeSeconds() - activeDeadline.ToUnixTimeSeconds())
                .ShouldBeLessThanOrEqualTo(2);
            response.Data.Menus.Count.ShouldBe(1);
            response.Data.Menus[0].Id.ShouldBe(menuId);
            var menuDeadline = response.Data.Menus[0].Deadline;
            menuDeadline.ShouldNotBeNull();
            Math.Abs(menuDeadline.Value.ToUnixTimeSeconds() - activeDeadline.ToUnixTimeSeconds())
                .ShouldBeLessThanOrEqualTo(2);
        });
    }

    [Fact]
    public async Task GetCurrentUserPermissionsAsync_WithSuperAdmin_ReturnsAllEnabledPermissionsAndMenus()
    {
        await _util.AddRoleAsync(
            RoleConstants.SuperAdmin,
            RoleConstants.SuperAdmin,
            built: true,
            id: RoleConstants.SuperAdminId);
        var permissionId = await _util.AddPermissionAsync();
        var menuId = await _util.AddMenuAsync();
        var disabledMenuId = await _util.AddMenuAsync(status: StatusEnum.Disable);
        await _util.AddUserRoleAsync(TestUserId, RoleConstants.SuperAdminId);

        await Run<IMediator, IUserRoleService>(async (mediator, userRoleService) =>
        {
            var response = await mediator.RequestAsync<GetCurrentUserPermissionsRequest, GetCurrentUserPermissionsResponse>(
                new GetCurrentUserPermissionsRequest()).ConfigureAwait(false);

            response.Data.ShouldNotBeNull();
            response.Data.Roles.Single().Id.ShouldBe(RoleConstants.SuperAdminId);
            response.Data.Permissions.Single().Id.ShouldBe(permissionId);
            response.Data.Menus.Single().Id.ShouldBe(menuId);
            response.Data.Menus.ShouldNotContain(x => x.Id == disabledMenuId);
            (await userRoleService.CheckUserIsSuperAdminAsync(TestUserId, CancellationToken.None)).ShouldBeTrue();
        });
    }

    [Fact]
    public async Task VerifyPermissionEndpointAsync_WithUnauthorizedEndpoint_ReturnsFalse()
    {
        var userId = TestUserId;

        await Run<ISecurityService>(async service =>
        {
            var result = await service.VerifyPermissionEndpointAsync(
                userId, "/api/unauthorized", "GET", CancellationToken.None);

            result.ShouldBeFalse();
        });
    }

    [Fact]
    public async Task VerifyPermissionEndpointAsync_WithAuthorizedEndpoint_ReturnsTrue()
    {
        var userId = TestUserId;
        var roleId = await _util.AddRoleAsync();
        var permissionId = await _util.AddPermissionAsync();
        var endpoint = "/api/authorized";

        await _util.AddUserRoleAsync(userId, roleId);
        await _util.AddRolePermissionAsync(roleId, permissionId);
        await _util.AddPermissionEndpointAsync(permissionId, endpoint);

        var mockEndpointService = Substitute.For<IEndpointService>();
        mockEndpointService.GetEndpointListAsync(Arg.Any<GetEndpointListRequest>())
            .Returns(new GetEndpointListResponse
            {
                Data = [new EndpointDto { Endpoint = endpoint, Method = "GET" }]
            });

        await Run<ISecurityService>(async service =>
        {
            var result = await service.VerifyPermissionEndpointAsync(
                userId, endpoint, "GET", CancellationToken.None);

            result.ShouldBeTrue();
        }, cb => cb.RegisterInstance(mockEndpointService).As<IEndpointService>());
    }

    [Fact]
    public async Task VerifyPermissionEndpointAsync_WithExpiredRole_ReturnsFalse()
    {
        var userId = TestUserId;
        var roleId = await _util.AddRoleAsync();
        var permissionId = await _util.AddPermissionAsync();
        var endpoint = "/api/authorized";

        await _util.AddUserRoleAsync(userId, roleId, DateTimeOffset.Now.AddDays(-1));
        await _util.AddRolePermissionAsync(roleId, permissionId);
        await _util.AddPermissionEndpointAsync(permissionId, endpoint);

        var mockEndpointService = Substitute.For<IEndpointService>();
        mockEndpointService.GetEndpointListAsync(Arg.Any<GetEndpointListRequest>())
            .Returns(new GetEndpointListResponse
            {
                Data = [new EndpointDto { Endpoint = endpoint, Method = "GET" }]
            });

        await Run<ISecurityService>(async service =>
        {
            var result = await service.VerifyPermissionEndpointAsync(
                userId, endpoint, "GET", CancellationToken.None);

            result.ShouldBeFalse();
        }, cb => cb.RegisterInstance(mockEndpointService).As<IEndpointService>());
    }

    [Fact]
    public async Task VerifyPermissionEndpointAsync_WithFutureDeadline_ReturnsTrue()
    {
        var userId = TestUserId;
        var roleId = await _util.AddRoleAsync();
        var permissionId = await _util.AddPermissionAsync();
        var endpoint = "/api/authorized";

        await _util.AddUserRoleAsync(userId, roleId, DateTimeOffset.Now.AddDays(30));
        await _util.AddRolePermissionAsync(roleId, permissionId);
        await _util.AddPermissionEndpointAsync(permissionId, endpoint);

        var mockEndpointService = Substitute.For<IEndpointService>();
        mockEndpointService.GetEndpointListAsync(Arg.Any<GetEndpointListRequest>())
            .Returns(new GetEndpointListResponse
            {
                Data = [new EndpointDto { Endpoint = endpoint, Method = "GET" }]
            });

        await Run<ISecurityService>(async service =>
        {
            var result = await service.VerifyPermissionEndpointAsync(
                userId, endpoint, "GET", CancellationToken.None);

            result.ShouldBeTrue();
        }, cb => cb.RegisterInstance(mockEndpointService).As<IEndpointService>());
    }

    #endregion
}
