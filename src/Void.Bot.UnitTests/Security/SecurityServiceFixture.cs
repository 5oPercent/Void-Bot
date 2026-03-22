using Shouldly;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Dto.Security;
using Void.Bot.Message.Request.Security;
using NSubstitute;

namespace Void.Bot.UnitTest.Security;

public class SecurityServiceFixture
{
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
            mockPermissionEndpointService);

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
            mockPermissionEndpointService);

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
            mockPermissionEndpointService);

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
            mockPermissionEndpointService);

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
            mockPermissionEndpointService);

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
            mockPermissionEndpointService);

        var result = await service.VerifyPermissionEndpointAsync(userId, endpoint, method, CancellationToken.None);

        result.ShouldBeFalse();
    }

    #endregion
}