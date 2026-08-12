using System.Net;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Domain.Security;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;
using Void.Bot.Message.Enum.Security;

namespace Void.Bot.UnitTest.Security;

public class MenuServiceFixture
{
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
}
