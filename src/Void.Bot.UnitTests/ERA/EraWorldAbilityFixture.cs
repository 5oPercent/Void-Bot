using NSubstitute;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Services.ERA.AbilityProfile;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Core.Services.ERA.Team;
using Void.Bot.Core.Services.ERA.World;
using Void.Bot.Core.Services.ERA.World.Models;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.UnitTest.ERA;

public class EraWorldAbilityFixture
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Should_reject_a_stale_sql_host_or_redis_authority_before_reading_assets(bool staleSqlHost, bool staleRedisEpoch)
    {
        var player = new EraPlayer { Id = Guid.NewGuid() };
        var character = Guid.NewGuid();
        var member = Guid.NewGuid();
        var world = new EraWorld { Id = Guid.NewGuid(), TeamId = Guid.NewGuid(), State = EraWorldState.Ready,
            HostPlayerId = staleSqlHost ? Guid.NewGuid() : player.Id, AuthorityEpoch = 3, Version = 1 };
        var request = new GetEraWorldAbilityProfilesRequest { WorldInstanceId = world.Id, MemberId = member, AuthorityEpoch = 3 };
        var players = Substitute.For<IEraPlayerContext>();
        players.RequireCurrentAsync(false, Arg.Any<CancellationToken>()).Returns(player);
        var worlds = Substitute.For<IEraWorldDataProvider>();
        worlds.ReadSnapshotAsync(world.Id, player.Id, Arg.Any<CancellationToken>()).Returns(new EraWorldReadSnapshot(world,
            new[] { new EraWorldMember { PlayerId = player.Id, CharacterId = character, MemberId = member, IsContinuing = true,
                ActivePlayerId = player.Id, ActiveCharacterId = character } }));
        var teams = Substitute.For<IEraTeamDataProvider>();
        teams.GetTeamAsync(world.TeamId, Arg.Any<CancellationToken>()).Returns(new EraTeam
        {
            HostPlayerId = player.Id, AuthorityEpoch = staleRedisEpoch ? 2 : 3, ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
            Members = new List<EraTeamMember> { new EraTeamMember { PlayerId = player.Id, CharacterId = character,
                MemberId = member, ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5) } }
        });
        var equipment = Substitute.For<IEraAbilityProfileDataProvider>();
        await Should.ThrowAsync<EraWorldAccessException>(() => new EraWorldAbilityService(worlds, teams, players,
            equipment, Substitute.For<ILocalizationService>(), TimeProvider.System).GetAsync(request, CancellationToken.None));
        await equipment.DidNotReceive().GetProfileAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

}
