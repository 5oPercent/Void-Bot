using Shouldly;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Dto.Security;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Security;
using NSubstitute;

namespace Void.Bot.UnitTest.Security;

public class SecurityServiceFixture
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IPermissionMenuService _permissionMenuService = Substitute.For<IPermissionMenuService>();

    #region GetCurrentUserPermissionsAsync

    [Fact]
    public async Task GetCurrentUserPermissionsAsync_WithoutCurrentUser_ReturnsEmptyPermissions()
    {
        var mockUserRoleService = Substitute.For<IUserRoleService>();
        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        var mockPermissionEndpointService = Substitute.For<IPermissionEndpointService>();

        var service = new SecurityService(
            mockUserRoleService,
            mockRolePermissionService,
            _permissionMenuService,
            mockPermissionEndpointService,
            _currentUser);

        var response = await service.GetCurrentUserPermissionsAsync(CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.Roles.ShouldBeEmpty();
        response.Data.Permissions.ShouldBeEmpty();
        response.Data.Menus.ShouldBeEmpty();
        await mockUserRoleService.DidNotReceive().GetRolesByUserIdAsync(
            Arg.Any<GetRolesByUserIdRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCurrentUserPermissionsAsync_WithActiveRoles_ReturnsAuthorizedData()
    {
        var userId = Guid.NewGuid();
        var activeRoleId = Guid.NewGuid();
        var expiredRoleId = Guid.NewGuid();
        var activeDeadline = DateTimeOffset.Now.AddDays(1);
        var activeRole = new RoleDto
        {
            Id = activeRoleId,
            Name = "ActiveRole",
            Description = "Active role",
            Code = "ACTIVE_ROLE",
            Status = StatusEnum.Enable
        };
        var permission = new PermissionDto
        {
            Id = Guid.NewGuid(),
            Name = "TestPermission",
            Description = "Test permission",
            Code = "TEST_PERMISSION",
            Status = StatusEnum.Enable
        };
        var menu = new MenuDto
        {
            Id = Guid.NewGuid(),
            Name = "TestMenu",
            Path = "/test",
            Status = StatusEnum.Enable
        };
        _currentUser.Id.Returns(userId);

        var mockUserRoleService = Substitute.For<IUserRoleService>();
        mockUserRoleService.GetRolesByUserIdAsync(
                Arg.Any<GetRolesByUserIdRequest>(), Arg.Any<CancellationToken>())
            .Returns(new GetRolesByUserIdResponse
            {
                Data = new GetRolesByUserIdData
                {
                    UserRoles =
                    [
                        new UserRoleDto
                        {
                            UserId = userId,
                            RoleId = activeRoleId,
                            Deadline = activeDeadline
                        },
                        new UserRoleDto
                        {
                            UserId = userId,
                            RoleId = expiredRoleId,
                            Deadline = DateTimeOffset.Now.AddDays(-1)
                        }
                    ],
                    Roles =
                    [
                        activeRole,
                        new RoleDto { Id = expiredRoleId, Name = "ExpiredRole" }
                    ]
                }
            });

        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        mockRolePermissionService.GetPermissionsByRoleIdsAsync(
                Arg.Is<GetPermissionsByRoleIdsRequest>(request =>
                    request.RoleIds.Count == 1 && request.RoleIds[0] == activeRoleId),
                Arg.Any<CancellationToken>())
            .Returns(new GetPermissionsByRoleIdsResponse { Data = [permission] });
        mockRolePermissionService.GetRolePermissionsAsync(
                Arg.Is<List<Guid>>(roleIds => roleIds.Count == 1 && roleIds[0] == activeRoleId),
                Arg.Any<CancellationToken>())
            .Returns(new List<RolePermissionDto>
            {
                new RolePermissionDto
                {
                    RoleId = activeRoleId,
                    PermissionId = permission.Id
                }
            });

        _permissionMenuService.GetMenusByPermissionIdsAsync(
                Arg.Is<GetMenusByPermissionIdsRequest>(request =>
                    request.PermissionIds.Count == 1 && request.PermissionIds[0] == permission.Id),
                Arg.Any<CancellationToken>())
            .Returns(new GetMenusByPermissionIdsResponse { Data = [menu] });
        _permissionMenuService.GetPermissionMenusAsync(
                Arg.Is<List<Guid>>(permissionIds => permissionIds.Count == 1 && permissionIds[0] == permission.Id),
                Arg.Any<CancellationToken>())
            .Returns(new List<PermissionMenuDto>
            {
                new PermissionMenuDto
                {
                    PermissionId = permission.Id,
                    MenuId = menu.Id
                }
            });

        var mockPermissionEndpointService = Substitute.For<IPermissionEndpointService>();

        var service = new SecurityService(
            mockUserRoleService,
            mockRolePermissionService,
            _permissionMenuService,
            mockPermissionEndpointService,
            _currentUser);

        var response = await service.GetCurrentUserPermissionsAsync(CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.Roles.Count.ShouldBe(1);
        response.Data.Roles[0].Id.ShouldBe(activeRole.Id);
        response.Data.Roles[0].Code.ShouldBe(activeRole.Code);
        response.Data.Roles[0].Deadline.ShouldBe(activeDeadline);
        response.Data.Permissions.Count.ShouldBe(1);
        response.Data.Permissions[0].Id.ShouldBe(permission.Id);
        response.Data.Permissions[0].Code.ShouldBe(permission.Code);
        response.Data.Permissions[0].Deadline.ShouldBe(activeDeadline);
        response.Data.Menus.Count.ShouldBe(1);
        response.Data.Menus[0].Id.ShouldBe(menu.Id);
        response.Data.Menus[0].Path.ShouldBe(menu.Path);
        response.Data.Menus[0].Deadline.ShouldBe(activeDeadline);
    }

    [Fact]
    public async Task GetCurrentUserPermissionsAsync_WithExpiredRoles_ReturnsEmptyPermissions()
    {
        var userId = Guid.NewGuid();
        _currentUser.Id.Returns(userId);

        var mockUserRoleService = Substitute.For<IUserRoleService>();
        mockUserRoleService.GetRolesByUserIdAsync(
                Arg.Any<GetRolesByUserIdRequest>(), Arg.Any<CancellationToken>())
            .Returns(new GetRolesByUserIdResponse
            {
                Data = new GetRolesByUserIdData
                {
                    UserRoles =
                    [
                        new UserRoleDto
                        {
                            UserId = userId,
                            RoleId = Guid.NewGuid(),
                            Deadline = DateTimeOffset.Now.AddDays(-1)
                        }
                    ]
                }
            });

        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        var mockPermissionEndpointService = Substitute.For<IPermissionEndpointService>();

        var service = new SecurityService(
            mockUserRoleService,
            mockRolePermissionService,
            _permissionMenuService,
            mockPermissionEndpointService,
            _currentUser);

        var response = await service.GetCurrentUserPermissionsAsync(CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.Permissions.ShouldBeEmpty();
        await mockRolePermissionService.DidNotReceive().GetPermissionsByRoleIdsAsync(
            Arg.Any<GetPermissionsByRoleIdsRequest>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region VerifyPermissionEndpointAsync

    [Fact]
    public async Task VerifyPermissionEndpointAsync_WithEmptyUserId_ReturnsFalse()
    {
        var mockUserRoleService = Substitute.For<IUserRoleService>();
        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        var mockPermissionEndpointService = Substitute.For<IPermissionEndpointService>();

        var service = new SecurityService(
            mockUserRoleService,
            mockRolePermissionService,
            _permissionMenuService,
            mockPermissionEndpointService,
            _currentUser);

        var result = await service.VerifyPermissionEndpointAsync(
            Guid.Empty, "/api/test", "GET", CancellationToken.None);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task VerifyPermissionEndpointAsync_WithEmptyEndpoint_ReturnsFalse()
    {
        var userId = Guid.NewGuid();

        var mockUserRoleService = Substitute.For<IUserRoleService>();
        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        var mockPermissionEndpointService = Substitute.For<IPermissionEndpointService>();

        var service = new SecurityService(
            mockUserRoleService,
            mockRolePermissionService,
            _permissionMenuService,
            mockPermissionEndpointService,
            _currentUser);

        var result = await service.VerifyPermissionEndpointAsync(
            userId, string.Empty, "GET", CancellationToken.None);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task VerifyPermissionEndpointAsync_WithNoUserRoles_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var endpoint = "/api/test";
        var method = "GET";

        var mockUserRoleService = Substitute.For<IUserRoleService>();
        mockUserRoleService.GetRolesByUserIdAsync(
                Arg.Any<GetRolesByUserIdRequest>(), Arg.Any<CancellationToken>())
            .Returns(new GetRolesByUserIdResponse
            {
                Data = new GetRolesByUserIdData()
            });

        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        var mockPermissionEndpointService = Substitute.For<IPermissionEndpointService>();

        var service = new SecurityService(
            mockUserRoleService,
            mockRolePermissionService,
            _permissionMenuService,
            mockPermissionEndpointService,
            _currentUser);

        var result = await service.VerifyPermissionEndpointAsync(userId, endpoint, method, CancellationToken.None);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task VerifyPermissionEndpointAsync_WithExpiredUserRole_ExcludesFromPermissions()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        var endpoint = "/api/test";
        var method = "GET";

        var userRolesResponse = new GetRolesByUserIdResponse
        {
            Data = new GetRolesByUserIdData
            {
                UserRoles = new List<UserRoleDto>
                {
                    new UserRoleDto
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        RoleId = roleId,
                        Deadline = DateTimeOffset.Now.AddDays(-1)
                    }
                },
                Roles = new List<RoleDto>
                {
                    new RoleDto { Id = roleId, Name = "TestRole" }
                }
            }
        };

        var permissionsResponse = new GetPermissionsByRoleIdsResponse
        {
            Data = new List<PermissionDto>()
        };

        var mockUserRoleService = Substitute.For<IUserRoleService>();
        mockUserRoleService.GetRolesByUserIdAsync(
                Arg.Any<GetRolesByUserIdRequest>(), Arg.Any<CancellationToken>())
            .Returns(userRolesResponse);

        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        mockRolePermissionService.GetPermissionsByRoleIdsAsync(
                Arg.Any<GetPermissionsByRoleIdsRequest>(), Arg.Any<CancellationToken>())
            .Returns(permissionsResponse);

        var mockPermissionEndpointService = Substitute.For<IPermissionEndpointService>();

        var service = new SecurityService(
            mockUserRoleService,
            mockRolePermissionService,
            _permissionMenuService,
            mockPermissionEndpointService,
            _currentUser);

        var result = await service.VerifyPermissionEndpointAsync(userId, endpoint, method, CancellationToken.None);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task VerifyPermissionEndpointAsync_WithValidPermission_ReturnsTrue()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        var endpoint = "/api/test";
        var method = "GET";

        var userRolesResponse = new GetRolesByUserIdResponse
        {
            Data = new GetRolesByUserIdData
            {
                UserRoles = new List<UserRoleDto>
                {
                    new UserRoleDto
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        RoleId = roleId,
                        Deadline = null
                    }
                },
                Roles = new List<RoleDto>
                {
                    new RoleDto { Id = roleId, Name = "TestRole" }
                }
            }
        };

        var permissionsResponse = new GetPermissionsByRoleIdsResponse
        {
            Data = new List<PermissionDto>
            {
                new PermissionDto { Id = permissionId, Name = "TestPermission" }
            }
        };

        var endpointsResponse = new GetEndpointsByPermissionIdsResponse
        {
            Data = new List<EndpointDto>
            {
                new EndpointDto { Endpoint = endpoint }
            }
        };

        var mockUserRoleService = Substitute.For<IUserRoleService>();
        mockUserRoleService.GetRolesByUserIdAsync(
                Arg.Any<GetRolesByUserIdRequest>(), Arg.Any<CancellationToken>())
            .Returns(userRolesResponse);

        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        mockRolePermissionService.GetPermissionsByRoleIdsAsync(
                Arg.Any<GetPermissionsByRoleIdsRequest>(), Arg.Any<CancellationToken>())
            .Returns(permissionsResponse);

        var mockPermissionEndpointService = Substitute.For<IPermissionEndpointService>();
        mockPermissionEndpointService.GetEndpointsByPermissionIdsAsync(
                Arg.Any<GetEndpointsByPermissionIdsRequest>(), Arg.Any<CancellationToken>())
            .Returns(endpointsResponse);

        var service = new SecurityService(
            mockUserRoleService,
            mockRolePermissionService,
            _permissionMenuService,
            mockPermissionEndpointService,
            _currentUser);

        var result = await service.VerifyPermissionEndpointAsync(userId, endpoint, method, CancellationToken.None);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task VerifyPermissionEndpointAsync_WithFutureDeadline_IncludesPermission()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        var endpoint = "/api/test";
        var method = "GET";

        var userRolesResponse = new GetRolesByUserIdResponse
        {
            Data = new GetRolesByUserIdData
            {
                UserRoles = new List<UserRoleDto>
                {
                    new UserRoleDto
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        RoleId = roleId,
                        Deadline = DateTimeOffset.Now.AddDays(30)
                    }
                },
                Roles = new List<RoleDto>
                {
                    new RoleDto { Id = roleId, Name = "TestRole" }
                }
            }
        };

        var permissionsResponse = new GetPermissionsByRoleIdsResponse
        {
            Data = new List<PermissionDto>
            {
                new PermissionDto { Id = permissionId, Name = "TestPermission" }
            }
        };

        var endpointsResponse = new GetEndpointsByPermissionIdsResponse
        {
            Data = new List<EndpointDto>
            {
                new EndpointDto { Endpoint = endpoint }
            }
        };

        var mockUserRoleService = Substitute.For<IUserRoleService>();
        mockUserRoleService.GetRolesByUserIdAsync(
                Arg.Any<GetRolesByUserIdRequest>(), Arg.Any<CancellationToken>())
            .Returns(userRolesResponse);

        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        mockRolePermissionService.GetPermissionsByRoleIdsAsync(
                Arg.Any<GetPermissionsByRoleIdsRequest>(), Arg.Any<CancellationToken>())
            .Returns(permissionsResponse);

        var mockPermissionEndpointService = Substitute.For<IPermissionEndpointService>();
        mockPermissionEndpointService.GetEndpointsByPermissionIdsAsync(
                Arg.Any<GetEndpointsByPermissionIdsRequest>(), Arg.Any<CancellationToken>())
            .Returns(endpointsResponse);

        var service = new SecurityService(
            mockUserRoleService,
            mockRolePermissionService,
            _permissionMenuService,
            mockPermissionEndpointService,
            _currentUser);

        var result = await service.VerifyPermissionEndpointAsync(userId, endpoint, method, CancellationToken.None);

        result.ShouldBeFalse();
    }

    #endregion
}