using Shouldly;
using Void.Bot.Core.Domain.Security;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Security;
using Void.Bot.Message.Request.Security;
using NSubstitute;
using System.Net;

namespace Void.Bot.UnitTest.Security;

public class PermissionServiceFixture
{
    #region GetPermissionAsync

    [Fact]
    public async Task GetPermissionAsync_WithValidId_ReturnsPermission()
    {
        var permissionId = Guid.NewGuid();
        var permission = new Permission
        {
            Id = permissionId,
            Pid = Guid.Empty,
            Name = "TestPermission",
            Code = "TEST_PERMISSION",
            Description = "Test Description",
            Type = PermissionTypeEnum.Action,
            Sequence = 1,
            Status = StatusEnum.Enable,
            CreatedAt = DateTimeOffset.Now,
            CreatedBy = Guid.NewGuid()
        };

        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        mockPermissionDataProvider.GetPermissionAsync(permissionId, Arg.Any<CancellationToken>())
            .Returns(permission);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        mockMapper.Map<Message.Dto.Security.PermissionDto>(permission)
            .Returns(new Message.Dto.Security.PermissionDto
            {
                Id = permission.Id,
                Pid = permission.Pid,
                Name = permission.Name,
                Code = permission.Code,
                Description = permission.Description,
                Type = permission.Type,
                Sequence = permission.Sequence,
                Status = permission.Status
            });

        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        var mockPermissionMenuService = Substitute.For<IPermissionMenuService>();
        var mockPermissionEndpointService = Substitute.For<IPermissionEndpointService>();

        var service = new PermissionService(
            mockMapper,
            mockPermissionDataProvider,
            mockRolePermissionService,
            mockPermissionMenuService,
            mockPermissionEndpointService);

        var response = await service.GetPermissionAsync(new GetPermissionRequest { Id = permissionId }, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Data.ShouldNotBeNull();
        response.Data.Id.ShouldBe(permissionId);
        response.Data.Name.ShouldBe("TestPermission");
        response.Data.Type.ShouldBe(PermissionTypeEnum.Action);
    }

    [Fact]
    public async Task GetPermissionAsync_WithNonExistentId_ReturnsNullData()
    {
        var nonExistentId = Guid.NewGuid();

        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        mockPermissionDataProvider.GetPermissionAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((Permission?)null);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        var mockPermissionMenuService = Substitute.For<IPermissionMenuService>();
        var mockPermissionEndpointService = Substitute.For<IPermissionEndpointService>();

        var service = new PermissionService(
            mockMapper,
            mockPermissionDataProvider,
            mockRolePermissionService,
            mockPermissionMenuService,
            mockPermissionEndpointService);

        var response = await service.GetPermissionAsync(new GetPermissionRequest { Id = nonExistentId }, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Data.ShouldBeNull();
    }

    #endregion

    #region GetPermissionTreeAsync

    [Fact]
    public async Task GetPermissionTreeAsync_WithPermissions_ReturnsTreeStructure()
    {
        var parentPermission = new Permission
        {
            Id = Guid.NewGuid(),
            Pid = Guid.Empty,
            Name = "ParentPermission",
            Code = "PARENT",
            Type = PermissionTypeEnum.Module,
            Sequence = 1,
            Status = StatusEnum.Enable,
            CreatedAt = DateTimeOffset.Now,
            CreatedBy = Guid.NewGuid()
        };

        var childPermission = new Permission
        {
            Id = Guid.NewGuid(),
            Pid = parentPermission.Id,
            Name = "ChildPermission",
            Code = "CHILD",
            Type = PermissionTypeEnum.Action,
            Sequence = 1,
            Status = StatusEnum.Enable,
            CreatedAt = DateTimeOffset.Now,
            CreatedBy = Guid.NewGuid()
        };

        var permissions = new List<Permission> { parentPermission, childPermission };

        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        mockPermissionDataProvider.GetAllPermissionAsync(Arg.Any<CancellationToken>())
            .Returns(permissions);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        var permissionDtoList = new List<Message.Dto.Security.PermissionDto>
        {
            new Message.Dto.Security.PermissionDto
            {
                Id = parentPermission.Id,
                Pid = parentPermission.Pid,
                Name = parentPermission.Name,
                Code = parentPermission.Code,
                Type = parentPermission.Type,
                Sequence = parentPermission.Sequence,
                Status = parentPermission.Status,
                Children = new List<Message.Dto.Security.PermissionDto>
                {
                    new Message.Dto.Security.PermissionDto
                    {
                        Id = childPermission.Id,
                        Pid = childPermission.Pid,
                        Name = childPermission.Name,
                        Code = childPermission.Code,
                        Type = childPermission.Type,
                        Sequence = childPermission.Sequence,
                        Status = childPermission.Status,
                        Children = new List<Message.Dto.Security.PermissionDto>()
                    }
                }
            }
        };
        mockMapper.Map<List<Message.Dto.Security.PermissionDto>>(Arg.Any<List<Permission>>())
            .Returns(permissionDtoList);

        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        var mockPermissionMenuService = Substitute.For<IPermissionMenuService>();
        var mockPermissionEndpointService = Substitute.For<IPermissionEndpointService>();

        var service = new PermissionService(
            mockMapper,
            mockPermissionDataProvider,
            mockRolePermissionService,
            mockPermissionMenuService,
            mockPermissionEndpointService);

        var response = await service.GetPermissionTreeAsync(new GetPermissionTreeRequest(), CancellationToken.None);

        response.ShouldNotBeNull();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(1);
        response.Data[0].Children.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetPermissionTreeAsync_WithEmptyPermissions_ReturnsEmptyList()
    {
        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        mockPermissionDataProvider.GetAllPermissionAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Permission>());

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        mockMapper.Map<List<Message.Dto.Security.PermissionDto>>(Arg.Any<List<Permission>>())
            .Returns(new List<Message.Dto.Security.PermissionDto>());

        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        var mockPermissionMenuService = Substitute.For<IPermissionMenuService>();
        var mockPermissionEndpointService = Substitute.For<IPermissionEndpointService>();

        var service = new PermissionService(
            mockMapper,
            mockPermissionDataProvider,
            mockRolePermissionService,
            mockPermissionMenuService,
            mockPermissionEndpointService);

        var response = await service.GetPermissionTreeAsync(new GetPermissionTreeRequest(), CancellationToken.None);

        response.ShouldNotBeNull();
        response.Data.ShouldNotBeNull();
        response.Data.ShouldBeEmpty();
    }

    #endregion

    #region AddPermissionAsync

    [Fact]
    public async Task AddPermissionAsync_WithValidParent_CreatesPermission()
    {
        var parentId = Guid.NewGuid();
        var command = new AddPermissionCommand
        {
            Pid = parentId,
            Name = "NewPermission",
            Code = "NEW_PERMISSION",
            Description = "New permission description",
            Type = PermissionTypeEnum.Action,
            Sequence = 1,
            Status = StatusEnum.Enable
        };

        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        mockPermissionDataProvider.GetAnyAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(true);
        mockPermissionDataProvider.CreatePermissionAsync(Arg.Any<Permission>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        mockMapper.Map<Permission>(command)
            .Returns(new Permission
            {
                Id = Guid.NewGuid(),
                Pid = command.Pid,
                Name = command.Name,
                Code = command.Code,
                Description = command.Description,
                Type = command.Type,
                Sequence = command.Sequence,
                Status = command.Status
            });

        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        var mockPermissionMenuService = Substitute.For<IPermissionMenuService>();
        var mockPermissionEndpointService = Substitute.For<IPermissionEndpointService>();

        var service = new PermissionService(
            mockMapper,
            mockPermissionDataProvider,
            mockRolePermissionService,
            mockPermissionMenuService,
            mockPermissionEndpointService);

        var response = await service.AddPermissionAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldNotBe(HttpStatusCode.BadRequest);
        response.Message.ShouldBeEmpty();
        await mockPermissionDataProvider.Received().CreatePermissionAsync(Arg.Any<Permission>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddPermissionAsync_WithNonExistentParent_ReturnsBadRequest()
    {
        var nonExistentParentId = Guid.NewGuid();

        var command = new AddPermissionCommand
        {
            Pid = nonExistentParentId,
            Name = "NewPermission",
            Code = "NEW_PERMISSION",
            Type = PermissionTypeEnum.Action,
            Status = StatusEnum.Enable
        };

        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        mockPermissionDataProvider.GetAnyAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        var mockPermissionMenuService = Substitute.For<IPermissionMenuService>();
        var mockPermissionEndpointService = Substitute.For<IPermissionEndpointService>();

        var service = new PermissionService(
            mockMapper,
            mockPermissionDataProvider,
            mockRolePermissionService,
            mockPermissionMenuService,
            mockPermissionEndpointService);

        var response = await service.AddPermissionAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldBe(HttpStatusCode.BadRequest);
        response.Message.ShouldBe("Parent permission not found");
    }

    #endregion

    #region UpdatePermissionAsync

    [Fact]
    public async Task UpdatePermissionAsync_WithNonExistentPermission_ReturnsBadRequest()
    {
        var permissionId = Guid.NewGuid();
        var command = new UpdatePermissionCommand
        {
            Id = permissionId,
            Name = "UpdatedName"
        };

        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        mockPermissionDataProvider.GetPermissionAsync(permissionId, Arg.Any<CancellationToken>())
            .Returns((Permission?)null);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        var mockPermissionMenuService = Substitute.For<IPermissionMenuService>();
        var mockPermissionEndpointService = Substitute.For<IPermissionEndpointService>();

        var service = new PermissionService(
            mockMapper,
            mockPermissionDataProvider,
            mockRolePermissionService,
            mockPermissionMenuService,
            mockPermissionEndpointService);

        var response = await service.UpdatePermissionAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldBe(HttpStatusCode.BadRequest);
        response.Message.ShouldBe("Permission not found");
    }

    [Fact]
    public async Task UpdatePermissionAsync_WithValidData_UpdatesPermission()
    {
        var permissionId = Guid.NewGuid();
        var existingPermission = new Permission
        {
            Id = permissionId,
            Pid = Guid.Empty,
            Name = "OldName",
            Code = "OLD_CODE",
            Type = PermissionTypeEnum.Module,
            Status = StatusEnum.Enable
        };

        var command = new UpdatePermissionCommand
        {
            Id = permissionId,
            Name = "UpdatedName",
            Code = "UPDATED_CODE"
        };

        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        mockPermissionDataProvider.GetPermissionAsync(permissionId, Arg.Any<CancellationToken>())
            .Returns(existingPermission);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        mockMapper.Map(command, existingPermission).Returns(existingPermission);

        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        var mockPermissionMenuService = Substitute.For<IPermissionMenuService>();
        var mockPermissionEndpointService = Substitute.For<IPermissionEndpointService>();

        var service = new PermissionService(
            mockMapper,
            mockPermissionDataProvider,
            mockRolePermissionService,
            mockPermissionMenuService,
            mockPermissionEndpointService);

        var response = await service.UpdatePermissionAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldNotBe(HttpStatusCode.BadRequest);
        mockMapper.Received().Map(command, existingPermission);
    }

    [Fact]
    public async Task UpdatePermissionAsync_ModuleToActionWithChildren_ReturnsBadRequest()
    {
        var permissionId = Guid.NewGuid();
        var existingPermission = new Permission
        {
            Id = permissionId,
            Pid = Guid.Empty,
            Name = "ModulePermission",
            Code = "MODULE",
            Type = PermissionTypeEnum.Module,
            Status = StatusEnum.Enable
        };

        var command = new UpdatePermissionCommand
        {
            Id = permissionId,
            Type = PermissionTypeEnum.Action
        };

        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        mockPermissionDataProvider.GetPermissionAsync(permissionId, Arg.Any<CancellationToken>())
            .Returns(existingPermission);
        mockPermissionDataProvider.GetChildAnyAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        var mockPermissionMenuService = Substitute.For<IPermissionMenuService>();
        var mockPermissionEndpointService = Substitute.For<IPermissionEndpointService>();

        var service = new PermissionService(
            mockMapper,
            mockPermissionDataProvider,
            mockRolePermissionService,
            mockPermissionMenuService,
            mockPermissionEndpointService);

        var response = await service.UpdatePermissionAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldBe(HttpStatusCode.BadRequest);
        response.Message.ShouldBe("Child permission exists, cannot change type to Action");
    }

    #endregion

    #region DeletePermissionCascadeAsync

    [Fact]
    public async Task DeletePermissionCascadeAsync_WithChildPermission_ReturnsBadRequest()
    {
        var permissionId = Guid.NewGuid();
        var command = new DeletePermissionCascadeCommand
        {
            IdList = [permissionId]
        };

        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        mockPermissionDataProvider.GetChildAnyAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        var mockPermissionMenuService = Substitute.For<IPermissionMenuService>();
        var mockPermissionEndpointService = Substitute.For<IPermissionEndpointService>();

        var service = new PermissionService(
            mockMapper,
            mockPermissionDataProvider,
            mockRolePermissionService,
            mockPermissionMenuService,
            mockPermissionEndpointService);

        var response = await service.DeletePermissionCascadeAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldBe(HttpStatusCode.BadRequest);
        response.Message.ShouldBe("Child permission exists");
    }

    [Fact]
    public async Task DeletePermissionCascadeAsync_WithValidPermission_DeletesCascade()
    {
        var permissionId = Guid.NewGuid();
        var command = new DeletePermissionCascadeCommand
        {
            IdList = [permissionId]
        };

        var mockPermissionDataProvider = Substitute.For<IPermissionDataProvider>();
        mockPermissionDataProvider.GetChildAnyAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(false);
        mockPermissionDataProvider.DeletePermissionsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var mockRolePermissionService = Substitute.For<IRolePermissionService>();
        mockRolePermissionService.RemovePermissionCascadeRoleAsync(Arg.Any<RemovePermissionCascadeRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(new RemovePermissionCascadeRoleResponse());

        var mockPermissionMenuService = Substitute.For<IPermissionMenuService>();
        mockPermissionMenuService.RemovePermissionsCascadeMenuAsync(Arg.Any<RemovePermissionsCascadeMenuCommand>(), Arg.Any<CancellationToken>())
            .Returns(new RemovePermissionsCascadeMenuResponse());

        var mockPermissionEndpointService = Substitute.For<IPermissionEndpointService>();
        mockPermissionEndpointService.RemovePermissionsCascadeEndpointAsync(Arg.Any<RemovePermissionsCascadeEndpointCommand>(), Arg.Any<CancellationToken>())
            .Returns(new RemovePermissionsCascadeEndpointResponse());

        var mockMapper = Substitute.For<AutoMapper.IMapper>();

        var service = new PermissionService(
            mockMapper,
            mockPermissionDataProvider,
            mockRolePermissionService,
            mockPermissionMenuService,
            mockPermissionEndpointService);

        var response = await service.DeletePermissionCascadeAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldNotBe(HttpStatusCode.BadRequest);
        await mockRolePermissionService.Received().RemovePermissionCascadeRoleAsync(Arg.Any<RemovePermissionCascadeRoleCommand>(), Arg.Any<CancellationToken>());
        await mockPermissionMenuService.Received().RemovePermissionsCascadeMenuAsync(Arg.Any<RemovePermissionsCascadeMenuCommand>(), Arg.Any<CancellationToken>());
        await mockPermissionEndpointService.Received().RemovePermissionsCascadeEndpointAsync(Arg.Any<RemovePermissionsCascadeEndpointCommand>(), Arg.Any<CancellationToken>());
        await mockPermissionDataProvider.Received().DeletePermissionsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>());
    }

    #endregion
}