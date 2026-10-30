using NSubstitute;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Core.Services.ERA.Equipment.Models;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Core.Services.ERA.Team;
using Void.Bot.Core.Services.ERA.World;
using Void.Bot.Core.Services.ERA.World.Models;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.UnitTest.ERA;

public class EraWorldEquipmentFixture
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
        var request = new GetEraWorldEquipmentRequest { WorldInstanceId = world.Id, MemberId = member, AuthorityEpoch = 3 };
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
        var equipment = Substitute.For<IEraEquipmentDataProvider>();
        await Should.ThrowAsync<EraWorldAccessException>(() => new EraWorldEquipmentService(worlds, teams, players,
            equipment, Substitute.For<ILocalizationService>(), TimeProvider.System).GetAsync(request, CancellationToken.None));
        await equipment.DidNotReceive().GetCharacterSnapshotAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_return_equipped_weapons_without_exposing_the_members_inventory()
    {
        var player = new EraPlayer { Id = Guid.NewGuid() };
        var character = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var world = new EraWorld { Id = Guid.NewGuid(), TeamId = Guid.NewGuid(), State = EraWorldState.Ready,
            HostPlayerId = player.Id, AuthorityEpoch = 3, Version = 1 };
        var players = Substitute.For<IEraPlayerContext>();
        players.RequireCurrentAsync(false, Arg.Any<CancellationToken>()).Returns(player);
        var worlds = Substitute.For<IEraWorldDataProvider>();
        worlds.ReadSnapshotAsync(world.Id, player.Id, Arg.Any<CancellationToken>()).Returns(new EraWorldReadSnapshot(world,
            new[] { new EraWorldMember { PlayerId = player.Id, CharacterId = character, MemberId = memberId, IsContinuing = true,
                ActivePlayerId = player.Id, ActiveCharacterId = character } }));
        worlds.GetWorldAsync(world.Id, Arg.Any<CancellationToken>()).Returns(world);
        var teams = Substitute.For<IEraTeamDataProvider>();
        teams.GetTeamAsync(world.TeamId, Arg.Any<CancellationToken>()).Returns(new EraTeam
        {
            HostPlayerId = player.Id, AuthorityEpoch = 3, ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
            Members = new List<EraTeamMember> { new EraTeamMember { PlayerId = player.Id, CharacterId = character,
                MemberId = memberId, ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5) } }
        });
        var equipment = Substitute.For<IEraEquipmentDataProvider>();
        equipment.GetConfigurationAsync(Arg.Any<CancellationToken>()).Returns(new EraEquipmentConfiguration
            { Version = 8, Payload = "{\"SchemaVersion\":3}" });
        var catalog = EraWeaponFixture.Catalog();
        var configuration = catalog.SlotConfigurations.Single(item => item.Id == catalog.Templates[0].SlotConfigurationId);
        equipment.GetWeaponCatalogAsync(Arg.Any<CancellationToken>()).Returns(catalog);
        var snapshot = new EraEquipmentReadSnapshot { CharacterId = character, PlayerId = player.Id, EquipmentVersion = 5 };
        foreach (var equipped in new[] { false, true })
        {
            var item = new EraEquipmentInstance
            {
                Id = Guid.NewGuid(), OwnerPlayerId = player.Id, CharacterId = character, TemplateId = catalog.Templates[0].Id,
                GenerationManifestVersion = 8, EquipmentType = EraEquipmentType.Weapon, Grade = EraEquipmentGrade.Common,
                Location = equipped ? EraEquipmentLocation.Equipped : EraEquipmentLocation.Inventory,
                EquippedSlot = equipped ? EraEquipmentSlot.Weapon1 : null, Version = 1, CreatedAt = DateTimeOffset.UtcNow
            };
            snapshot.Entries.Add(new EraEquipmentReadEntry(item, null, EraWeaponGenerator.Generate(configuration, _ => 0)));
        }
        equipment.GetCharacterSnapshotAsync(character, player.Id, Arg.Any<CancellationToken>()).Returns(snapshot);
        var response = await new EraWorldEquipmentService(worlds, teams, players, equipment, Substitute.For<ILocalizationService>(), TimeProvider.System)
            .GetAsync(new GetEraWorldEquipmentRequest { WorldInstanceId = world.Id, MemberId = memberId, AuthorityEpoch = 3 }, CancellationToken.None);
        response.Data!.Members.Single().Equipment.Single().Location.ShouldBe(EraEquipmentLocation.Equipped);
        response.Data.ManifestVersion.ShouldBe(8);
    }
}
