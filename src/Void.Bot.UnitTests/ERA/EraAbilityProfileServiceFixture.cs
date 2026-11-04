using System.Net;
using System.Text.Json;
using AutoMapper;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Mappings;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Core.Services.ERA.Player.Models;
using Void.Bot.Core.Services.ERA.AbilityProfile;
using Void.Bot.Core.Services.ERA.Configuration;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.UnitTest.ERA;

public class EraAbilityProfileServiceFixture
{
    private readonly IEraAbilityProfileDataProvider _data = Substitute.For<IEraAbilityProfileDataProvider>();
    private readonly IEraAbilityCatalogQueryService _catalog = Substitute.For<IEraAbilityCatalogQueryService>();
    private readonly IEraDataProvider _players = Substitute.For<IEraDataProvider>();
    private readonly ICurrentUser _user = Substitute.For<ICurrentUser>();
    private readonly IMapper _mapper;
    private readonly Guid _accountId = Guid.NewGuid();
    private readonly EraPlayer _player = new EraPlayer()
    {
        Id = Guid.NewGuid()
    };
    private readonly Guid _characterId = Guid.NewGuid();

    public EraAbilityProfileServiceFixture()
    {
        var configuration = new MapperConfiguration(ConfigureMappings);
        _mapper = configuration.CreateMapper();
    }

    private static void ConfigureMappings(IMapperConfigurationExpression configuration)
    {
        configuration.AddProfile<EraResonanceMapping>();
    }

    private EraAbilityProfileService Service()
    {
        _user.Id.Returns(_accountId);
        _players.LockPlayerByAccountIdAsync(_accountId, Arg.Any<CancellationToken>()).Returns(_player);
        _players.GetPlayerByAccountIdAsync(_accountId, Arg.Any<CancellationToken>()).Returns(_player);
        _players.GetCharacterByIdAsync(_characterId, Arg.Any<CancellationToken>()).Returns(new EraCharacter { Id = _characterId, PlayerId = _player.Id });
        _players.LockCharacterAsync(_characterId, Arg.Any<CancellationToken>()).Returns(new EraCharacter { Id = _characterId, PlayerId = _player.Id });
        _data.GetLearnedResonancesAsync(_characterId, Arg.Any<CancellationToken>()).Returns(new List<string>());
        return new EraAbilityProfileService(
            _data,
            new EraCharacterContext(_players, new EraPlayerContext(_players, _user, Substitute.For<ILocalizationService>(), new EraPlayerData()), Substitute.For<ILocalizationService>()),
            Substitute.For<ILocalizationService>(),
            _mapper,
            _catalog);
    }

    private static ResonanceConfiguration Config()
    {
        return new ResonanceConfiguration()
        {
            Systems = new List<LatticeDefinition>()
            {
                new LatticeDefinition()
                {
                    Id = "support",
                    PrimarySkills = new List<string>()
                    {
                        "heal"
                    }
                }
            },
            Skills = new List<ResonanceDefinition>()
            {
                new ResonanceDefinition()
                {
                    Id = "heal",
                    IsDisabled = true,
                    Variants = new List<ResonanceVariant>()
                    {
                        new ResonanceVariant()
                        {
                            Slot = ResonanceSlotKind.Primary,
                            CarrierKind = ResonanceCarrierKind.Self, Condition = new()
                        }
                    }
                }
            }
        };
    }

    private static CharacterResonanceProfile Profile()
    {
        return new CharacterResonanceProfile()
        {
            SelectedSystemId = "support",
            Loadouts = new List<AbilityLoadout>()
            {
                new AbilityLoadout()
                {
                    SystemId = "support",
                    Slots = new string?[]
                    {
                        null,
                        "heal",
                        null,
                        null
                    }
                }
            }
        };
    }

    private void ConfigureRead()
    {
        _catalog.ReadConfigurationAsync(Arg.Any<CancellationToken>()).Returns(Config());
    }

    [Fact]
    public async Task Save_DisabledResonanceIsAllowed()
    {
        var service = Service();
        ConfigureRead();
        var result = await service.SaveProfileAsync(new SaveEraResonanceProfileCommand()
        {
            CharacterId = _characterId,
            Profile = Profile()
        }, CancellationToken.None);
        result.Code.ShouldBe(HttpStatusCode.OK);
        result.Data!.Version.ShouldBe(1);
        result.Data.Loadouts[0].Slots[1].ShouldBe("heal");
        await _data.Received(1).CreateProfileAsync(Arg.Is<EraCharacterResonanceProfile>(p => p.CharacterId == _characterId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Save_UnlearnedResonanceIsAllowed()
    {
        var service = Service();
        var configuration = Config();
        configuration.Skills.Single().IsDisabled = false;
        _catalog.ReadConfigurationAsync(Arg.Any<CancellationToken>()).Returns(configuration);
        var result = await service.SaveProfileAsync(new SaveEraResonanceProfileCommand()
        {
            CharacterId = _characterId,
            Profile = Profile()
        }, CancellationToken.None);
        result.Code.ShouldBe(HttpStatusCode.OK);
        result.Data!.Version.ShouldBe(1);
        result.Data.Loadouts[0].Slots[1].ShouldBe("heal");
        await _data.Received(1).CreateProfileAsync(Arg.Is<EraCharacterResonanceProfile>(p =>
            p.CharacterId == _characterId && p.Version == 1), Arg.Any<CancellationToken>());
        await _data.DidNotReceive().GetLearnedResonancesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("missing", null)]
    [InlineData(null, "heal")]
    [InlineData("heal", "heal")]
    public async Task Save_InvalidPoolOrDuplicateDoesNotWriteProfile(string? primary, string? secondary)
    {
        var service = Service();
        ConfigureRead();
        var profile = Profile();
        profile.Loadouts[0].Slots[1] = primary;
        profile.Loadouts[0].Slots[2] = secondary;
        var result = await service.SaveProfileAsync(new SaveEraResonanceProfileCommand()
        {
            CharacterId = _characterId,
            Profile = profile
        }, CancellationToken.None);
        result.ErrorCode.ShouldBe("era.skills_invalid_loadout");
        await _data.DidNotReceive().CreateProfileAsync(Arg.Any<EraCharacterResonanceProfile>(), Arg.Any<CancellationToken>());
        _data.DidNotReceive().UpdateProfile(Arg.Any<EraCharacterResonanceProfile>());
    }

    [Fact]
    public async Task Save_StaleVersionDoesNotOverwriteProfile()
    {
        var service = Service();
        _data.GetProfileAsync(_characterId, Arg.Any<CancellationToken>()).Returns(new EraCharacterResonanceProfile()
        {
            Version = 2
        });
        var result = await service.SaveProfileAsync(new SaveEraResonanceProfileCommand()
        {
            CharacterId = _characterId,
            ExpectedVersion = 1,
            Profile = Profile()
        }, CancellationToken.None);
        result.ErrorCode.ShouldBe("era.skills_version_conflict");
        _data.DidNotReceive().UpdateProfile(Arg.Any<EraCharacterResonanceProfile>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Save_TacticalSkillMustBelongToSelectedLattice(bool includedInLattice)
    {
        var service = Service();
        var configuration = Config();
        configuration.Skills.Add(new ResonanceDefinition
        {
            Id = "tactical.skill",
            Variants = new List<ResonanceVariant>
            {
                new ResonanceVariant { Slot = ResonanceSlotKind.Tactical, CarrierKind = ResonanceCarrierKind.Self, Condition = new() }
            }
        });
        if (includedInLattice)
            configuration.Systems.Single().TacticalSkills.Add("tactical.skill");
        _catalog.ReadConfigurationAsync(Arg.Any<CancellationToken>()).Returns(configuration);
        var profile = Profile();
        profile.Loadouts.Single().Slots[3] = "tactical.skill";

        var result = await service.SaveProfileAsync(new SaveEraResonanceProfileCommand
        {
            CharacterId = _characterId,
            Profile = profile
        }, CancellationToken.None);

        result.Code.ShouldBe(includedInLattice ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
        if (!includedInLattice)
            await _data.DidNotReceive().CreateProfileAsync(Arg.Any<EraCharacterResonanceProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetProfile_DoesNotCreateDataOnRead()
    {
        var service = Service();
        var result = await service.GetProfileAsync(new GetEraResonanceProfileRequest { CharacterId = _characterId }, CancellationToken.None);
        result.Data!.Version.ShouldBe(0);
        result.Data.Loadouts.ShouldBeEmpty();
        await _data.DidNotReceive().CreateProfileAsync(Arg.Any<EraCharacterResonanceProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetProfile_AnotherAccountsCharacterDoesNotReadResonanceData()
    {
        var service = Service();
        _players.GetCharacterByIdAsync(_characterId, Arg.Any<CancellationToken>()).Returns(new EraCharacter
        {
            Id = _characterId,
            PlayerId = Guid.NewGuid()
        });

        var exception = await Should.ThrowAsync<EraPlayerAccessException>(() => service.GetProfileAsync(
            new GetEraResonanceProfileRequest { CharacterId = _characterId }, CancellationToken.None));

        exception.ErrorCode.ShouldBe("era.character_not_found");
        await _data.DidNotReceive().GetProfileAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetProfile_TwoCharactersUseIndependentProfiles()
    {
        var service = Service();
        var secondCharacterId = Guid.NewGuid();
        _players.GetCharacterByIdAsync(secondCharacterId, Arg.Any<CancellationToken>()).Returns(new EraCharacter
        {
            Id = secondCharacterId,
            PlayerId = _player.Id
        });
        _data.GetProfileAsync(_characterId, Arg.Any<CancellationToken>()).Returns(new EraCharacterResonanceProfile
        {
            CharacterId = _characterId,
            Version = 2,
            Payload = JsonSerializer.Serialize(Profile())
        });
        _data.GetProfileAsync(secondCharacterId, Arg.Any<CancellationToken>()).Returns(new EraCharacterResonanceProfile
        {
            CharacterId = secondCharacterId,
            Version = 7,
            Payload = JsonSerializer.Serialize(new CharacterResonanceProfile())
        });

        var first = await service.GetProfileAsync(new GetEraResonanceProfileRequest { CharacterId = _characterId }, CancellationToken.None);
        var second = await service.GetProfileAsync(new GetEraResonanceProfileRequest { CharacterId = secondCharacterId }, CancellationToken.None);

        first.Data!.Version.ShouldBe(2);
        first.Data.Loadouts.ShouldHaveSingleItem();
        second.Data!.Version.ShouldBe(7);
        second.Data.Loadouts.ShouldBeEmpty();
    }
}
