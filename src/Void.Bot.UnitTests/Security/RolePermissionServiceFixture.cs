using Shouldly;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Security;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;
using Void.Bot.Message.Dto.Security;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Security;
using Void.Bot.Message.Request.Security;
using NSubstitute;
using System.Linq.Expressions;

namespace Void.Bot.UnitTest.Security;

public class RolePermissionServiceFixture
{
    #region GetPermissionsByRoleIdsAsync

    [Fact]
    public async Task GetPermissionsByRoleIdsAsync_WithValidRoleIds_ReturnsPermissions()
    {
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var rolePermissions = new List<RolePermission>
        {
            new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                PermissionId = permissionId,
                CreatedAt = DateTimeOffset.Now,
                CreatedBy = Guid.NewGuid()
            }
        };

        var permissions = new List<Permission>
        {
            new Permission
            {
                Id = permissionId,
                Pid = Guid.Empty,
                Name = "TestPermission",
                Code = "TEST_PERMISSION",
                Type = PermissionTypeEnum.Action,
                Status = StatusEnum.Enable
            }
        };

        var mockRolePermissionRep = Substitute.For<IRepository<RolePermission>>();
        var asyncQueryable = new AsyncQueryableWrapper<RolePermission>(rolePermissions);
        mockRolePermissionRep.QueryNoTracking().Returns(asyncQueryable);

        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        mockPermissionDataProvider.GetPermissionsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(permissions);

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        mockMapper.Map<List<PermissionDto>>(Arg.Any<List<Permission>>())
            .Returns(permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Type = p.Type
            }).ToList());

        var service = new RolePermissionService(
            mockMapper,
            mockRoleDataProvider,
            mockPermissionDataProvider,
            mockRolePermissionRep);

        var response = await service.GetPermissionsByRoleIdsAsync(
            new GetPermissionsByRoleIdsRequest { RoleIds = [roleId] }, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(1);
        response.Data[0].Id.ShouldBe(permissionId);
    }

    #endregion

    #region GetRoleHasPermissionAsync

    [Fact]
    public async Task GetRoleHasPermissionAsync_WithExistingPermission_ReturnsTrue()
    {
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var rolePermissions = new List<RolePermission>
        {
            new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                PermissionId = permissionId
            }
        };

        var mockRolePermissionRep = Substitute.For<IRepository<RolePermission>>();
        var asyncQueryable = new AsyncQueryableWrapper<RolePermission>(rolePermissions);
        mockRolePermissionRep.QueryNoTracking().Returns(asyncQueryable);
        mockRolePermissionRep.AnyAsync(Arg.Any<Expression<Func<RolePermission, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        var mockMapper = Substitute.For<AutoMapper.IMapper>();

        var service = new RolePermissionService(
            mockMapper,
            mockRoleDataProvider,
            mockPermissionDataProvider,
            mockRolePermissionRep);

        var response = await service.GetRoleHasPermissionAsync(
            new GetRoleHasPermissionRequest { RoleId = roleId, PermissionId = permissionId }, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Data.ShouldBeTrue();
    }

    [Fact]
    public async Task GetRoleHasPermissionAsync_WithNonExistingPermission_ReturnsFalse()
    {
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var emptyRolePermissions = new List<RolePermission>();

        var mockRolePermissionRep = Substitute.For<IRepository<RolePermission>>();
        var asyncQueryable = new AsyncQueryableWrapper<RolePermission>(emptyRolePermissions);
        mockRolePermissionRep.QueryNoTracking().Returns(asyncQueryable);
        mockRolePermissionRep.AnyAsync(Arg.Any<Expression<Func<RolePermission, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        var mockMapper = Substitute.For<AutoMapper.IMapper>();

        var service = new RolePermissionService(
            mockMapper,
            mockRoleDataProvider,
            mockPermissionDataProvider,
            mockRolePermissionRep);

        var response = await service.GetRoleHasPermissionAsync(
            new GetRoleHasPermissionRequest { RoleId = roleId, PermissionId = permissionId }, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Data.ShouldBeFalse();
    }

    #endregion

    #region AssignPermissionsToRoleAsync

    [Fact]
    public async Task AssignPermissionsToRoleAsync_WithNonExistentRole_ReturnsBadRequest()
    {
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var command = new AssignPermissionsToRoleCommand
        {
            RoleId = roleId,
            PermissionIds = [permissionId]
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetRoleAsync(roleId, Arg.Any<CancellationToken>())
            .Returns((Role?)null);

        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        var mockRolePermissionRep = Substitute.For<IRepository<RolePermission>>();
        var mockMapper = Substitute.For<AutoMapper.IMapper>();

        var service = new RolePermissionService(
            mockMapper,
            mockRoleDataProvider,
            mockPermissionDataProvider,
            mockRolePermissionRep);

        var response = await service.AssignPermissionsToRoleAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        response.Message.ShouldContain("does not exist");
    }

    [Fact]
    public async Task AssignPermissionsToRoleAsync_WithNonExistentPermission_ReturnsBadRequest()
    {
        var roleId = Guid.NewGuid();
        var nonExistentPermissionId = Guid.NewGuid();

        var command = new AssignPermissionsToRoleCommand
        {
            RoleId = roleId,
            PermissionIds = [nonExistentPermissionId]
        };

        var role = new Role
        {
            Id = roleId,
            Pid = Guid.Empty,
            Name = "TestRole",
            Code = "TEST",
            Status = StatusEnum.Enable
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetRoleAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);

        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        mockPermissionDataProvider.GetPermissionsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Permission>());

        var mockRolePermissionRep = Substitute.For<IRepository<RolePermission>>();
        var mockMapper = Substitute.For<AutoMapper.IMapper>();

        var service = new RolePermissionService(
            mockMapper,
            mockRoleDataProvider,
            mockPermissionDataProvider,
            mockRolePermissionRep);

        var response = await service.AssignPermissionsToRoleAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        response.Message.ShouldContain("do not exist");
    }

    [Fact]
    public async Task AssignPermissionsToRoleAsync_WithAlreadyAssignedPermission_ReturnsBadRequest()
    {
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var command = new AssignPermissionsToRoleCommand
        {
            RoleId = roleId,
            PermissionIds = [permissionId]
        };

        var role = new Role { Id = roleId, Pid = Guid.Empty, Name = "TestRole", Code = "TEST", Status = StatusEnum.Enable };
        var permissions = new List<Permission> { new Permission { Id = permissionId, Pid = Guid.Empty, Name = "Test", Code = "T", Status = StatusEnum.Enable } };
        var existingRolePermissions = new List<RolePermission>
        {
            new RolePermission { Id = Guid.NewGuid(), RoleId = roleId, PermissionId = permissionId }
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetRoleAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);

        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        mockPermissionDataProvider.GetPermissionsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(permissions);

        var mockRolePermissionRep = Substitute.For<IRepository<RolePermission>>();
        var asyncQueryable = new AsyncQueryableWrapper<RolePermission>(existingRolePermissions);
        mockRolePermissionRep.QueryNoTracking().Returns(asyncQueryable);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();

        var service = new RolePermissionService(
            mockMapper,
            mockRoleDataProvider,
            mockPermissionDataProvider,
            mockRolePermissionRep);

        var response = await service.AssignPermissionsToRoleAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        response.Message.ShouldContain("already assigned");
    }

    [Fact]
    public async Task AssignPermissionsToRoleAsync_WithValidData_CreatesRolePermissions()
    {
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var command = new AssignPermissionsToRoleCommand
        {
            RoleId = roleId,
            PermissionIds = [permissionId]
        };

        var role = new Role { Id = roleId, Pid = Guid.Empty, Name = "TestRole", Code = "TEST", Status = StatusEnum.Enable };
        var permissions = new List<Permission> { new Permission { Id = permissionId, Pid = Guid.Empty, Name = "Test", Code = "T", Status = StatusEnum.Enable } };
        var existingRolePermissions = new List<RolePermission>();

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetRoleAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);

        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        mockPermissionDataProvider.GetPermissionsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(permissions);

        var mockRolePermissionRep = Substitute.For<IRepository<RolePermission>>();
        var asyncQueryable = new AsyncQueryableWrapper<RolePermission>(existingRolePermissions);
        mockRolePermissionRep.QueryNoTracking().Returns(asyncQueryable);
        mockRolePermissionRep.InsertManyAsync(Arg.Any<List<RolePermission>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();

        var service = new RolePermissionService(
            mockMapper,
            mockRoleDataProvider,
            mockPermissionDataProvider,
            mockRolePermissionRep);

        var response = await service.AssignPermissionsToRoleAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        await mockRolePermissionRep.Received().InsertManyAsync(
            Arg.Is<List<RolePermission>>(rps => rps.Count == 1 && rps[0].RoleId == roleId && rps[0].PermissionId == permissionId),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region UnassignPermissionsFromRoleAsync

    [Fact]
    public async Task UnassignPermissionsFromRoleAsync_WithValidData_DeletesRolePermissions()
    {
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var command = new UnassignPermissionsFromRoleCommand
        {
            RoleId = roleId,
            PermissionIds = [permissionId]
        };

        var mockRolePermissionRep = Substitute.For<IRepository<RolePermission>>();
        mockRolePermissionRep.DeleteAsync(Arg.Any<Expression<Func<RolePermission, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        var mockMapper = Substitute.For<AutoMapper.IMapper>();

        var service = new RolePermissionService(
            mockMapper,
            mockRoleDataProvider,
            mockPermissionDataProvider,
            mockRolePermissionRep);

        var response = await service.UnassignPermissionsFromRoleAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        await mockRolePermissionRep.Received().DeleteAsync(
            Arg.Any<Expression<Func<RolePermission, bool>>>(),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region ReplaceRolePermissionsAsync

    [Fact]
    public async Task ReplaceRolePermissionsAsync_WithNonExistentRole_ReturnsBadRequest()
    {
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var command = new ReplaceRolePermissionsCommand
        {
            RoleId = roleId,
            PermissionIds = [permissionId]
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetRoleAsync(roleId, Arg.Any<CancellationToken>())
            .Returns((Role?)null);

        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        var mockRolePermissionRep = Substitute.For<IRepository<RolePermission>>();
        var mockMapper = Substitute.For<AutoMapper.IMapper>();

        var service = new RolePermissionService(
            mockMapper,
            mockRoleDataProvider,
            mockPermissionDataProvider,
            mockRolePermissionRep);

        var response = await service.ReplaceRolePermissionsAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        response.Message.ShouldContain("does not exist");
    }

    [Fact]
    public async Task ReplaceRolePermissionsAsync_WithValidData_ReplacesRolePermissions()
    {
        var roleId = Guid.NewGuid();
        var newPermissionId = Guid.NewGuid();

        var command = new ReplaceRolePermissionsCommand
        {
            RoleId = roleId,
            PermissionIds = [newPermissionId]
        };

        var role = new Role { Id = roleId, Pid = Guid.Empty, Name = "TestRole", Code = "TEST", Status = StatusEnum.Enable };
        var permissions = new List<Permission>
        {
            new Permission { Id = newPermissionId, Pid = Guid.Empty, Name = "NewPermission", Code = "NEW", Status = StatusEnum.Enable }
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetRoleAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);

        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        mockPermissionDataProvider.GetPermissionsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(permissions);

        var mockRolePermissionRep = Substitute.For<IRepository<RolePermission>>();
        mockRolePermissionRep.DeleteAsync(Arg.Any<Expression<Func<RolePermission, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        mockRolePermissionRep.InsertManyAsync(Arg.Any<List<RolePermission>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();

        var service = new RolePermissionService(
            mockMapper,
            mockRoleDataProvider,
            mockPermissionDataProvider,
            mockRolePermissionRep);

        var response = await service.ReplaceRolePermissionsAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        await mockRolePermissionRep.Received().DeleteAsync(
            Arg.Any<Expression<Func<RolePermission, bool>>>(),
            Arg.Any<CancellationToken>());
        await mockRolePermissionRep.Received().InsertManyAsync(
            Arg.Is<List<RolePermission>>(rps => rps.Count == 1 && rps[0].PermissionId == newPermissionId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReplaceRolePermissionsAsync_WithEmptyPermissions_DeletesAllRolePermissions()
    {
        var roleId = Guid.NewGuid();

        var command = new ReplaceRolePermissionsCommand
        {
            RoleId = roleId,
            PermissionIds = []
        };

        var role = new Role { Id = roleId, Pid = Guid.Empty, Name = "TestRole", Code = "TEST", Status = StatusEnum.Enable };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetRoleAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);

        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        var mockRolePermissionRep = Substitute.For<IRepository<RolePermission>>();
        mockRolePermissionRep.DeleteAsync(Arg.Any<Expression<Func<RolePermission, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();

        var service = new RolePermissionService(
            mockMapper,
            mockRoleDataProvider,
            mockPermissionDataProvider,
            mockRolePermissionRep);

        var response = await service.ReplaceRolePermissionsAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        await mockRolePermissionRep.Received().DeleteAsync(
            Arg.Any<Expression<Func<RolePermission, bool>>>(),
            Arg.Any<CancellationToken>());
        await mockRolePermissionRep.DidNotReceive().InsertManyAsync(
            Arg.Any<List<RolePermission>>(),
            Arg.Any<CancellationToken>());
    }

    #endregion
}