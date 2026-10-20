using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Settings.System;

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

    [Fact]
    public void Should_register_equipment_entities_and_single_armor_primary_key()
    {
        using var context = Context();
        context.Model.FindEntityType(typeof(EraEquipmentConfiguration)).ShouldNotBeNull();
        context.Model.FindEntityType(typeof(EraEquipmentInstance)).ShouldNotBeNull();
        var armor = context.Model.FindEntityType(typeof(EraArmorInstanceData))!;

        armor.FindPrimaryKey()!.Properties.Select(property => property.Name).ShouldBe(new[] { nameof(EraArmorInstanceData.EquipmentId) });
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
    }

    [Fact]
    public void Should_protect_equipped_slot_uniqueness_and_independent_concurrency_versions()
    {
        using var context = Context();
        var equipment = context.Model.FindEntityType(typeof(EraEquipmentInstance))!;

        equipment.GetIndexes().Any(index => index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(new[]
        {
            nameof(EraEquipmentInstance.CharacterId), nameof(EraEquipmentInstance.EquippedSlot)
        })).ShouldBeTrue();
        equipment.FindProperty(nameof(EraEquipmentInstance.EquippedSlot))!.IsNullable.ShouldBeTrue();
        equipment.FindProperty(nameof(EraEquipmentInstance.Version))!.IsConcurrencyToken.ShouldBeTrue();
        context.Model.FindEntityType(typeof(EraCharacter))!
            .FindProperty(nameof(EraCharacter.EquipmentVersion))!.IsConcurrencyToken.ShouldBeTrue();
    }
}
