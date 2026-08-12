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
        mockMenuDataProvider.GetAnyAsync([parentId], Arg.Any<CancellationToken>())
            .Returns(true);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        mockMapper.Map(command, existingMenu).Returns(existingMenu);

        var service = new MenuService(
            mockMapper,
            mockMenuDataProvider,
            Substitute.For<IPermissionMenuService>());

        var response = await service.UpdateMenuAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        mockMapper.Received().Map(command, existingMenu);
        await mockMenuDataProvider.Received(1)
            .UpdateMenuAsync(existingMenu, Arg.Any<CancellationToken>());
    }
}
