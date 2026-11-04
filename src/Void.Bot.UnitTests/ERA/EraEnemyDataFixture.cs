using System.Net;
using System.Text.Json;
using Shouldly;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.UnitTest.ERA;

public sealed partial class EraConfigurationServicesFixture
{
    private EnemyDefinition Enemy()
    {
        _workspace.SettingsPayload = JsonSerializer.Serialize(new EraDataSettings
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
    public async Task Enemy_SaveSearchConflictAndDeletionUseCurrentDefinitions()
    {
        var service = Service();
        var definition = Enemy();
        var created = await service.Enemies.SaveDataEnemyAsync(new() { Id = "guard", Definition = definition }, CancellationToken.None);
        created.Code.ShouldBe(HttpStatusCode.OK);
        created.Data!.Version.ShouldBe(2);
        var duplicate = await service.Enemies.SaveDataEnemyAsync(new() { Id = "guard", Definition = definition }, CancellationToken.None);
        duplicate.ErrorCode.ShouldBe("era.data_version_conflict");
        var list = await service.Enemies.ListDataEnemyAsync(new() { Search = "守卫" }, CancellationToken.None);
        list.Data!.Items.Single().Definition.Ai.Behavior.ShouldBe(EnemyBehavior.Roam);
        await service.Enemies.DeleteDataEnemyAsync(new() { Id = "guard", ExpectedVersion = created.Data.Version }, CancellationToken.None);
        (await service.Enemies.ListDataEnemyAsync(new(), CancellationToken.None)).Data!.Total.ShouldBe(0);
        _records.ShouldBeEmpty();
    }

    [Fact]
    public async Task Enemy_InvalidReferencesDoNotCreateOrTouchData()
    {
        var service = Service();
        var definition = Enemy();
        definition.SpeciesId = "missing";
        var result = await service.Enemies.SaveDataEnemyAsync(new() { Id = "guard", Definition = definition }, CancellationToken.None);
        result.ErrorCode.ShouldBe("era.data_invalid");
        _records.ShouldBeEmpty();
        _workspace.Revision.ShouldBe(1);
    }

    [Fact]
    public void Enemy_RejectsInvalidNumbersDuplicateSkillsAndUnknownFields()
    {
        var definition = Enemy();
        foreach (var key in new[] { " ", " guard.body", "guard.body ", new string('x', 129) })
        {
            definition.PresentationKey = key;
            Should.Throw<ArgumentException>(() => EnemyDefinitionValidator.Validate(definition));
        }
        definition.PresentationKey = "";
        definition.Tier = (EnemyTier)5;
        Should.Throw<ArgumentException>(() => EnemyDefinitionValidator.Validate(definition));
        definition.Tier = EnemyTier.Minor;
        EnemyDefinitionValidator.Validate(definition);
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
        definition.BaseHealth = double.NaN;
        Should.Throw<ArgumentException>(() => EnemyDefinitionValidator.Validate(definition));
        definition.BaseHealth = 100;
        definition.Skills = new() { new() { SkillId = "a" }, new() { SkillId = "a" } };
        Should.Throw<ArgumentException>(() => EnemyDefinitionValidator.Validate(definition));
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<EnemyDefinition>("{\"RegionId\":\"zone\"}"));
    }

    [Theory]
    [InlineData("", EnemyTier.Minor)]
    [InlineData("guard.body", EnemyTier.Elite)]
    [InlineData("guard.body", EnemyTier.Major)]
    [InlineData("guard.body", EnemyTier.Ultra)]
    [InlineData("", EnemyTier.Boss)]
    public async Task Enemy_SaveImmediatelyPreservesAllParameters(string presentationKey, EnemyTier tier)
    {
        var service = Service();
        var definition = Enemy();
        definition.PresentationKey = presentationKey;
        definition.Tier = tier;
        definition.BaseHealth = 100.1;
        definition.BaseProtection = 50.1;
        definition.NormalAttack.BaseDamage = 10.1;
        definition.TierMultipliers.Elite = new() { Health = 2.5, Protection = 1.25, Damage = 0.5 };
        await service.Enemies.SaveDataEnemyAsync(new() { Id = "guard", Definition = definition }, CancellationToken.None);
        var saved = (await service.Enemies.GetDataEnemyAsync(new() { Id = "guard" }, CancellationToken.None)).Data!.Definition;
        saved.PresentationKey.ShouldBe(presentationKey);
        saved.Tier.ShouldBe(tier);
        _records.Single().ShouldBeOfType<Void.Bot.Core.Domain.ERA.EraEnemyDefinition>();
        saved.BaseHealth.ShouldBe(100.1);
        saved.BaseProtection.ShouldBe(50.1);
        saved.NormalAttack.BaseDamage.ShouldBe(10.1);
        saved.TierMultipliers.Elite.Health.ShouldBe(2.5);
        saved.TierMultipliers.Elite.Protection.ShouldBe(1.25);
        saved.TierMultipliers.Elite.Damage.ShouldBe(0.5);
        var runtime = await service.Catalog.GetConfigurationAsync(new(), CancellationToken.None);
        runtime.Code.ShouldBe(HttpStatusCode.OK);
        JsonSerializer.Serialize(runtime.Data).ShouldNotContain("Enemies");
        (await service.Catalog.ExportDataAsync(new(), CancellationToken.None)).Data!.Enemies!.Count.ShouldBe(1);
    }

    [Fact]
    public void Enemy_BaseStatsAndTierMultipliersHaveIndependentDefaultsAndStrictValidation()
    {
        var definition = Enemy();
        definition.BaseHealth.ShouldBe(100);
        definition.BaseProtection.ShouldBe(50);
        definition.NormalAttack.BaseDamage.ShouldBe(10);
        var tiers = definition.TierMultipliers;
        new[] { tiers.Minor.Health, tiers.Elite.Health, tiers.Major.Health, tiers.Ultra.Health, tiers.Boss.Health }.ShouldBe(new double[] { 1, 5, 10, 20, 100 });
        new[] { tiers.Minor.Protection, tiers.Elite.Protection, tiers.Major.Protection, tiers.Ultra.Protection, tiers.Boss.Protection }.ShouldBe(new double[] { 0, 3, 6, 10, 0 });
        new[] { tiers.Minor.Damage, tiers.Elite.Damage, tiers.Major.Damage, tiers.Ultra.Damage, tiers.Boss.Damage }.ShouldBe(new double[] { 1, 2, 3, 4, 2 });
        foreach (var row in new[] { tiers.Minor, tiers.Elite, tiers.Major, tiers.Ultra, tiers.Boss })
        {
            var original = (row.Health, row.Protection, row.Damage);
            foreach (var invalid in new[] { double.NaN, double.PositiveInfinity, -1, 10001 })
            {
                row.Health = invalid;
                Should.Throw<ArgumentException>(() => EnemyDefinitionValidator.Validate(definition));
                row.Health = original.Health;
                row.Protection = invalid;
                Should.Throw<ArgumentException>(() => EnemyDefinitionValidator.Validate(definition));
                row.Protection = original.Protection;
                row.Damage = invalid;
                Should.Throw<ArgumentException>(() => EnemyDefinitionValidator.Validate(definition));
                row.Damage = original.Damage;
            }
            row.Health = 0;
            Should.Throw<ArgumentException>(() => EnemyDefinitionValidator.Validate(definition));
            row.Health = original.Health;
            EnemyDefinitionValidator.Validate(definition);
        }
        tiers.Elite.Health = 2.5;
        tiers.Minor.Health.ShouldBe(1);
        Enemy().TierMultipliers.Elite.Health.ShouldBe(5);
        tiers.Boss = null!;
        Should.Throw<ArgumentException>(() => EnemyDefinitionValidator.Validate(definition));
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<EnemyDefinition>("{\"MaximumHealth\":100}"));
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<EnemyTierMultipliersDefinition>("{}"));
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<EnemyStatMultipliersDefinition>("{\"Health\":1,\"Protection\":0,\"Damage\":1,\"Speed\":2}"));
    }

    [Fact]
    public async Task Enemy_ReferencedSkillCannotBeDeleted()
    {
        var service = Service();
        var definition = Enemy();
        var skill = await service.Skills.SaveDataResonanceAsync(new() { Id = "test.skill", Definition = Resonance() }, CancellationToken.None);
        definition.Skills.Add(new() { SkillId = "test.skill", Slot = ResonanceSlotKind.Tactical });
        await service.Enemies.SaveDataEnemyAsync(new() { Id = "guard", Definition = definition }, CancellationToken.None);
        var references = await service.Catalog.GetDataReferencesAsync(new() { Id = "test.skill", Kind = EraDataKind.Resonance }, CancellationToken.None);
        references.Data!.Items.ShouldContain(item => item.Path == "Enemies/guard/Skills");
        var failed = await service.Skills.DeleteDataResonanceAsync(new() { Id = "test.skill", ExpectedVersion = skill.Data!.Version }, CancellationToken.None);
        failed.Code.ShouldBe(HttpStatusCode.BadRequest);
        _records.Count.ShouldBe(2);
    }

    [Fact]
    public void Enemy_InitialBuffRejectsInstantAndSixDimensionalModifiers()
    {
        var enemy = Enemy();
        enemy.InitialBuffs.Add("initial");
        var catalog = new ResonanceConfiguration { CombatRelations = JsonSerializer.Deserialize<EraDataSettings>(_workspace.SettingsPayload)!.CombatRelations, Buffs = new() { new() { Id = "initial", Kind = BuffKind.Instant } } };
        Should.Throw<ArgumentException>(() => EnemyDefinitionValidator.References(enemy, catalog));
        catalog.Buffs[0].Kind = BuffKind.Periodic;
        catalog.Buffs[0].Effects.Add(new() { Kind = BuffEffectKind.Modifier, Stat = 0 });
        Should.Throw<ArgumentException>(() => EnemyDefinitionValidator.References(enemy, catalog));
        catalog.Buffs[0].Effects[0].Stat = 4;
        Should.NotThrow(() => EnemyDefinitionValidator.References(enemy, catalog));
    }
}
