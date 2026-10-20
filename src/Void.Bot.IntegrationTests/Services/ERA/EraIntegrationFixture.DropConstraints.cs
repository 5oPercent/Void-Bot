using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Database_should_reject_two_available_drop_records_for_the_same_equipment()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        await SubmitGroundAsync(GroundCommand(world, team));
        var error = await Should.ThrowAsync<DbUpdateException>(() => ReadDatabaseAsync(async context =>
        {
            var original = await context.Set<EraEquipmentDrop>().AsNoTracking().SingleAsync();
            context.Set<EraEquipmentDrop>().Add(new EraEquipmentDrop
            {
                Id = Guid.NewGuid(), EquipmentId = original.EquipmentId, ActiveEquipmentId = original.EquipmentId,
                WorldInstanceId = original.WorldInstanceId, PrivateOwnerPlayerId = original.PrivateOwnerPlayerId,
                Status = EraEquipmentDropStatus.Available, Version = 1, CreatedAt = original.CreatedAt
            });
            await context.SaveChangesAsync();
        }));
        error.InnerException.ShouldBeOfType<MySqlException>().Number.ShouldBe(1062);
    }
}
