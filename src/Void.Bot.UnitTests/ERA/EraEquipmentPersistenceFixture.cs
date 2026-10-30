using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;
using System.Text.Json;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Settings.System;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.UnitTest.ERA;

/// <summary>只构造 EF 模型，不连接数据库；实际迁移和 MySQL 查询仍需单独集成验证。</summary>
public class EraEquipmentPersistenceFixture
{
    private static VoidBotDbContext Context()
    {
        var configuration = Substitute.For<IConfiguration>();
        configuration["ConnectionStrings:Mysql"].Returns("Server=localhost;Database=equipment_model_only;User=model_only;Password=unused");
        return new VoidBotDbContext(new ConnectionStringSetting(configuration), Substitute.For<ICurrentUser>());
    }

    [Theory]
    [InlineData(typeof(EraArmorInstance), "era_armor_instance")]
    [InlineData(typeof(EraArmorInstanceModCandidate), "era_armor_instance_mod_candidate")]
    [InlineData(typeof(EraArmorInstanceModSelection), "era_armor_instance_mod_selection")]
    [InlineData(typeof(EraWeaponInstanceModCandidate), "era_weapon_instance_mod_candidate")]
    [InlineData(typeof(EraWeaponInstanceModSelection), "era_weapon_instance_mod_selection")]
    public void Should_map_instance_extensions_and_children_to_explicit_instance_tables(Type type, string table)
    {
        using var context = Context();
        var entity = context.Model.FindEntityType(type)!;
        entity.GetTableName().ShouldBe(table);
        entity.FindProperty("EquipmentInstanceId")!.GetColumnName().ShouldBe("equipment_instance_id");
    }

    [Fact]
    public void Should_distinguish_instance_identity_template_reference_and_generation_directory_in_storage_and_payload()
    {
        using var context = Context();
        var entity = context.Model.FindEntityType(typeof(EraEquipmentInstance))!;
        entity.GetTableName().ShouldBe("era_equipment_instance");
        entity.FindProperty(nameof(EraEquipmentInstance.TemplateId))!.GetColumnName().ShouldBe("template_id");
        entity.FindProperty(nameof(EraEquipmentInstance.GenerationManifestVersion))!.GetColumnName().ShouldBe("generation_manifest_version");
        var asset = new EraEquipmentInstanceDto { Id = Guid.NewGuid(), TemplateId = Guid.NewGuid(), GenerationManifestVersion = 7,
            ArmorTemplate = new EraArmorTemplateDto() };
        var payload = JsonSerializer.SerializeToElement(asset);
        payload.GetProperty("Id").GetGuid().ShouldBe(asset.Id);
        payload.GetProperty("TemplateId").GetGuid().ShouldBe(asset.TemplateId);
        payload.GetProperty("GenerationManifestVersion").GetInt64().ShouldBe(7);
        payload.GetProperty("ArmorTemplate").ValueKind.ShouldBe(JsonValueKind.Object);
        payload.TryGetProperty("DefinitionId", out _).ShouldBeFalse();
        payload.TryGetProperty("Definition", out _).ShouldBeFalse();
    }

    [Fact]
    public void Should_constrain_weapon_selections_to_candidates_of_the_same_instance_and_slot()
    {
        using var context = Context();
        var types = new[] { typeof(EraWeaponTemplateStat), typeof(EraWeaponFramework), typeof(EraWeaponFrameworkStat), typeof(EraWeaponFrameworkCapability), typeof(EraWeaponSlotType), typeof(EraWeaponSlotConfiguration), typeof(EraWeaponTemplate), typeof(EraWeaponConfigurationSlot), typeof(EraWeaponModDefinition),
            typeof(EraWeaponModStatModifier), typeof(EraWeaponConfigurationModPool), typeof(EraWeaponInstanceModCandidate), typeof(EraWeaponInstanceModSelection) };
        foreach (var type in types)
        {
            var entity = context.Model.FindEntityType(type);
            entity.ShouldNotBeNull();
            entity.GetForeignKeys().All(key => key.DeleteBehavior == DeleteBehavior.Restrict).ShouldBeTrue();
        }
        foreach (var type in new[] { typeof(EraWeaponTemplate), typeof(EraWeaponModDefinition) })
            context.Model.FindEntityType(type)!.FindProperty("Id")!.ClrType.ShouldBe(typeof(Guid));
        context.Model.FindEntityType(typeof(EraEquipmentInstance))!.FindProperty(nameof(EraEquipmentInstance.TemplateId))!
            .ClrType.ShouldBe(typeof(Guid));
        var selection = context.Model.FindEntityType(typeof(EraWeaponInstanceModSelection))!;
        var reference = selection.GetForeignKeys().Single(key => key.PrincipalEntityType.ClrType == typeof(EraWeaponInstanceModCandidate));
        reference.Properties.Select(property => property.Name).ShouldBe(new[] { "EquipmentInstanceId", "SlotId", "SelectedCandidateId" });
        reference.PrincipalKey.Properties.Select(property => property.Name).ShouldBe(new[] { "EquipmentInstanceId", "SlotId", "CandidateId" });
        var candidate = context.Model.FindEntityType(typeof(EraWeaponInstanceModCandidate))!;
        candidate.GetForeignKeys().Any(key => key.PrincipalEntityType.ClrType == typeof(EraWeaponConfigurationModPool)).ShouldBeFalse();
        candidate.FindProperty("Damage").ShouldBeNull();
        selection.FindProperty("Damage").ShouldBeNull();
        context.Model.FindEntityType(typeof(EraEquipmentInstance))!.FindProperty("Damage").ShouldBeNull();
        context.Model.FindEntityType(typeof(EraEquipmentInstance))!.FindProperty("InventorySlot").ShouldBeNull();
        var inventoryIndex = context.Model.FindEntityType(typeof(EraEquipmentInstance))!.GetIndexes()
            .Single(index => index.Properties.Select(property => property.Name)
                .SequenceEqual(new[] { nameof(EraEquipmentInstance.CharacterId), nameof(EraEquipmentInstance.Location) }));
        inventoryIndex.GetDatabaseName().ShouldBe("ix_era_equipment_character_inventory");
        inventoryIndex.IsUnique.ShouldBeFalse();
    }

    [Fact]
    public void Should_map_private_drop_ownership_active_uniqueness_and_world_reward_without_cascade_deletes()
    {
        using var context = Context();
        var drop = context.Model.FindEntityType(typeof(EraEquipmentDrop))!;
        drop.GetForeignKeys().Count(key => key.PrincipalEntityType.ClrType == typeof(EraPlayer)).ShouldBe(2);
        drop.GetForeignKeys().All(key => key.DeleteBehavior == DeleteBehavior.Restrict).ShouldBeTrue();
        drop.GetIndexes().Any(index => index.IsUnique && index.Properties.Select(property => property.Name)
            .SequenceEqual(new[] { nameof(EraEquipmentDrop.ActiveEquipmentInstanceId) })).ShouldBeTrue();
        drop.FindProperty(nameof(EraEquipmentDrop.ActiveEquipmentInstanceId))!.IsNullable.ShouldBeTrue();
        var share = context.Model.FindEntityType(typeof(EraEquipmentShare))!;
        share.FindPrimaryKey()!.Properties.Single().Name.ShouldBe(nameof(EraEquipmentShare.EquipmentInstanceId));
        share.GetForeignKeys().Single().DeleteBehavior.ShouldBe(DeleteBehavior.Restrict);
        var reward = context.Model.FindEntityType(typeof(EraRewardEvent))!;
        reward.GetForeignKeys().Single(key => key.PrincipalEntityType.ClrType == typeof(EraWorld)).DeleteBehavior.ShouldBe(DeleteBehavior.Restrict);
        reward.FindProperty(nameof(EraRewardEvent.WorldInstanceId))!.IsNullable.ShouldBeTrue();
    }

    [Fact]
    public void Should_register_equipment_entities_and_single_armor_primary_key()
    {
        using var context = Context();
        context.Model.FindEntityType(typeof(EraEquipmentConfiguration)).ShouldNotBeNull();
        context.Model.FindEntityType(typeof(EraEquipmentInstance)).ShouldNotBeNull();
        var armor = context.Model.FindEntityType(typeof(EraArmorInstance))!;

        armor.FindPrimaryKey()!.Properties.Select(property => property.Name).ShouldBe(new[] { nameof(EraArmorInstance.EquipmentInstanceId) });
        armor.GetForeignKeys().Single().PrincipalEntityType.ClrType.ShouldBe(typeof(EraEquipmentInstance));
        armor.GetForeignKeys().Single().DeleteBehavior.ShouldBe(DeleteBehavior.Restrict);
    }

    [Fact]
    public void Should_enforce_character_owner_pair_and_prevent_cascade_deletion()
    {
        using var context = Context();
        var equipment = context.Model.FindEntityType(typeof(EraEquipmentInstance))!;
        var character = equipment.GetForeignKeys().Single(key => key.PrincipalEntityType.ClrType == typeof(EraCharacter));

        character.Properties.Select(property => property.Name).ShouldBe(new[]
        {
            nameof(EraEquipmentInstance.CharacterId), nameof(EraEquipmentInstance.OwnerPlayerId)
        });
        character.PrincipalKey.Properties.Select(property => property.Name).ShouldBe(new[]
        {
            nameof(EraCharacter.Id), nameof(EraCharacter.PlayerId)
        });
        equipment.GetForeignKeys().All(key => key.DeleteBehavior == DeleteBehavior.Restrict).ShouldBeTrue();
        var ownerIndex = equipment.GetIndexes().Single(index => index.Properties.Select(property => property.Name)
            .SequenceEqual(new[] { nameof(EraEquipmentInstance.OwnerPlayerId), nameof(EraEquipmentInstance.Location) }));
        ownerIndex.GetDatabaseName().ShouldBe("ix_era_equipment_owner_location");
        ownerIndex.IsUnique.ShouldBeFalse();
    }

    [Fact]
    public void Should_protect_equipped_slot_uniqueness_and_independent_concurrency_versions()
    {
        using var context = Context();
        var equipment = context.Model.FindEntityType(typeof(EraEquipmentInstance))!;

        var equippedIndex = equipment.GetIndexes().Single(index => index.Properties.Select(property => property.Name).SequenceEqual(new[]
        {
            nameof(EraEquipmentInstance.CharacterId), nameof(EraEquipmentInstance.EquippedSlot)
        }));
        equippedIndex.GetDatabaseName().ShouldBe("ux_era_equipment_equipped_slot");
        equippedIndex.IsUnique.ShouldBeTrue();
        equipment.FindProperty(nameof(EraEquipmentInstance.EquippedSlot))!.IsNullable.ShouldBeTrue();
        equipment.FindProperty(nameof(EraEquipmentInstance.Version))!.IsConcurrencyToken.ShouldBeTrue();
        context.Model.FindEntityType(typeof(EraCharacter))!
            .FindProperty(nameof(EraCharacter.EquipmentVersion))!.IsConcurrencyToken.ShouldBeTrue();
    }

    [Fact]
    public void Should_map_framework_children_and_template_reference_without_cascade_deletes()
    {
        using var context = Context();
        var template = context.Model.FindEntityType(typeof(EraWeaponTemplate))!;
        template.GetForeignKeys().Single(key => key.PrincipalEntityType.ClrType == typeof(EraWeaponFramework))
            .Properties.Single().Name.ShouldBe(nameof(EraWeaponTemplate.FrameworkId));
        template.FindProperty("StatMappingId").ShouldBeNull();
        context.Model.FindEntityType(typeof(EraWeaponFramework))!.FindProperty("StatMappingId").ShouldBeNull();
        context.Model.FindEntityType(typeof(EraWeaponFramework))!.GetForeignKeys().ShouldBeEmpty();
        template.FindProperty("Range").ShouldBeNull();
        template.FindProperty("BehaviorPayload").ShouldBeNull();
        foreach (var name in System.Enum.GetNames<Void.Bot.Message.Enum.ERA.EraWeaponStat>())
            template.FindProperty(name).ShouldBeNull();
        var templateStat = context.Model.FindEntityType(typeof(EraWeaponTemplateStat))!;
        templateStat.FindPrimaryKey()!.Properties.Select(property => property.Name).ShouldBe(new[] { "TemplateId", "Stat" });
        templateStat.GetForeignKeys().Single().PrincipalEntityType.ClrType.ShouldBe(typeof(EraWeaponTemplate));
        templateStat.FindProperty(nameof(EraWeaponTemplateStat.Value))!.IsNullable.ShouldBeFalse();
        context.Model.FindEntityType(typeof(EraWeaponFrameworkStat))!.FindPrimaryKey()!.Properties.Select(property => property.Name)
            .ShouldBe(new[] { "FrameworkId", "Stat" });
        context.Model.FindEntityType(typeof(EraWeaponFrameworkCapability))!.FindPrimaryKey()!.Properties.Select(property => property.Name)
            .ShouldBe(new[] { "FrameworkId", "CapabilityKind" });
        templateStat.FindProperty(nameof(EraWeaponTemplateStat.EffectAtZero))!.IsNullable.ShouldBeTrue();
        templateStat.FindProperty(nameof(EraWeaponTemplateStat.EffectAtHundred))!.IsNullable.ShouldBeTrue();
    }

    [Fact]
    public void Should_keep_success_receipt_unique_per_player_request_and_restrict_character_deletion()
    {
        using var context = Context();
        var receipt = context.Model.FindEntityType(typeof(EraEquipmentOperationReceipt))!;

        receipt.GetIndexes().Any(index => index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(new[]
        {
            nameof(EraEquipmentOperationReceipt.PlayerId), nameof(EraEquipmentOperationReceipt.RequestId)
        })).ShouldBeTrue();
        var ownership = receipt.GetForeignKeys().Single();
        ownership.Properties.Select(property => property.Name).ShouldBe(new[]
        {
            nameof(EraEquipmentOperationReceipt.CharacterId), nameof(EraEquipmentOperationReceipt.PlayerId)
        });
        ownership.PrincipalEntityType.ClrType.ShouldBe(typeof(EraCharacter));
        ownership.DeleteBehavior.ShouldBe(DeleteBehavior.Restrict);
    }

    [Fact]
    public void Should_keep_one_active_world_per_player_and_character_and_preserve_member_history()
    {
        using var context = Context();
        var member = context.Model.FindEntityType(typeof(EraWorldMember))!;
        member.FindPrimaryKey()!.Properties.Select(property => property.Name)
            .ShouldBe(new[] { nameof(EraWorldMember.WorldId), nameof(EraWorldMember.PlayerId) });
        foreach (var name in new[] { nameof(EraWorldMember.ActivePlayerId), nameof(EraWorldMember.ActiveCharacterId) })
        {
            member.FindProperty(name)!.IsNullable.ShouldBeTrue();
            member.GetIndexes().Any(index => index.IsUnique && index.Properties.Count == 1 && index.Properties[0].Name == name).ShouldBeTrue();
        }
        member.GetForeignKeys().All(key => key.DeleteBehavior == DeleteBehavior.Restrict).ShouldBeTrue();
        context.Model.FindEntityType(typeof(EraWorldTeamState)).ShouldNotBeNull();
        context.Model.FindEntityType(typeof(EraWorld))!.FindProperty(nameof(EraWorld.Version))!.IsConcurrencyToken.ShouldBeTrue();
    }
}
