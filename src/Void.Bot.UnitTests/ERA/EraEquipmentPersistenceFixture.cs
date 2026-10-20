using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Models;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.ERA;
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
    public void Should_map_private_drop_ownership_active_uniqueness_and_world_reward_without_cascade_deletes()
    {
        using var context = Context();
        var drop = context.Model.FindEntityType(typeof(EraEquipmentDrop))!;
        drop.GetForeignKeys().Count(key => key.PrincipalEntityType.ClrType == typeof(EraPlayer)).ShouldBe(2);
        drop.GetForeignKeys().All(key => key.DeleteBehavior == DeleteBehavior.Restrict).ShouldBeTrue();
        drop.GetIndexes().Any(index => index.IsUnique && index.Properties.Select(property => property.Name)
            .SequenceEqual(new[] { nameof(EraEquipmentDrop.ActiveEquipmentId) })).ShouldBeTrue();
        drop.FindProperty(nameof(EraEquipmentDrop.ActiveEquipmentId))!.IsNullable.ShouldBeTrue();
        var share = context.Model.FindEntityType(typeof(EraEquipmentShare))!;
        share.FindPrimaryKey()!.Properties.Single().Name.ShouldBe(nameof(EraEquipmentShare.EquipmentId));
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
    public async Task Should_reject_position_updates_without_outer_command_transaction()
    {
        using var context = Context();
        var data = new EraEquipmentDataProvider(context);

        await Should.ThrowAsync<InvalidOperationException>(() => data.ApplyPositionChangesAsync(
            Guid.NewGuid(), Guid.NewGuid(), Array.Empty<EraEquipmentPositionChange>(), CancellationToken.None));
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
