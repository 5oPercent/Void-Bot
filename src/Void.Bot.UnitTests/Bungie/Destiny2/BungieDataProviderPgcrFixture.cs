using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Destiny2;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Core.Services.Http;
using Void.Bot.Core.Settings.Bungie;
using Void.Bot.Message.Constants.Bungie.Destiny2;
using Void.Bot.UnitTest.Security;

namespace Void.Bot.UnitTest.Bungie.Destiny2;

public class BungieDataProviderPgcrFixture
{
    [Fact]
    public void ShouldUseStatsHostForPgcrRequests()
    {
        Destiny2Constants.PostGameCarnageReportUrl.ShouldStartWith(
            "https://stats.bungie.net/Platform/Destiny2/Stats/PostGameCarnageReport/");
    }

    [Fact]
    public async Task ShouldInsertMissingPgcrAndPlayerSummary()
    {
        const long instanceId = 12280271243;
        var pgcrRepository = Substitute.For<IRepository<Destiny2Pgcr>>();
        var playerRepository = Substitute.For<IRepository<Destiny2PgcrPlayer>>();
        pgcrRepository.QueryNoTracking().Returns(
            new AsyncQueryableWrapper<Destiny2Pgcr>([]));
        playerRepository.QueryNoTracking().Returns(
            new AsyncQueryableWrapper<Destiny2PgcrPlayer>([]));
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var provider = new BungieDataProvider(
            new BungieSetting(Substitute.For<IConfiguration>()),
            Substitute.For<IVoidBotHttpClientFactory>(),
            Substitute.For<IRepository<Destiny2SeasonHistory>>(),
            pgcrRepository,
            playerRepository,
            unitOfWork);

        await provider.SavePgcrSummariesToDatabaseAsync(
            [new Destiny2Pgcr { Id = instanceId, DataVersion = 1 }],
            [
                new Destiny2PgcrPlayer
                {
                    Id = Guid.NewGuid(),
                    InstanceId = instanceId,
                    CharacterId = "2305843009511844485"
                }
            ],
            CancellationToken.None);

        await pgcrRepository.Received(1).InsertManyAsync(
            Arg.Any<IEnumerable<Destiny2Pgcr>>(), Arg.Any<CancellationToken>());
        await playerRepository.Received(1).InsertManyAsync(
            Arg.Any<IEnumerable<Destiny2PgcrPlayer>>(), Arg.Any<CancellationToken>());
        await pgcrRepository.DidNotReceive().UpdateManyAsync(
            Arg.Any<IEnumerable<Destiny2Pgcr>>(), Arg.Any<CancellationToken>());
        await playerRepository.DidNotReceive().UpdateManyAsync(
            Arg.Any<IEnumerable<Destiny2PgcrPlayer>>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldUpdateExistingPgcrAndPlayerSummary()
    {
        const long instanceId = 12280271243;
        const string characterId = "2305843009511844485";
        var existingPgcr = new Destiny2Pgcr
        {
            Id = instanceId,
            ReferenceId = 1,
            DataVersion = 0,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };
        var existingPlayer = new Destiny2PgcrPlayer
        {
            Id = Guid.NewGuid(),
            InstanceId = instanceId,
            CharacterId = characterId,
            Completed = false,
            StartSeconds = 60,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };
        var pgcrRepository = Substitute.For<IRepository<Destiny2Pgcr>>();
        var playerRepository = Substitute.For<IRepository<Destiny2PgcrPlayer>>();
        pgcrRepository.QueryNoTracking().Returns(
            new AsyncQueryableWrapper<Destiny2Pgcr>([existingPgcr]));
        playerRepository.QueryNoTracking().Returns(
            new AsyncQueryableWrapper<Destiny2PgcrPlayer>([existingPlayer]));
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var provider = new BungieDataProvider(
            new BungieSetting(Substitute.For<IConfiguration>()),
            Substitute.For<IVoidBotHttpClientFactory>(),
            Substitute.For<IRepository<Destiny2SeasonHistory>>(),
            pgcrRepository,
            playerRepository,
            unitOfWork);
        var period = new DateTimeOffset(2023, 1, 19, 14, 31, 20, TimeSpan.Zero);

        await provider.SavePgcrSummariesToDatabaseAsync(
            [
                new Destiny2Pgcr
                {
                    Id = instanceId,
                    Period = period,
                    ReferenceId = 1262462921,
                    StartingPhaseIndex = 0,
                    StartedFromBeginning = true,
                    DataVersion = 1
                }
            ],
            [
                new Destiny2PgcrPlayer
                {
                    Id = Guid.NewGuid(),
                    InstanceId = instanceId,
                    CharacterId = characterId,
                    Completed = true,
                    StartSeconds = 2
                }
            ],
            CancellationToken.None);

        existingPgcr.Period.ShouldBe(period);
        existingPgcr.ReferenceId.ShouldBe((uint)1262462921);
        existingPgcr.StartedFromBeginning.ShouldBe(true);
        existingPgcr.DataVersion.ShouldBe(1);
        existingPlayer.Completed.ShouldBe(true);
        existingPlayer.StartSeconds.ShouldBe(2);
        await pgcrRepository.Received(1).UpdateManyAsync(
            Arg.Any<IEnumerable<Destiny2Pgcr>>(), Arg.Any<CancellationToken>());
        await playerRepository.Received(1).UpdateManyAsync(
            Arg.Any<IEnumerable<Destiny2PgcrPlayer>>(), Arg.Any<CancellationToken>());
        await pgcrRepository.DidNotReceive().InsertManyAsync(
            Arg.Any<IEnumerable<Destiny2Pgcr>>(), Arg.Any<CancellationToken>());
        await playerRepository.DidNotReceive().InsertManyAsync(
            Arg.Any<IEnumerable<Destiny2PgcrPlayer>>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
