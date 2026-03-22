using Shouldly;
using Void.Bot.Core.Domain.Security;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Security;
using NSubstitute;
using System.Net;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.UnitTest.Security;

public class RoleServiceFixture
{
    #region GetRoleAsync

    [Fact]
    public async Task GetRoleAsync_WithValidId_ReturnsRole()
    {
        var roleId = Guid.NewGuid();
        var role = new Role
        {
            Id = roleId,
            Pid = Guid.Empty,
            Name = "TestRole",
            Code = "TEST_ROLE",
            Description = "Test Description",
            Sequence = 1,
            Status = StatusEnum.Enable,
            Built = false,
            CreatedAt = DateTimeOffset.Now,
            CreatedBy = Guid.NewGuid()
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetRoleAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(role);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        mockMapper.Map<RoleDto>(role)
            .Returns(new RoleDto
            {
                Id = role.Id,
                Pid = role.Pid,
                Name = role.Name,
                Code = role.Code,
                Description = role.Description,
                Sequence = role.Sequence,
                Status = role.Status,
                Built = role.Built
            });

        var mockUserRoleService = Substitute.For<IUserRoleService>();
        var mockRolePermissionService = Substitute.For<IRolePermissionService>();

        var service = new RoleService(
            mockMapper,
            mockRoleDataProvider,
            mockUserRoleService,
            mockRolePermissionService);

        var response = await service.GetRoleAsync(new GetRoleRequest { Id = roleId }, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Data.ShouldNotBeNull();
        response.Data.Id.ShouldBe(roleId);
        response.Data.Name.ShouldBe("TestRole");
    }

    [Fact]
    public async Task GetRoleAsync_WithNonExistentId_ReturnsNullData()
    {
        var nonExistentId = Guid.NewGuid();

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetRoleAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((Role?)null);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        var mockUserRoleService = Substitute.For<IUserRoleService>();
        var mockRolePermissionService = Substitute.For<IRolePermissionService>();

        var service = new RoleService(
            mockMapper,
            mockRoleDataProvider,
            mockUserRoleService,
            mockRolePermissionService);

        var response = await service.GetRoleAsync(new GetRoleRequest { Id = nonExistentId }, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Data.ShouldBeNull();
    }

    #endregion

    #region GetRoleTreeAsync

    [Fact]
    public async Task GetRoleTreeAsync_WithRoles_ReturnsTreeStructure()
    {
        var parentRole = new Role
        {
            Id = Guid.NewGuid(),
            Pid = Guid.Empty,
            Name = "ParentRole",
            Code = "PARENT",
            Sequence = 1,
            Status = StatusEnum.Enable,
            Built = false,
            CreatedAt = DateTimeOffset.Now,
            CreatedBy = Guid.NewGuid()
        };

        var childRole = new Role
        {
            Id = Guid.NewGuid(),
            Pid = parentRole.Id,
            Name = "ChildRole",
            Code = "CHILD",
            Sequence = 1,
            Status = StatusEnum.Enable,
            Built = false,
            CreatedAt = DateTimeOffset.Now,
            CreatedBy = Guid.NewGuid()
        };

        var roles = new List<Role> { parentRole, childRole };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetAllRoleAsync(Arg.Any<CancellationToken>())
            .Returns(roles);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        var roleDtoList = new List<RoleDto>
        {
            new RoleDto
            {
                Id = parentRole.Id,
                Pid = parentRole.Pid,
                Name = parentRole.Name,
                Code = parentRole.Code,
                Sequence = parentRole.Sequence,
                Status = parentRole.Status,
                Built = parentRole.Built,
                Children = new List<RoleDto>
                {
                    new RoleDto
                    {
                        Id = childRole.Id,
                        Pid = childRole.Pid,
                        Name = childRole.Name,
                        Code = childRole.Code,
                        Sequence = childRole.Sequence,
                        Status = childRole.Status,
                        Built = childRole.Built,
                        Children = new List<RoleDto>()
                    }
                }
            }
        };
        mockMapper.Map<List<RoleDto>>(Arg.Any<List<Role>>())
            .Returns(roleDtoList);

        var mockUserRoleService = Substitute.For<IUserRoleService>();
        var mockRolePermissionService = Substitute.For<IRolePermissionService>();

        var service = new RoleService(
            mockMapper,
            mockRoleDataProvider,
            mockUserRoleService,
            mockRolePermissionService);

        var response = await service.GetRoleTreeAsync(new GetRoleTreeRequest(), CancellationToken.None);

        response.ShouldNotBeNull();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(1);
        response.Data[0].Children.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetRoleTreeAsync_WithEmptyRoles_ReturnsEmptyList()
    {
        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetAllRoleAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Role>());

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        mockMapper.Map<List<RoleDto>>(Arg.Any<List<Role>>())
            .Returns(new List<RoleDto>());

        var mockUserRoleService = Substitute.For<IUserRoleService>();
        var mockRolePermissionService = Substitute.For<IRolePermissionService>();

        var service = new RoleService(
            mockMapper,
            mockRoleDataProvider,
            mockUserRoleService,
            mockRolePermissionService);

        var response = await service.GetRoleTreeAsync(new GetRoleTreeRequest(), CancellationToken.None);

        response.ShouldNotBeNull();
        response.Data.ShouldNotBeNull();
        response.Data.ShouldBeEmpty();
    }

    #endregion

    #region AddRoleAsync

    [Fact]
    public async Task AddRoleAsync_WithValidParent_CreatesRole()
    {
        var parentId = Guid.NewGuid();
        var parentRole = new Role
        {
            Id = parentId,
            Pid = Guid.Empty,
            Name = "Parent",
            Code = "PARENT",
            Status = StatusEnum.Enable
        };

        var command = new AddRoleCommand
        {
            Pid = parentId,
            Name = "NewRole",
            Code = "NEW_ROLE",
            Description = "New role description",
            Sequence = 1,
            Status = StatusEnum.Enable
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetAnyAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(true);
        mockRoleDataProvider.CreateRoleAsync(Arg.Any<Role>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        mockMapper.Map<Role>(command)
            .Returns(new Role
            {
                Id = Guid.NewGuid(),
                Pid = command.Pid,
                Name = command.Name,
                Code = command.Code,
                Description = command.Description,
                Sequence = command.Sequence,
                Status = command.Status
            });

        var mockUserRoleService = Substitute.For<IUserRoleService>();
        var mockRolePermissionService = Substitute.For<IRolePermissionService>();

        var service = new RoleService(
            mockMapper,
            mockRoleDataProvider,
            mockUserRoleService,
            mockRolePermissionService);

        var response = await service.AddRoleAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldNotBe(HttpStatusCode.BadRequest);
        response.Message.ShouldBeEmpty();
        await mockRoleDataProvider.Received().CreateRoleAsync(Arg.Any<Role>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddRoleAsync_WithNonExistentParent_ReturnsBadRequest()
    {
        var nonExistentParentId = Guid.NewGuid();

        var command = new AddRoleCommand
        {
            Pid = nonExistentParentId,
            Name = "NewRole",
            Code = "NEW_ROLE",
            Status = StatusEnum.Enable
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetAnyAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        var mockUserRoleService = Substitute.For<IUserRoleService>();
        var mockRolePermissionService = Substitute.For<IRolePermissionService>();

        var service = new RoleService(
            mockMapper,
            mockRoleDataProvider,
            mockUserRoleService,
            mockRolePermissionService);

        var response = await service.AddRoleAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldBe(HttpStatusCode.BadRequest);
        response.Message.ShouldBe("Parent role not found");
    }

    [Fact]
    public async Task AddRoleAsync_WithEmptyParent_CreatesRootRole()
    {
        var command = new AddRoleCommand
        {
            Pid = Guid.Empty,
            Name = "RootRole",
            Code = "ROOT_ROLE",
            Status = StatusEnum.Enable
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.CreateRoleAsync(Arg.Any<Role>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        mockMapper.Map<Role>(command)
            .Returns(new Role
            {
                Id = Guid.NewGuid(),
                Pid = Guid.Empty,
                Name = command.Name,
                Code = command.Code,
                Status = command.Status
            });

        var mockUserRoleService = Substitute.For<IUserRoleService>();
        var mockRolePermissionService = Substitute.For<IRolePermissionService>();

        var service = new RoleService(
            mockMapper,
            mockRoleDataProvider,
            mockUserRoleService,
            mockRolePermissionService);

        var response = await service.AddRoleAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldNotBe(HttpStatusCode.BadRequest);
        await mockRoleDataProvider.Received().CreateRoleAsync(Arg.Any<Role>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region UpdateRoleAsync

    [Fact]
    public async Task UpdateRoleAsync_WithBuiltRole_ReturnsBadRequest()
    {
        var roleId = Guid.NewGuid();
        var command = new UpdateRoleCommand
        {
            Id = roleId,
            Name = "UpdatedName"
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetBuiltAnyAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        var mockUserRoleService = Substitute.For<IUserRoleService>();
        var mockRolePermissionService = Substitute.For<IRolePermissionService>();

        var service = new RoleService(
            mockMapper,
            mockRoleDataProvider,
            mockUserRoleService,
            mockRolePermissionService);

        var response = await service.UpdateRoleAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldBe(HttpStatusCode.BadRequest);
        response.Message.ShouldBe("System built-in role cannot be modified.");
    }

    [Fact]
    public async Task UpdateRoleAsync_WithNonExistentRole_ReturnsBadRequest()
    {
        var roleId = Guid.NewGuid();
        var command = new UpdateRoleCommand
        {
            Id = roleId,
            Name = "UpdatedName"
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetBuiltAnyAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(false);
        mockRoleDataProvider.GetRoleAsync(roleId, Arg.Any<CancellationToken>())
            .Returns((Role?)null);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        var mockUserRoleService = Substitute.For<IUserRoleService>();
        var mockRolePermissionService = Substitute.For<IRolePermissionService>();

        var service = new RoleService(
            mockMapper,
            mockRoleDataProvider,
            mockUserRoleService,
            mockRolePermissionService);

        var response = await service.UpdateRoleAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldBe(HttpStatusCode.BadRequest);
        response.Message.ShouldBe("Role not found");
    }

    [Fact]
    public async Task UpdateRoleAsync_WithValidData_UpdatesRole()
    {
        var roleId = Guid.NewGuid();
        var existingRole = new Role
        {
            Id = roleId,
            Pid = Guid.Empty,
            Name = "OldName",
            Code = "OLD_CODE",
            Status = StatusEnum.Enable,
            Built = false
        };

        var command = new UpdateRoleCommand
        {
            Id = roleId,
            Name = "UpdatedName",
            Code = "UPDATED_CODE"
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetBuiltAnyAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(false);
        mockRoleDataProvider.GetRoleAsync(roleId, Arg.Any<CancellationToken>())
            .Returns(existingRole);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        mockMapper.Map(command, existingRole).Returns(existingRole);

        var mockUserRoleService = Substitute.For<IUserRoleService>();
        var mockRolePermissionService = Substitute.For<IRolePermissionService>();

        var service = new RoleService(
            mockMapper,
            mockRoleDataProvider,
            mockUserRoleService,
            mockRolePermissionService);

        var response = await service.UpdateRoleAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldNotBe(HttpStatusCode.BadRequest);
        mockMapper.Received().Map(command, existingRole);
    }

    #endregion

    #region DeleteRoleCascadeAsync

    [Fact]
    public async Task DeleteRoleCascadeAsync_WithBuiltRole_ReturnsBadRequest()
    {
        var roleId = Guid.NewGuid();
        var command = new DeleteRoleCascadeCommand
        {
            IdList = [roleId]
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetBuiltAnyAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        var mockUserRoleService = Substitute.For<IUserRoleService>();
        var mockRolePermissionService = Substitute.For<IRolePermissionService>();

        var service = new RoleService(
            mockMapper,
            mockRoleDataProvider,
            mockUserRoleService,
            mockRolePermissionService);

        var response = await service.DeleteRoleCascadeAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldBe(HttpStatusCode.BadRequest);
        response.Message.ShouldBe("System built-in roles cannot be deleted.");
    }

    [Fact]
    public async Task DeleteRoleCascadeAsync_WithChildRole_ReturnsBadRequest()
    {
        var roleId = Guid.NewGuid();
        var command = new DeleteRoleCascadeCommand
        {
            IdList = [roleId]
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetBuiltAnyAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(false);
        mockRoleDataProvider.GetChildAnyAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        var mockUserRoleService = Substitute.For<IUserRoleService>();
        var mockRolePermissionService = Substitute.For<IRolePermissionService>();

        var service = new RoleService(
            mockMapper,
            mockRoleDataProvider,
            mockUserRoleService,
            mockRolePermissionService);

        var response = await service.DeleteRoleCascadeAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldBe(HttpStatusCode.BadRequest);
        response.Message.ShouldBe("Child role exists");
    }

    [Fact]
    public async Task DeleteRoleCascadeAsync_WithValidRole_DeletesCascade()
    {
        var roleId = Guid.NewGuid();
        var command = new DeleteRoleCascadeCommand
        {
            IdList = [roleId]
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetBuiltAnyAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(false);
        mockRoleDataProvider.GetChildAnyAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(false);
        mockRoleDataProvider.DeleteRoleListAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var mockUserRoleService = Substitute.For<IUserRoleService>();
        mockUserRoleService.RemoveRolesCascadeUserAsync(Arg.Any<RemoveRolesCascadeUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(new RemoveRolesCascadeUserResponse());

        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        mockRolePermissionService.RemoveRoleCascadePermissionAsync(Arg.Any<RemoveRoleCascadePermissionCommand>(), Arg.Any<CancellationToken>())
            .Returns(new RemoveRoleCascadePermissionResponse());

        var mockMapper = Substitute.For<AutoMapper.IMapper>();

        var service = new RoleService(
            mockMapper,
            mockRoleDataProvider,
            mockUserRoleService,
            mockRolePermissionService);

        var response = await service.DeleteRoleCascadeAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldNotBe(HttpStatusCode.BadRequest);
        await mockUserRoleService.Received().RemoveRolesCascadeUserAsync(Arg.Any<RemoveRolesCascadeUserCommand>(), Arg.Any<CancellationToken>());
        await mockRolePermissionService.Received().RemoveRoleCascadePermissionAsync(Arg.Any<RemoveRoleCascadePermissionCommand>(), Arg.Any<CancellationToken>());
        await mockRoleDataProvider.Received().DeleteRoleListAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>());
    }

    #endregion
}