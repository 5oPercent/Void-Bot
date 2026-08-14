using System.Net;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Domain.Security;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;
using Void.Bot.Message.Dto.Security;
using Void.Bot.Message.Enum.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.UnitTest.Security;

public class MenuServiceFixture
{
    [Fact]
    public async Task GetMenuTreeAsync_OrdersEachLevelBySequenceAndCreatedAt()
    {
        var baseTime = new DateTimeOffset(2026, 8, 14, 0, 0, 0, TimeSpan.Zero);
        var firstRoot = new Menu { Id = Guid.NewGuid(), Sequence = 0, CreatedAt = baseTime.AddSeconds(1) };
        var secondRoot = new Menu { Id = Guid.NewGuid(), Sequence = 1, CreatedAt = baseTime };
        var firstChild = new Menu
        {
            Id = Guid.NewGuid(), Pid = firstRoot.Id, Sequence = 1, CreatedAt = baseTime
        };
        var secondChild = new Menu
        {
            Id = Guid.NewGuid(), Pid = firstRoot.Id, Sequence = 1, CreatedAt = baseTime.AddSeconds(1)
        };
        var mockMenuDataProvider = Substitute.For<IMenuDataProvider>();
        mockMenuDataProvider.GetAllMenuAsync(Arg.Any<CancellationToken>())
            .Returns([secondChild, secondRoot, firstChild, firstRoot]);
        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        mockMapper.Map<List<MenuDto>>(Arg.Any<List<Menu>>()).Returns([]);
        var service = new MenuService(
            mockMapper,
            mockMenuDataProvider,
            Substitute.For<IPermissionMenuService>());

        await service.GetMenuTreeAsync(new GetMenuTreeRequest(), CancellationToken.None);

        mockMapper.Received(1).Map<List<MenuDto>>(Arg.Is<List<Menu>>(tree =>
            tree.Select(x => x.Id).SequenceEqual(new[] { firstRoot.Id, secondRoot.Id }) &&
            tree[0].Children.Select(x => x.Id).SequenceEqual(new[] { firstChild.Id, secondChild.Id })));
    }

    [Fact]
    public async Task AddMenuAsync_WithRootMenu_CreatesMenu()
    {
        var command = new AddMenuCommand
        {
            Pid = Guid.Empty,
            Name = "RootMenu",
            Type = MenuTypeEnum.Directory
        };
        var menu = new Menu
        {
            Pid = Guid.Empty,
            Name = command.Name,
            Type = command.Type
        };

        var mockMenuDataProvider = Substitute.For<IMenuDataProvider>();
        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        mockMapper.Map<Menu>(command).Returns(menu);

        var service = new MenuService(
            mockMapper,
            mockMenuDataProvider,
            Substitute.For<IPermissionMenuService>());

        var response = await service.AddMenuAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        await mockMenuDataProvider.DidNotReceive()
            .GetAnyAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>());
        await mockMenuDataProvider.Received(1)
            .CreateMenuAsync(menu, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateMenuAsync_WithValidData_UpdatesMenu()
    {
        var parentId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var existingMenu = new Menu
        {
            Id = menuId,
            Pid = parentId,
            Name = "OldName",
            Type = MenuTypeEnum.Page
        };
        var command = new UpdateMenuCommand
        {
            Id = menuId,
            Pid = parentId,
            Name = "UpdatedName",
            Type = MenuTypeEnum.Page
        };

        var mockMenuDataProvider = Substitute.For<IMenuDataProvider>();
        mockMenuDataProvider.GetMenuAsync(menuId, Arg.Any<CancellationToken>())
            .Returns(existingMenu);
        mockMenuDataProvider.GetAnyAsync(
                Arg.Is<List<Guid>>(ids => ids.Count == 1 && ids[0] == parentId),
                Arg.Any<CancellationToken>())
            .Returns(true);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        mockMapper.Map(command, existingMenu).Returns(existingMenu);

        var service = new MenuService(
            mockMapper,
            mockMenuDataProvider,
            Substitute.For<IPermissionMenuService>());

        var response = await service.UpdateMenuAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldNotBe(HttpStatusCode.BadRequest);
        mockMapper.Received().Map(command, existingMenu);
        await mockMenuDataProvider.Received(1)
            .UpdateMenuAsync(existingMenu, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateMenuAsync_WithRootMenu_UpdatesMenu()
    {
        var menuId = Guid.NewGuid();
        var existingMenu = new Menu
        {
            Id = menuId,
            Pid = Guid.NewGuid(),
            Name = "ChildMenu",
            Type = MenuTypeEnum.Page
        };
        var command = new UpdateMenuCommand
        {
            Id = menuId,
            Pid = Guid.Empty,
            Name = "RootMenu",
            Type = MenuTypeEnum.Page
        };

        var mockMenuDataProvider = Substitute.For<IMenuDataProvider>();
        mockMenuDataProvider.GetMenuAsync(menuId, Arg.Any<CancellationToken>())
            .Returns(existingMenu);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        mockMapper.Map(command, existingMenu).Returns(existingMenu);

        var service = new MenuService(
            mockMapper,
            mockMenuDataProvider,
            Substitute.For<IPermissionMenuService>());

        var response = await service.UpdateMenuAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        await mockMenuDataProvider.DidNotReceive()
            .GetAnyAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>());
        await mockMenuDataProvider.Received(1)
            .UpdateMenuAsync(existingMenu, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteMenuCascadeAsync_WithoutCascadeAndWithChild_ReturnsBadRequest()
    {
        var menuId = Guid.NewGuid();
        var mockMenuDataProvider = Substitute.For<IMenuDataProvider>();
        mockMenuDataProvider.GetAllMenuAsync(Arg.Any<CancellationToken>())
            .Returns([
                new Menu { Id = menuId },
                new Menu { Id = Guid.NewGuid(), Pid = menuId }
            ]);
        var service = new MenuService(
            Substitute.For<AutoMapper.IMapper>(),
            mockMenuDataProvider,
            Substitute.For<IPermissionMenuService>());

        var response = await service.DeleteMenuCascadeAsync(
            new DeleteMenuCascadeCommand { IdList = [menuId] }, CancellationToken.None);

        response.Code.ShouldBe(HttpStatusCode.BadRequest);
        response.Message.ShouldBe("Child menu exists");
        await mockMenuDataProvider.DidNotReceive()
            .DeleteMenusAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteMenuCascadeAsync_WithoutCascadeAndWithExplicitChildren_DeletesSelectedMenus()
    {
        var menuId = Guid.NewGuid();
        var childMenuId = Guid.NewGuid();
        var mockMenuDataProvider = Substitute.For<IMenuDataProvider>();
        mockMenuDataProvider.GetAllMenuAsync(Arg.Any<CancellationToken>())
            .Returns([
                new Menu { Id = menuId },
                new Menu { Id = childMenuId, Pid = menuId }
            ]);
        var mockPermissionMenuService = Substitute.For<IPermissionMenuService>();
        mockPermissionMenuService.RemoveMenusCascadePermissionAsync(
                Arg.Any<RemoveMenusCascadePermissionCommand>(), Arg.Any<CancellationToken>())
            .Returns(new RemoveMenusCascadePermissionResponse());
        var service = new MenuService(
            Substitute.For<AutoMapper.IMapper>(),
            mockMenuDataProvider,
            mockPermissionMenuService);

        var response = await service.DeleteMenuCascadeAsync(
            new DeleteMenuCascadeCommand { IdList = [menuId, childMenuId] }, CancellationToken.None);

        response.Code.ShouldNotBe(HttpStatusCode.BadRequest);
        await mockMenuDataProvider.Received(1).DeleteMenusAsync(
            Arg.Is<List<Guid>>(ids => ids.Count == 2 && ids.Contains(menuId) && ids.Contains(childMenuId)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteMenuCascadeAsync_WithCascade_DeletesDescendants()
    {
        var menuId = Guid.NewGuid();
        var childMenuId = Guid.NewGuid();
        var grandchildMenuId = Guid.NewGuid();
        var mockMenuDataProvider = Substitute.For<IMenuDataProvider>();
        mockMenuDataProvider.GetAllMenuAsync(Arg.Any<CancellationToken>())
            .Returns([
                new Menu { Id = menuId },
                new Menu { Id = childMenuId, Pid = menuId },
                new Menu { Id = grandchildMenuId, Pid = childMenuId }
            ]);
        var mockPermissionMenuService = Substitute.For<IPermissionMenuService>();
        mockPermissionMenuService.RemoveMenusCascadePermissionAsync(
                Arg.Any<RemoveMenusCascadePermissionCommand>(), Arg.Any<CancellationToken>())
            .Returns(new RemoveMenusCascadePermissionResponse());
        var service = new MenuService(
            Substitute.For<AutoMapper.IMapper>(),
            mockMenuDataProvider,
            mockPermissionMenuService);

        var response = await service.DeleteMenuCascadeAsync(
            new DeleteMenuCascadeCommand { IdList = [menuId], Cascade = true }, CancellationToken.None);

        response.Code.ShouldNotBe(HttpStatusCode.BadRequest);
        await mockMenuDataProvider.Received(1).DeleteMenusAsync(
            Arg.Is<List<Guid>>(ids => ids.Count == 3 && ids.Contains(menuId) &&
                                      ids.Contains(childMenuId) && ids.Contains(grandchildMenuId)),
            Arg.Any<CancellationToken>());
    }
}
