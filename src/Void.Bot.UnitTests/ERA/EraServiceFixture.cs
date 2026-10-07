using System.Net;
using AutoMapper;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.UnitTest.ERA;

public class EraServiceFixture
{
    private readonly IEraDataProvider _dataProvider = Substitute.For<IEraDataProvider>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly Guid _accountId = Guid.NewGuid();

    private EraService CreateService()
    {
        _currentUser.Id.Returns(_accountId);
        _mapper.Map<EraCapacityReceiptDto>(Arg.Any<EraCapacityChange>()).Returns(callInformation =>
        {
            var capacityChange = callInformation.Arg<EraCapacityChange>();
            return new EraCapacityReceiptDto
            {
                RequestId = capacityChange.RequestId,
                PlayerId = capacityChange.PlayerId,
                PreviousCapacity = capacityChange.PreviousCapacity,
                WarehouseCapacity = capacityChange.WarehouseCapacity,
                PlayerVersion = capacityChange.PlayerVersion
            };
        });
        return new EraService(
            _dataProvider,
            _currentUser,
            _mapper,
            Substitute.For<ILocalizationService>());
    }

    [Fact]
    public async Task AdjustCapacity_ReplayedRequestReturnsReceiptWithoutSecondAdjustment()
    {
        var service = CreateService();
        var player = new EraPlayer
        {
            Id = Guid.NewGuid(),
            WarehouseCapacity = 300,
            Version = 1
        };
        var command = new AdjustEraWarehouseCapacityCommand
        {
            PlayerId = player.Id,
            RequestId = Guid.NewGuid(),
            ExpectedVersion = 1,
            Delta = 20,
            Reason = "Grant"
        };
        _dataProvider.LockPlayerAsync(player.Id, Arg.Any<CancellationToken>()).Returns(player);
        var first = await service.AdjustCapacityAsync(command, CancellationToken.None);
        first.Data!.WarehouseCapacity.ShouldBe(320);
        _dataProvider.GetReceiptAsync(player.Id, command.RequestId, Arg.Any<CancellationToken>()).Returns(new EraCapacityChange
        {
            PlayerId = player.Id,
            RequestId = command.RequestId,
            OperatorId = _accountId,
            IsAdjustment = true,
            RequestedValue = 20,
            ExpectedVersion = 1,
            Reason = "Grant",
            PreviousCapacity = 300,
            WarehouseCapacity = 320,
            PlayerVersion = 2
        });
        var retry = await service.AdjustCapacityAsync(command, CancellationToken.None);
        retry.Code.ShouldBe(HttpStatusCode.OK);
        player.WarehouseCapacity.ShouldBe(320);
        player.Version.ShouldBe(2);
        await _dataProvider
            .Received(1)
            .CreateReceiptAsync(Arg.Any<EraCapacityChange>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(-301, 1, "era.capacity_out_of_range")]
    [InlineData(20, 2, "era.version_conflict")]
    public async Task AdjustCapacity_InvalidResultOrVersionDoesNotChangePlayer(int delta, long expected, string error)
    {
        var service = CreateService();
        var player = new EraPlayer
        {
            Id = Guid.NewGuid(),
            WarehouseCapacity = 300,
            Version = 1
        };
        _dataProvider.LockPlayerAsync(player.Id, Arg.Any<CancellationToken>()).Returns(player);
        var command = new AdjustEraWarehouseCapacityCommand
        {
            PlayerId = player.Id,
            RequestId = Guid.NewGuid(),
            ExpectedVersion = expected,
            Delta = delta,
            Reason = "Check"
        };
        var result = await service.AdjustCapacityAsync(command, CancellationToken.None);
        result.ErrorCode.ShouldBe(error);
        player.WarehouseCapacity.ShouldBe(300);
        await _dataProvider
            .DidNotReceive()
            .CreateReceiptAsync(Arg.Any<EraCapacityChange>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPlayer_UsesAuthenticatedAccount()
    {
        var service = CreateService();
        await service.GetPlayerAsync(new GetEraPlayerRequest(), CancellationToken.None);
        await _dataProvider
            .Received(1)
            .GetPlayerByAccountIdAsync(_accountId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Initialize_ExistingPlayerDoesNotApplyNewDefaultCapacity()
    {
        var service = CreateService();
        _dataProvider.LockAccountAsync(_accountId, Arg.Any<CancellationToken>()).Returns(true);
        var player = new EraPlayer
        {
            Id = Guid.NewGuid(),
            WarehouseCapacity = 450,
            Version = 3
        };
        _dataProvider.LockPlayerByAccountIdAsync(_accountId, Arg.Any<CancellationToken>()).Returns(player);
        _mapper.Map<EraPlayerDto>(player).Returns(new EraPlayerDto
        {
            Id = player.Id,
            WarehouseCapacity = 450
        });
        var result = await service.InitializePlayerAsync(new InitializeEraPlayerCommand(), CancellationToken.None);
        result.Data!.WarehouseCapacity.ShouldBe(450);
        await _dataProvider
            .DidNotReceive()
            .CreatePlayerAsync(Arg.Any<EraPlayer>(), Arg.Any<CancellationToken>());
        await _dataProvider
            .DidNotReceive()
            .GetConfigurationsAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }
}
