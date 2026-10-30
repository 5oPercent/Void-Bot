using System.Text.Json;
using Autofac;
using Mediator.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Account;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Services.ERA.Team;
using Void.Bot.Core.Settings.System;
using Void.Bot.Core.Settings.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

/// <summary>ERA 装备、世界与奖励集成测试，共用 MySQL、Redis、DbUp、Respawn 和真实 Mediator 工作单元。</summary>
[Trait("Category", "ERAIntegration")]
public partial class EraIntegrationFixture : IntegrationFixture
{
    private static readonly Guid ConfigurationId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    private static readonly Guid CurrentUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private readonly ILifetimeScope _root;
    private readonly Guid _playerId = Guid.NewGuid();
    private readonly Guid _characterId = Guid.NewGuid();
    private readonly Guid _otherPlayerId = Guid.NewGuid();
    private readonly Guid _otherCharacterId = Guid.NewGuid();
    private readonly Guid _otherAccountId = Guid.NewGuid();
    private bool _enableWorldNetworking;
    private int? _worldTestMaxMembers;
    private EraWorldTestClock? _worldTestClock;
    private IEraTeamDataProvider? _worldTestTeamProvider;
    private readonly List<EraEquipmentInstance> _inventory = new List<EraEquipmentInstance>();
    private EraEquipmentInstance? _equipped;

    public EraIntegrationFixture(IntegrationTestBase testBase) : base(testBase)
    {
        _root = testBase.LifetimeScope;
    }

    private ILifetimeScope Scope(DbCommandInterceptor? interceptor = null, Guid? accountId = null)
    {
        return _root.BeginLifetimeScope(builder =>
        {
            // 文案查询不属于本组验证范围；玩家、角色、装备、SQL 和事务均不替换。
            builder.RegisterInstance(Substitute.For<ILocalizationService>()).As<ILocalizationService>();
            if (_worldTestClock != null)
            {
                builder.RegisterInstance(_worldTestClock).As<TimeProvider>();
                builder.RegisterInstance(new EraWorldLifetimeSetting(new ConfigurationBuilder().Build()));
            }
            if (_worldTestTeamProvider != null) builder.RegisterInstance(_worldTestTeamProvider).As<IEraTeamDataProvider>();
            if (_enableWorldNetworking || _worldTestMaxMembers.HasValue)
            {
                var overrides = new Dictionary<string, string?>();
                if (_enableWorldNetworking)
                {
                    overrides["Era:Networking:Enabled"] = "true";
                    overrides["Era:Networking:PublicHost"] = "localhost";
                }
                if (_worldTestMaxMembers.HasValue)
                {
                    overrides["Era:Networking:MaxMembers"] = _worldTestMaxMembers.Value.ToString();
                }
                var configuration = new ConfigurationBuilder().AddConfiguration(_root.Resolve<IConfiguration>())
                    .AddInMemoryCollection(overrides).Build();
                builder.RegisterInstance(new EraNetworkingSetting(configuration));
            }
            if (accountId.HasValue)
            {
                var user = Substitute.For<ICurrentUser>();
                user.Id.Returns(accountId);
                user.UserName.Returns("WorldIntegrationUser");
                builder.RegisterInstance(user).As<ICurrentUser>();
            }
            if (interceptor != null)
            {
                builder.Register(context => new EraIntegrationDbContext(
                        context.Resolve<ConnectionStringSetting>(), context.Resolve<ICurrentUser>(), interceptor))
                    .As<VoidBotDbContext>().As<DbContext>().As<IUnitOfWork>().InstancePerLifetimeScope();
            }
        });
    }

    private async Task<T> RequestAsync<T>(Func<IMediator, CancellationToken, Task<T>> action,
        DbCommandInterceptor? interceptor = null, CancellationToken cancellationToken = default, Guid? accountId = null)
    {
        await using var scope = Scope(interceptor, accountId);
        return await action(scope.Resolve<IMediator>(), cancellationToken).ConfigureAwait(false);
    }

    private async Task ReadDatabaseAsync(Func<VoidBotDbContext, Task> action)
    {
        // 每次核对都使用新作用域，不能由同一 ChangeTracker 的旧值冒充持久化成功。
        await using var scope = Scope();
        await action(scope.Resolve<VoidBotDbContext>()).ConfigureAwait(false);
    }

    private async Task SeedAsync(int inventoryCount = 1, bool equipped = false)
    {
        await using var scope = Scope();
        var context = scope.Resolve<VoidBotDbContext>();
        await using var transaction = await context.Database.BeginTransactionAsync();
        context.Set<UserAccount>().AddRange(
            new UserAccount { Id = CurrentUserId, Name = "era-equipment-owner", Suffix = "1001", Password = "test-only" },
            new UserAccount { Id = _otherAccountId, Name = "era-equipment-other", Suffix = "1002", Password = "test-only" });
        await context.SaveChangesAsync();
        context.Set<EraPlayer>().AddRange(
            new EraPlayer { Id = _playerId, WarehouseCapacity = 300, Version = 1 },
            new EraPlayer { Id = _otherPlayerId, WarehouseCapacity = 300, Version = 1 });
        await context.SaveChangesAsync();
        context.Set<EraPlayerAccountLink>().AddRange(
            new EraPlayerAccountLink { Id = Guid.NewGuid(), AccountId = CurrentUserId, PlayerId = _playerId },
            new EraPlayerAccountLink { Id = Guid.NewGuid(), AccountId = _otherAccountId, PlayerId = _otherPlayerId });
        context.Set<EraCharacter>().AddRange(
            new EraCharacter
            {
                Id = _characterId, PlayerId = _playerId, CreationRequestId = Guid.NewGuid(),
                CharacterDefinitionId = "test-character", DisplayName = "Owner", Version = 12, EquipmentVersion = 3
            },
            new EraCharacter
            {
                Id = _otherCharacterId, PlayerId = _otherPlayerId, CreationRequestId = Guid.NewGuid(),
                CharacterDefinitionId = "test-character", DisplayName = "Other", Version = 12, EquipmentVersion = 3
            });
        await context.SaveChangesAsync();

        // Respawn 会清掉种子行，首次测试则可能保留 DbUp 种子；两种情况都显式准备目录。
        var configuration = await context.Set<EraEquipmentConfiguration>().SingleOrDefaultAsync(row => row.Id == ConfigurationId);
        if (configuration == null)
        {
            configuration = new EraEquipmentConfiguration { Id = ConfigurationId };
            context.Set<EraEquipmentConfiguration>().Add(configuration);
        }
        configuration.Version = 7;
        var armorManifest = new EraEquipmentManifestDto
        {
            Armors = new EraArmorCatalogDto { Templates = EraEquipmentRules.ArmorSlots.Select(slot => new EraArmorTemplateDto
            {
                Id = Guid.Parse($"10000000-0000-0000-0000-{(int)slot:000000000000}"), Name = "Test armor", PresentationKey = $"armor_test_{(int)slot}",
                CompatibleSlots = new List<EraEquipmentSlot> { slot }
            }).ToList() }
        };
        configuration.Payload = JsonSerializer.Serialize(new EraEquipmentManifestDto());
        context.Set<EraArmorTemplate>().AddRange(armorManifest.Armors.Templates.Select(template => new EraArmorTemplate
        {
            Id = template.Id, Name = template.Name, Icon = template.Icon, Note = template.Note, IsGenerationEnabled = true, PresentationKey = template.PresentationKey,
            CompatibleSlotsPayload = JsonSerializer.Serialize(template.CompatibleSlots)
        }));

        for (var index = 0; index < inventoryCount; index++)
        {
            _inventory.Add(NewItem());
        }
        var items = new List<EraEquipmentInstance>(_inventory);
        if (equipped)
        {
            _equipped = NewItem(EraEquipmentLocation.Equipped);
            items.Add(_equipped);
        }
        items.Add(NewItem(playerId: _otherPlayerId, characterId: _otherCharacterId));
        AddItems(context, items);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private EraEquipmentInstance NewItem(EraEquipmentLocation location = EraEquipmentLocation.Inventory,
        Guid? playerId = null, Guid? characterId = null)
    {
        return new EraEquipmentInstance
        {
            Id = Guid.NewGuid(), TemplateId = Guid.Parse("10000000-0000-0000-0000-000000000001"), GenerationManifestVersion = 1,
            EquipmentType = EraEquipmentType.Armor, Grade = EraEquipmentGrade.Common,
            OwnerPlayerId = location == EraEquipmentLocation.Ground ? null : playerId ?? _playerId,
            CharacterId = location is EraEquipmentLocation.Inventory or EraEquipmentLocation.Equipped
                ? characterId ?? _characterId : null,
            Location = location,
            EquippedSlot = location == EraEquipmentLocation.Equipped ? EraEquipmentSlot.Head : null,
            Version = 5, CreatedAt = new DateTimeOffset(2026, 10, 20, 0, 0, 0, TimeSpan.Zero)
        };
    }

    private static void AddItems(VoidBotDbContext context, IEnumerable<EraEquipmentInstance> items)
    {
        foreach (var item in items)
        {
            context.Set<EraEquipmentInstance>().Add(item);
            context.Set<EraArmorInstance>().Add(new EraArmorInstance
            {
                EquipmentInstanceId = item.Id, GenerationRuleVersion = 42,
                Strength = 120, Survival = 15, Mobility = 9, Focus = 6, Efficiency = 6, Resistance = 3
            });
        }
    }

    private EquipEraEquipmentCommand Equip(EraEquipmentInstance candidate, Guid? requestId = null)
    {
        return new EquipEraEquipmentCommand
        {
            CharacterId = _characterId, RequestId = requestId ?? Guid.NewGuid(),
            EquipmentInstanceId = candidate.Id, Slot = EraEquipmentSlot.Head,
            ExpectedEquipmentVersion = 3, ExpectedItemVersion = 5, ExpectedManifestVersion = 7,
            ExpectedEquippedEquipmentInstanceId = _equipped?.Id, ExpectedEquippedVersion = _equipped?.Version
        };
    }

    private Task<EquipEraEquipmentResponse> SendEquipAsync(EquipEraEquipmentCommand command,
        DbCommandInterceptor? interceptor = null, CancellationToken cancellationToken = default)
    {
        return RequestAsync((mediator, token) => mediator.SendAsync<EquipEraEquipmentCommand, EquipEraEquipmentResponse>(command, token),
            interceptor, cancellationToken);
    }

    private Task<UnequipEraEquipmentResponse> SendUnequipAsync(Guid equipmentId, long equipmentVersion = 3, long itemVersion = 5)
    {
        return RequestAsync((mediator, token) => mediator.SendAsync<UnequipEraEquipmentCommand, UnequipEraEquipmentResponse>(
            new UnequipEraEquipmentCommand
            {
                CharacterId = _characterId, RequestId = Guid.NewGuid(), EquipmentInstanceId = equipmentId,
                Slot = EraEquipmentSlot.Head, ExpectedEquipmentVersion = equipmentVersion, ExpectedItemVersion = itemVersion
            }, token));
    }

    private async Task AssertOriginalStateAsync()
    {
        await ReadDatabaseAsync(async context =>
        {
            var character = await context.Set<EraCharacter>().AsNoTracking().SingleAsync(row => row.Id == _characterId);
            character.EquipmentVersion.ShouldBe(3);
            character.Version.ShouldBe(12);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync(row => row.PlayerId == _playerId)).ShouldBe(0);
            var actual = await context.Set<EraEquipmentInstance>().AsNoTracking()
                .Where(row => row.CharacterId == _characterId).ToListAsync();
            var expected = new List<EraEquipmentInstance>(_inventory);
            if (_equipped != null) expected.Add(_equipped);
            actual.Count.ShouldBe(expected.Count);
            foreach (var original in expected)
            {
                var item = actual.Single(row => row.Id == original.Id);
                item.OwnerPlayerId.ShouldBe(_playerId);
                item.Location.ShouldBe(original.Location);
                item.EquippedSlot.ShouldBe(original.EquippedSlot);
                item.Version.ShouldBe(original.Version);
                item.CreatedAt.ShouldBe(original.CreatedAt);
                var armor = await context.Set<EraArmorInstance>().AsNoTracking().SingleAsync(row => row.EquipmentInstanceId == item.Id);
                armor.Strength.ShouldBe(120);
                armor.GenerationRuleVersion.ShouldBe(42);
            }
        });
    }
}
