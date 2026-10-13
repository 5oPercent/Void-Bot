using System.Net;
using System.Text.Json;
using AutoMapper;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Mappings;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.UnitTest.ERA;

public class EraSkillServiceFixture
{
    private readonly IEraSkillDataProvider _data = Substitute.For<IEraSkillDataProvider>();
    private readonly IEraDataProvider _players = Substitute.For<IEraDataProvider>();
    private readonly ICurrentUser _user = Substitute.For<ICurrentUser>();
    private readonly IMapper _mapper;
    private readonly Guid _accountId = Guid.NewGuid();
    private readonly EraPlayer _player = new EraPlayer()
    {
        Id = Guid.NewGuid()
    };
    public EraSkillServiceFixture()
    {
        var configuration = new MapperConfiguration(ConfigureMappings);
        _mapper = configuration.CreateMapper();
    }

    private static void ConfigureMappings(IMapperConfigurationExpression configuration)
    {
        configuration.AddProfile<EraSkillMapping>();
    }

    private EraSkillService Service()
    {
        _user.Id.Returns(_accountId);
        _players.LockPlayerByAccountIdAsync(_accountId, Arg.Any<CancellationToken>()).Returns(_player);
        _players.GetPlayerByAccountIdAsync(_accountId, Arg.Any<CancellationToken>()).Returns(_player);
        _data.GetLearnedSkillsAsync(_player.Id, Arg.Any<CancellationToken>()).Returns(new List<string>());
        return new EraSkillService(
            _data,
            new EraPlayerContext(_players, _user, Substitute.For<ILocalizationService>(), new EraPlayerData()),
            _user,
            Substitute.For<ILocalizationService>(),
            _mapper);
    }

    private static SkillConfiguration Config()
    {
        return new SkillConfiguration()
        {
            Version = 1,
            Systems = new List<AbilitySystemDefinition>()
            {
                new AbilitySystemDefinition()
                {
                    Id = "support",
                    PrimarySkills = new List<string>()
                    {
                        "heal"
                    }
                }
            },
            Skills = new List<SkillDefinition>()
            {
                new SkillDefinition()
                {
                    Id = "heal",
                    Variants = new List<SkillVariant>()
                    {
                        new SkillVariant()
                        {
                            Slot = SkillSlotKind.Primary,
                            CarrierId = "self"
                        }
                    }
                }
            },
            DisabledSkills = new List<string>()
            {
                "heal"
            }
        };
    }

    private static PlayerSkillProfile Profile()
    {
        return new PlayerSkillProfile()
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
        _data.GetConfigurationAsync(false, Arg.Any<CancellationToken>()).Returns(new EraSkillConfiguration()
        {
            Version = 1,
            Payload = JsonSerializer.Serialize(Config())
        });
    }

    [Fact]
    public async Task Save_DisabledLearnedSkillIsAllowed()
    {
        var service = Service();
        ConfigureRead();
        _data.GetLearnedSkillsAsync(_player.Id, Arg.Any<CancellationToken>()).Returns(new List<string>()
        {
            "heal"
        });
        var result = await service.SaveProfileAsync(new SaveEraSkillProfileCommand()
        {
            Profile = Profile()
        }, CancellationToken.None);
        result.Code.ShouldBe(HttpStatusCode.OK);
        result.Data!.Version.ShouldBe(1);
        result.Data.Loadouts[0].Slots[1].ShouldBe("heal");
        await _data.Received(1).CreateProfileAsync(Arg.Is<EraPlayerSkillProfile>(p => p.PlayerId == _player.Id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Save_UnlearnedSkillDoesNotWriteProfile()
    {
        var service = Service();
        ConfigureRead();
        var result = await service.SaveProfileAsync(new SaveEraSkillProfileCommand()
        {
            Profile = Profile()
        }, CancellationToken.None);
        result.ErrorCode.ShouldBe("era.skills_invalid_loadout");
        await _data.DidNotReceive().CreateProfileAsync(Arg.Any<EraPlayerSkillProfile>(), Arg.Any<CancellationToken>());
        _data.DidNotReceive().UpdateProfile(Arg.Any<EraPlayerSkillProfile>());
    }

    [Fact]
    public async Task Save_StaleVersionDoesNotOverwriteProfile()
    {
        var service = Service();
        _data.GetProfileAsync(_player.Id, Arg.Any<CancellationToken>()).Returns(new EraPlayerSkillProfile()
        {
            Version = 2
        });
        var result = await service.SaveProfileAsync(new SaveEraSkillProfileCommand()
        {
            ExpectedVersion = 1,
            Profile = Profile()
        }, CancellationToken.None);
        result.ErrorCode.ShouldBe("era.skills_version_conflict");
        _data.DidNotReceive().UpdateProfile(Arg.Any<EraPlayerSkillProfile>());
    }

    [Fact]
    public async Task GetProfile_DoesNotCreateDataOnRead()
    {
        var service = Service();
        var result = await service.GetProfileAsync(new GetEraSkillProfileRequest(), CancellationToken.None);
        result.Data!.Version.ShouldBe(0);
        result.Data.Loadouts.ShouldBeEmpty();
        await _data.DidNotReceive().CreateProfileAsync(Arg.Any<EraPlayerSkillProfile>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Publish_StaleVersionDoesNotChangeConfiguration()
    {
        var service = Service();
        var row = new EraSkillConfiguration()
        {
            Version = 2,
            Payload = JsonSerializer.Serialize(Config())
        };
        _data.GetConfigurationAsync(true, Arg.Any<CancellationToken>()).Returns(row);
        string original = row.Payload;
        _data.GetDraftWorkspaceAsync(true,Arg.Any<CancellationToken>()).Returns(new EraSkillDraftWorkspace
        {
            Id=EraSkillDataProvider.DraftWorkspaceId,Initialized=true,Revision=1,
            SettingsPayload=JsonSerializer.Serialize(new EraSkillDraftSettings())
        });
        _data.GetDraftRecordsAsync(true,Arg.Any<CancellationToken>()).Returns(new List<EraSkillDraftRecord>());
        var result = await service.PublishDraftAsync(new PublishEraSkillDraftCommand
        {
            ExpectedDraftRevision=1,ExpectedPublishedVersion=1,RequestId=Guid.NewGuid()
        }, CancellationToken.None);
        result.ErrorCode.ShouldBe("era.draft_version_conflict");
        row.Version.ShouldBe(2);
        row.Payload.ShouldBe(original);
    }
}
