using System.Net;
using System.Text.Json;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Resonance;
using Void.Bot.Core.Services.ERA.Configuration;
using Void.Bot.Core.Services.ERA.Lattice;
using Void.Bot.Core.Services.ERA.Effect;
using Void.Bot.Core.Services.ERA.Enemy;
using Void.Bot.Core.Services.ERA.CombatRelation;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.UnitTest.ERA;

public sealed partial class EraConfigurationServicesFixture
{
    private readonly IEraLatticeDataProvider _lattices = Substitute.For<IEraLatticeDataProvider>();
    private readonly IEraResonanceDataProvider _skills = Substitute.For<IEraResonanceDataProvider>();
    private readonly IEraEffectDataProvider _effects = Substitute.For<IEraEffectDataProvider>();
    private readonly IEraEnemyDataProvider _enemies = Substitute.For<IEraEnemyDataProvider>();
    private readonly IEraCombatRelationDataProvider _relations = Substitute.For<IEraCombatRelationDataProvider>();
    private readonly IEraConfigurationStateProvider _states = Substitute.For<IEraConfigurationStateProvider>();
    private readonly EraDataEditState _workspace = new()
    {
        Id = EraConfigurationStateProvider.DataStateId, Revision = 1, SettingsVersion = 1,
        SettingsPayload = JsonSerializer.Serialize(new EraDataSettings())
    };
    private readonly List<IEraDataDefinition> _records = new();

    private void ConfigureStore<TRecord>(IEraDefinitionDataProvider<TRecord> provider) where TRecord : class, IEraDataDefinition
    {
        provider.GetDefinitionsAsync(Arg.Any<CancellationToken>()).Returns(_ => _records.OfType<TRecord>().ToList());
        provider.GetDefinitionAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => _records.OfType<TRecord>().SingleOrDefault(record => record.StableId == call.ArgAt<string>(0)));
        provider.AddAsync(Arg.Any<TRecord>(), Arg.Any<CancellationToken>()).Returns(call =>
        { _records.Add(call.ArgAt<TRecord>(0)); return Task.CompletedTask; });
        provider.DeleteAsync(Arg.Any<TRecord>(), Arg.Any<CancellationToken>()).Returns(call =>
        { _records.Remove(call.ArgAt<TRecord>(0)); return Task.CompletedTask; });
    }

    private sealed record TestServices(EraLatticeService Lattices, EraResonanceService Skills, EraEffectService Effects,
        EraEnemyService Enemies, EraCombatRelationService Relations, EraAbilityCatalogQueryService Catalog);

    private TestServices Service()
    {
        var user = Substitute.For<ICurrentUser>();
        user.Id.Returns(Guid.NewGuid());
        var localization = Substitute.For<ILocalizationService>();
        ConfigureStore(_lattices); ConfigureStore(_skills); ConfigureStore(_effects); ConfigureStore(_enemies);
        _states.GetAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(_workspace);
        _states.When(provider => provider.Touch(_workspace)).Do(_ =>
        { _workspace.Revision++; _workspace.LastModifiedBy = user.Id; _workspace.UpdatedAt = DateTimeOffset.UtcNow; });
        _relations.GetAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(_ => new EraCombatRelationConfiguration
        {
            Id = _workspace.Id,
            Payload = JsonSerializer.Serialize(JsonSerializer.Deserialize<EraDataSettings>(_workspace.SettingsPayload)!.CombatRelations ?? new())
        });
        var catalog = new EraAbilityCatalogQueryService(_states, _lattices, _skills, _effects, _enemies, _relations, localization);
        return new(new(_lattices, catalog, _states, user, localization), new(_skills, catalog, _states, user, localization),
            new(_effects, catalog, _states, user, localization), new(_enemies, catalog, _states, user, localization),
            new(_relations, catalog, _states, localization), catalog);
    }

    private static ResonanceDefinition Resonance() => new()
    {
        Id = "test.skill", Name = "原技能", Variants = new()
        { new() { Slot = ResonanceSlotKind.Tactical, MaximumCharges = 3, CooldownSeconds = 2, CarrierKind = ResonanceCarrierKind.Self, Condition = new() } }
    };

    [Fact]
    public async Task SameKeyInDifferentCategories_UpdatesAndDeletesOnlyTheOwnedTable()
    {
        var services = Service();
        var enemy = Enemy();
        enemy.Id = "test.skill";
        (await services.Enemies.SaveDataEnemyAsync(new() { Id = enemy.Id, Definition = enemy }, CancellationToken.None)).Code.ShouldBe(HttpStatusCode.OK);
        var skill = Resonance();
        var saved = await services.Skills.SaveDataResonanceAsync(new() { Id = skill.Id, Definition = skill }, CancellationToken.None);
        saved.Code.ShouldBe(HttpStatusCode.OK);
        _records.Count.ShouldBe(2);
        (await services.Skills.DeleteDataResonanceAsync(new() { Id = skill.Id, ExpectedVersion = saved.Data!.Version }, CancellationToken.None)).Code.ShouldBe(HttpStatusCode.OK);
        _records.Single().ShouldBeOfType<EraEnemyDefinition>();
        (await services.Enemies.GetDataEnemyAsync(new() { Id = enemy.Id }, CancellationToken.None)).Data!.Definition.Name.ShouldBe(enemy.Name);
        await _enemies.DidNotReceive().DeleteAsync(Arg.Any<EraEnemyDefinition>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CatalogQuery_RetriesWhenTheRevisionChangesBetweenCategoryReads()
    {
        var services = Service();
        var reads = 0;
        _states.GetAsync(false, Arg.Any<CancellationToken>()).Returns(_ => new EraDataEditState
        {
            Id = _workspace.Id, Revision = ++reads == 1 ? 1 : 2, SettingsVersion = 1
        });

        var view = await services.Catalog.ReadAsync(CancellationToken.None);

        view!.Workspace.Revision.ShouldBe(2);
        reads.ShouldBe(4);
        await _skills.Received(2).GetDefinitionsAsync(Arg.Any<CancellationToken>());
        await _enemies.Received(2).GetDefinitionsAsync(Arg.Any<CancellationToken>());
        await _skills.DidNotReceive().AddAsync(Arg.Any<EraSkillDefinition>(), Arg.Any<CancellationToken>());
        _states.DidNotReceive().Touch(Arg.Any<EraDataEditState>());
    }

    [Fact]
    public async Task Save_DoesNotUseAnOlderTransactionSnapshotForReferenceValidation()
    {
        var services = Service();
        _workspace.Revision = 3;
        _states.GetAsync(false, Arg.Any<CancellationToken>()).Returns(_ => new EraDataEditState
        {
            Id = _workspace.Id, Revision = 2, SettingsVersion = 1
        });
        var definition = Resonance();

        var result = await services.Skills.SaveDataResonanceAsync(new() { Id = definition.Id, Definition = definition }, CancellationToken.None);

        result.Code.ShouldBe(HttpStatusCode.Conflict);
        _records.ShouldBeEmpty();
        _workspace.Revision.ShouldBe(3);
        _states.DidNotReceive().Touch(Arg.Any<EraDataEditState>());
    }

    [Fact]
    public async Task Relations_CannotRemoveAnIdentityReferencedByAnEnemy()
    {
        var services = Service();
        var enemy = Enemy();
        await services.Enemies.SaveDataEnemyAsync(new() { Id = enemy.Id, Definition = enemy }, CancellationToken.None);
        var revision = _workspace.Revision;

        var result = await services.Relations.SaveDataSettingsAsync(new()
        {
            ExpectedVersion = _workspace.SettingsVersion,
            Settings = new() { CombatRelations = new() }
        }, CancellationToken.None);

        result.Code.ShouldBe(HttpStatusCode.BadRequest);
        _workspace.Revision.ShouldBe(revision);
        (await services.Relations.GetDataSettingsAsync(new(), CancellationToken.None)).Data!.Settings.CombatRelations!.Species.Single().Id.ShouldBe("construct");
    }

    [Fact]
    public async Task Save_IsImmediatelyReadableWithoutPublish()
    {
        var service = Service();
        var skill = Resonance();
        var saved = await service.Skills.SaveDataResonanceAsync(new() { Id = skill.Id, Definition = skill }, CancellationToken.None);
        saved.Code.ShouldBe(HttpStatusCode.OK);
        _records.Single().ShouldBeOfType<EraSkillDefinition>();
        var current = await service.Catalog.GetConfigurationAsync(new(), CancellationToken.None);
        current.Data!.Skills.Single().Name.ShouldBe("原技能");
        skill.Name = "立即更新";
        (await service.Skills.SaveDataResonanceAsync(new() { Id = skill.Id, ExpectedVersion = _records.Single().Version, Definition = skill }, CancellationToken.None)).Code.ShouldBe(HttpStatusCode.OK);
        (await service.Catalog.GetConfigurationAsync(new(), CancellationToken.None)).Data!.Skills.Single().Name.ShouldBe("立即更新");
        JsonSerializer.Serialize(current.Data).ShouldNotContain("\"Version\"");
    }

    [Fact]
    public async Task InvalidSave_DoesNotMutateExistingDefinition()
    {
        var service = Service();
        var skill = Resonance();
        await service.Skills.SaveDataResonanceAsync(new() { Id = skill.Id, Definition = skill }, CancellationToken.None);
        var original = _records.Single().Payload;
        skill.Variants[0].Effects.Add(new() { EffectId = "missing" });
        var failed = await service.Skills.SaveDataResonanceAsync(new() { Id = skill.Id, ExpectedVersion = _records.Single().Version, Definition = skill }, CancellationToken.None);
        failed.Code.ShouldBe(HttpStatusCode.BadRequest);
        _records.Single().Payload.ShouldBe(original);
        _workspace.Revision.ShouldBe(2);
    }

    [Fact]
    public async Task SkillDisabledState_IsStoredInItsColumnAndReturnedWithTheDefinition()
    {
        var service = Service();
        var skill = Resonance();
        skill.IsDisabled = true;
        var saved = await service.Skills.SaveDataResonanceAsync(new() { Id = skill.Id, Definition = skill }, CancellationToken.None);
        saved.Code.ShouldBe(HttpStatusCode.OK);
        var row = (EraSkillDefinition)_records.Single();
        row.IsDisabled.ShouldBeTrue();
        using (var payload = JsonDocument.Parse(row.Payload))
            payload.RootElement.TryGetProperty("IsDisabled", out _).ShouldBeFalse();
        saved.Data!.Definition.IsDisabled.ShouldBeTrue();
        var configuration = await service.Catalog.GetConfigurationAsync(new(), CancellationToken.None);
        configuration.Data!.Skills.Single().IsDisabled.ShouldBeTrue();

        skill.IsDisabled = false;
        skill.Variants[0].Effects.Add(new() { EffectId = "missing" });
        (await service.Skills.SaveDataResonanceAsync(new() { Id = skill.Id, ExpectedVersion = row.Version, Definition = skill }, CancellationToken.None)).Code.ShouldBe(HttpStatusCode.BadRequest);
        row.IsDisabled.ShouldBeTrue();

        skill.Variants[0].Effects.Clear();
        (await service.Skills.SaveDataResonanceAsync(new() { Id = skill.Id, ExpectedVersion = row.Version, Definition = skill }, CancellationToken.None)).Code.ShouldBe(HttpStatusCode.OK);
        row.IsDisabled.ShouldBeFalse();
        (await service.Catalog.GetConfigurationAsync(new(), CancellationToken.None)).Data!.Skills.Single().IsDisabled.ShouldBeFalse();
    }

    [Fact]
    public async Task EffectDisabledState_IsColumnOwnedAndRetainsReferences()
    {
        var services = Service();
        var effect = new BuffDefinitionDto
        {
            Id = "health", IsDisabled = true,
            Effects = new() { new() { Kind = BuffEffectKind.Heal, Amount = 25 } }
        };
        var saved = await services.Effects.SaveDataBuffAsync(new() { Id = effect.Id, Definition = effect }, CancellationToken.None);
        saved.Code.ShouldBe(HttpStatusCode.OK);
        var row = _records.OfType<EraEffectDefinition>().Single();
        row.IsDisabled.ShouldBeTrue();
        using (var payload = JsonDocument.Parse(row.Payload))
            payload.RootElement.TryGetProperty("IsDisabled", out _).ShouldBeFalse();
        saved.Data!.Definition.IsDisabled.ShouldBeTrue();
        (await services.Effects.GetDataBuffAsync(new() { Id = effect.Id }, CancellationToken.None)).Data!.Definition.IsDisabled.ShouldBeTrue();
        (await services.Catalog.GetConfigurationAsync(new(), CancellationToken.None)).Data!.Buffs.Single().IsDisabled.ShouldBeTrue();
        var skill = Resonance();
        skill.Variants[0].Effects.Add(new() { EffectId = effect.Id });
        (await services.Skills.SaveDataResonanceAsync(new() { Id = skill.Id, Definition = skill }, CancellationToken.None)).Code.ShouldBe(HttpStatusCode.OK);

        effect.IsDisabled = false;
        effect.Effects.Clear();
        (await services.Effects.SaveDataBuffAsync(new() { Id = effect.Id, ExpectedVersion = row.Version, Definition = effect }, CancellationToken.None)).Code.ShouldBe(HttpStatusCode.BadRequest);
        row.IsDisabled.ShouldBeTrue();
        effect.Effects.Add(new() { Kind = BuffEffectKind.Heal, Amount = 25 });
        (await services.Effects.SaveDataBuffAsync(new() { Id = effect.Id, ExpectedVersion = row.Version, Definition = effect }, CancellationToken.None)).Code.ShouldBe(HttpStatusCode.OK);
        row.IsDisabled.ShouldBeFalse();
        (await services.Catalog.GetConfigurationAsync(new(), CancellationToken.None)).Data!.Skills.Single().Variants[0].Effects.Single().EffectId.ShouldBe(effect.Id);
    }

    [Fact]
    public async Task Delete_ReferencedEffectIsRejected_ThenUnreferencedDefinitionIsRemoved()
    {
        var service = Service();
        var effect = new BuffDefinitionDto { Id = "health", Effects = new() { new() { Kind = BuffEffectKind.Heal, Amount = 25 } } };
        (await service.Effects.SaveDataBuffAsync(new() { Id = effect.Id, Definition = effect }, CancellationToken.None)).Code.ShouldBe(HttpStatusCode.OK);
        _records.Single().ShouldBeOfType<EraEffectDefinition>();
        var skill = Resonance(); skill.Variants[0].Effects.Add(new() { EffectId = effect.Id });
        await service.Skills.SaveDataResonanceAsync(new() { Id = skill.Id, Definition = skill }, CancellationToken.None);
        (await service.Effects.DeleteDataBuffAsync(new() { Id = effect.Id, ExpectedVersion = _records.Single(r => r.StableId == effect.Id).Version }, CancellationToken.None)).Code.ShouldBe(HttpStatusCode.BadRequest);
        _records.Count.ShouldBe(2);
        await service.Skills.DeleteDataResonanceAsync(new() { Id = skill.Id, ExpectedVersion = _records.Single(r => r.StableId == skill.Id).Version }, CancellationToken.None);
        (await service.Effects.DeleteDataBuffAsync(new() { Id = effect.Id, ExpectedVersion = _records.Single(r => r.StableId == effect.Id).Version }, CancellationToken.None)).Code.ShouldBe(HttpStatusCode.OK);
        _records.ShouldBeEmpty();
    }

    [Fact]
    public async Task StaleEdit_DoesNotOverwriteCurrentDefinition()
    {
        var service = Service(); var skill = Resonance();
        await service.Skills.SaveDataResonanceAsync(new() { Id = skill.Id, Definition = skill }, CancellationToken.None);
        skill.Name = "过期修改";
        var failed = await service.Skills.SaveDataResonanceAsync(new() { Id = skill.Id, Definition = skill }, CancellationToken.None);
        failed.Code.ShouldBe(HttpStatusCode.Conflict);
        JsonSerializer.Deserialize<ResonanceDefinition>(_records.Single().Payload)!.Name.ShouldBe("原技能");
    }
    [Fact]
    public async Task RecreatedKey_RejectsThePreviousObjectsEditingToken()
    {
        var service = Service(); var skill = Resonance();
        var first = await service.Skills.SaveDataResonanceAsync(new() { Id = skill.Id, Definition = skill }, CancellationToken.None);
        await service.Skills.DeleteDataResonanceAsync(new() { Id = skill.Id, ExpectedVersion = first.Data!.Version }, CancellationToken.None);
        var recreated = await service.Skills.SaveDataResonanceAsync(new() { Id = skill.Id, Definition = skill }, CancellationToken.None);
        recreated.Data!.Version.ShouldBeGreaterThan(first.Data.Version);
        var stale = await service.Skills.SaveDataResonanceAsync(new() { Id = skill.Id, ExpectedVersion = first.Data.Version, Definition = skill }, CancellationToken.None);
        stale.Code.ShouldBe(HttpStatusCode.Conflict);
    }

}
