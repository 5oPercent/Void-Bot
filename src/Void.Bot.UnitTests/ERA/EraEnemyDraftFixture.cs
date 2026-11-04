using System.Net;
using System.Text.Json;
using Shouldly;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.UnitTest.ERA;

public sealed partial class EraResonanceDraftFixture
{
    private EnemyDefinition Enemy()
    {
        _workspace.SettingsPayload = JsonSerializer.Serialize(new EraResonanceDraftSettings
        {
            CombatRelations = new()
            {
                Species = new() { new() { Id = "construct", Name = "构造体" } },
                Factions = new() { new() { Id = "hostile", Name = "敌对阵营" } }
            }
        });
        return new() { Id = "guard", Name = "守卫", PresentationKey = "guard.body", SpeciesId = "construct", FactionId = "hostile", Ai = new() { Behavior = EnemyBehavior.Roam } };
    }

    [Fact]
    public async Task Enemy_SaveSearchConflictAndDeletionUseSharedDraftVersions()
    {
        var service = Service();
        var definition = Enemy();
        var created = await service.SaveDraftEnemyAsync(new() { Id = "guard", Definition = definition }, CancellationToken.None);
        created.Code.ShouldBe(HttpStatusCode.OK);
        created.Data!.Version.ShouldBe(1);
        _published.Version.ShouldBe(1);
        var duplicate = await service.SaveDraftEnemyAsync(new() { Id = "guard", Definition = definition }, CancellationToken.None);
        duplicate.ErrorCode.ShouldBe("era.draft_version_conflict");
        var list = await service.ListDraftEnemyAsync(new() { Search = "守卫" }, CancellationToken.None);
        list.Data!.Items.Single().Definition.Ai.Behavior.ShouldBe(EnemyBehavior.Roam);
        await service.DeleteDraftEnemyAsync(new() { Id = "guard", ExpectedVersion = 1 }, CancellationToken.None);
        (await service.ListDraftEnemyAsync(new(), CancellationToken.None)).Data!.Total.ShouldBe(0);
        var restored = await service.RestoreDraftEnemyAsync(new() { Id = "guard", ExpectedVersion = 2 }, CancellationToken.None);
        restored.Data!.IsDeleted.ShouldBeFalse();
        restored.Data.Version.ShouldBe(3);
    }

    [Fact]
    public async Task Enemy_InvalidReferencesDoNotCreateOrTouchDraft()
    {
        var service = Service();
        var definition = Enemy();
        definition.SpeciesId = "missing";
        var result = await service.SaveDraftEnemyAsync(new() { Id = "guard", Definition = definition }, CancellationToken.None);
        result.ErrorCode.ShouldBe("era.draft_invalid");
        _records.ShouldBeEmpty();
        _workspace.Revision.ShouldBe(1);
    }

    [Fact]
    public void Enemy_RejectsInvalidNumbersDuplicateSkillsAndUnknownFields()
    {
        var definition = Enemy();
        definition.RecoveryDelaySeconds = -1;
        Should.Throw<ArgumentException>(() => EnemyDefinitionValidator.Validate(definition));
        definition.RecoveryDelaySeconds = 5;
        definition.HealthRecoveryPerSecond = double.PositiveInfinity;
        Should.Throw<ArgumentException>(() => EnemyDefinitionValidator.Validate(definition));
        definition.HealthRecoveryPerSecond = 12;
        definition.ProtectionRecoveryPerSecond = -1;
        Should.Throw<ArgumentException>(() => EnemyDefinitionValidator.Validate(definition));
        definition.ProtectionRecoveryPerSecond = 0;
        definition.Ai.PerceptionIntervalSeconds = 0;
        Should.Throw<ArgumentException>(() => EnemyDefinitionValidator.Validate(definition));
        definition.Ai.PerceptionIntervalSeconds = .3;
        definition.MaximumHealth = double.NaN;
        Should.Throw<ArgumentException>(() => EnemyDefinitionValidator.Validate(definition));
        definition.MaximumHealth = 100;
        definition.Skills = new() { new() { SkillId = "a" }, new() { SkillId = "a" } };
        Should.Throw<ArgumentException>(() => EnemyDefinitionValidator.Validate(definition));
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<EnemyDefinition>("{\"RegionId\":\"zone\"}"));
    }

    [Fact]
    public async Task Enemy_PublicationIncludesTemplatesButRuntimeResponseKeepsExistingContract()
    {
        var service = Service();
        await service.SaveDraftEnemyAsync(new() { Id = "guard", Definition = Enemy() }, CancellationToken.None);
        var request = new Void.Bot.Message.Commands.ERA.PublishEraResonanceDraftCommand { RequestId = Guid.NewGuid(), ExpectedDraftRevision = _workspace.Revision, ExpectedPublishedVersion = 1 };
        var published = await service.PublishDraftAsync(request, CancellationToken.None);
        published.Code.ShouldBe(HttpStatusCode.OK);
        published.Data!.Enemies!.Single().Id.ShouldBe("guard");
        var retry = await service.PublishDraftAsync(request, CancellationToken.None);
        retry.Data!.Enemies!.Single().PresentationKey.ShouldBe("guard.body");
        var runtime = await service.GetConfigurationAsync(new(), CancellationToken.None);
        runtime.Code.ShouldBe(HttpStatusCode.OK);
        JsonSerializer.Serialize(runtime.Data).ShouldNotContain("Enemies");
        (await service.ExportDraftAsync(new(), CancellationToken.None)).Data!.Enemies!.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Enemy_DeletedSkillBlocksPublicationAndAppearsInReferences()
    {
        var service = Service();
        var definition = Enemy();
        var skill = AddResonance();
        definition.Skills.Add(new() { SkillId = "test.skill", Slot = ResonanceSlotKind.Tactical });
        await service.SaveDraftEnemyAsync(new() { Id = "guard", Definition = definition }, CancellationToken.None);
        var references = await service.GetDraftReferencesAsync(new() { Id = "test.skill", Kind = EraResonanceDraftKind.Resonance }, CancellationToken.None);
        references.Data!.Items.ShouldContain(item => item.Path == "Enemies/guard/Skills");
        await service.DeleteDraftResonanceAsync(new() { Id = "test.skill", ExpectedVersion = skill.Version }, CancellationToken.None);
        var preview = await service.PreviewDraftAsync(new(), CancellationToken.None);
        preview.Data!.Errors.ShouldContain(item => item.Path == "Enemies/guard");
        var published = await service.PublishDraftAsync(new() { RequestId = Guid.NewGuid(), ExpectedDraftRevision = _workspace.Revision, ExpectedPublishedVersion = 1 }, CancellationToken.None);
        published.ErrorCode.ShouldBe("era.draft_invalid");
        _published.Version.ShouldBe(1);
        _history.ShouldBeEmpty();
    }

    [Fact]
    public async Task Enemy_OmittedImportPreservesTemplatesAndExplicitEmptyRemovesThem()
    {
        var service = Service();
        await service.SaveDraftEnemyAsync(new() { Id = "guard", Definition = Enemy() }, CancellationToken.None);
        var imported = await service.ImportDraftAsync(new() { ExpectedDraftRevision = _workspace.Revision, Mode = EraResonanceDraftImportMode.Replace, Configuration = new() }, CancellationToken.None);
        imported.Code.ShouldBe(HttpStatusCode.OK);
        _records.Single().IsDeleted.ShouldBeFalse();
        var removed = await service.ImportDraftAsync(new() { ExpectedDraftRevision = _workspace.Revision, Mode = EraResonanceDraftImportMode.Replace, Configuration = new() { Enemies = new() } }, CancellationToken.None);
        removed.Code.ShouldBe(HttpStatusCode.OK);
        _records.Single().IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void Enemy_InitialBuffRejectsInstantAndSixDimensionalModifiers()
    {
        var enemy = Enemy();
        enemy.InitialBuffs.Add("initial");
        var catalog = new ResonanceConfiguration { CombatRelations = JsonSerializer.Deserialize<EraResonanceDraftSettings>(_workspace.SettingsPayload)!.CombatRelations, Buffs = new() { new() { Id = "initial", Kind = BuffKind.Instant } } };
        Should.Throw<ArgumentException>(() => EnemyDefinitionValidator.References(enemy, catalog));
        catalog.Buffs[0].Kind = BuffKind.Periodic;
        catalog.Buffs[0].Effects.Add(new() { Kind = BuffEffectKind.Modifier, Stat = 0 });
        Should.Throw<ArgumentException>(() => EnemyDefinitionValidator.References(enemy, catalog));
        catalog.Buffs[0].Effects[0].Stat = 4;
        Should.NotThrow(() => EnemyDefinitionValidator.References(enemy, catalog));
    }
}
