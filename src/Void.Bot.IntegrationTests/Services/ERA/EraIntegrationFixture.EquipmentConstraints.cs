using System.Net;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    [Fact]
    [Trait("Category", "ERAEquipmentIntegration")]
    public async Task Database_should_reject_two_equipped_items_in_the_same_character_slot()
    {
        await SeedAsync(0, equipped: true);

        var error = await Should.ThrowAsync<DbUpdateException>(() => ReadDatabaseAsync(async context =>
        {
            AddItems(context, new[] { NewItem(EraEquipmentLocation.Equipped) });
            await context.SaveChangesAsync();
        }));

        error.InnerException.ShouldBeOfType<MySqlException>().Number.ShouldBe(1062);
        await AssertOriginalStateAsync();
    }

    [Fact]
    [Trait("Category", "ERAEquipmentIntegration")]
    public async Task Database_should_reject_a_second_armor_row_for_the_same_equipment()
    {
        await SeedAsync();

        var error = await Should.ThrowAsync<DbUpdateException>(() => ReadDatabaseAsync(async context =>
        {
            context.Set<EraArmorInstanceData>().Add(new EraArmorInstanceData
            {
                EquipmentId = _inventory[0].Id, GenerationRuleVersion = 1, Strength = 1
            });
            await context.SaveChangesAsync();
        }));

        error.InnerException.ShouldBeOfType<MySqlException>().Number.ShouldBe(1062);
        await AssertOriginalStateAsync();
    }

    [Fact]
    [Trait("Category", "ERAEquipmentIntegration")]
    public async Task Database_should_reject_equipment_whose_owner_differs_from_its_character_owner()
    {
        await SeedAsync(0);

        var error = await Should.ThrowAsync<DbUpdateException>(() => ReadDatabaseAsync(async context =>
        {
            // 玩家和角色分别存在，仅组合归属错误，验证复合外键而非缺失主键。
            AddItems(context, new[] { NewItem(playerId: _otherPlayerId, characterId: _characterId) });
            await context.SaveChangesAsync();
        }));

        error.InnerException.ShouldBeOfType<MySqlException>().Number.ShouldBe(1452);
        await AssertOriginalStateAsync();
    }

    [Fact]
    [Trait("Category", "ERAEquipmentIntegration")]
    public async Task Database_should_enforce_request_id_uniqueness_across_operations_for_a_player()
    {
        await SeedAsync();
        var command = Equip(_inventory[0]);
        (await SendEquipAsync(command)).Code.ShouldBe(HttpStatusCode.OK);

        var error = await Should.ThrowAsync<DbUpdateException>(() => ReadDatabaseAsync(async context =>
        {
            context.Set<EraEquipmentOperationReceipt>().Add(new EraEquipmentOperationReceipt
            {
                Id = Guid.NewGuid(), PlayerId = _playerId, CharacterId = _characterId,
                RequestId = command.RequestId, Operation = EraEquipmentOperation.Unequip,
                RequestPayload = "{}", ResultPayload = "{}"
            });
            await context.SaveChangesAsync();
        }));

        error.InnerException.ShouldBeOfType<MySqlException>().Number.ShouldBe(1062);
        await ReadDatabaseAsync(async context =>
        {
            var receipt = await context.Set<EraEquipmentOperationReceipt>().SingleAsync(row => row.PlayerId == _playerId);
            receipt.RequestId.ShouldBe(command.RequestId);
            receipt.Operation.ShouldBe(EraEquipmentOperation.Equip);
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion.ShouldBe(4);
        });
    }
}
