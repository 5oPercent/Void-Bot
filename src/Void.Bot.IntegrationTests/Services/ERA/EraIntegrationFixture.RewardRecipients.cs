using System.Net;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    [Theory]
    [InlineData(5)]
    [InlineData(16)]
    [Trait("Category", "ERARewardIntegration")]
    public async Task Internal_inventory_rewards_should_use_the_supplied_roster_without_rechecking_team_size_settings(int count)
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync();
        var command = RewardCommand(includeOther: true);
        await ReadDatabaseAsync(async context =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            var characters = new List<EraCharacter>();
            for (var index = 2; index < count; index++)
            {
                var player = new EraPlayer { Id = Guid.NewGuid(), WarehouseCapacity = 300, Version = 1 };
                var character = new EraCharacter
                {
                    Id = Guid.NewGuid(), PlayerId = player.Id, CreationRequestId = Guid.NewGuid(), CharacterDefinitionId = "test-character",
                    DisplayName = "Reward member " + index, Version = 12, EquipmentVersion = 3
                };
                context.Set<EraPlayer>().Add(player);
                characters.Add(character);
                command.Recipients.Add(new EraRewardRecipient { PlayerId = player.Id, CharacterId = character.Id });
            }
            // 数据库有角色到玩家的外键，当前 EF 模型未映射该关系，需显式先保存所有玩家。
            await context.SaveChangesAsync();
            context.Set<EraCharacter>().AddRange(characters);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        });
        // 内部能力不绑定队伍，可信业务提供的名单不应被当前网络配置重复限制。
        _worldTestMaxMembers = 2;
        var plan = await PrepareRewardAsync(command);
        plan.Code.ShouldBe(HttpStatusCode.OK);
        _worldTestMaxMembers = 16;
        command.RequestId = Guid.NewGuid();
        (await PrepareRewardAsync(command)).IsReplay.ShouldBeTrue();
        var granted = await GrantRewardAsync(plan.Data);
        granted.Code.ShouldBe(HttpStatusCode.OK);
        granted.Data!.Grants.Count.ShouldBe(count);
        granted.Data.Grants.ShouldAllBe(grant => grant.EquipmentInstanceIds.Count == 1 && grant.EquipmentVersion == 4);
        (await GrantRewardAsync(plan.Data)).IsReplay.ShouldBeTrue();
        _worldTestMaxMembers = 2;
        command.SourceInstanceId = Guid.NewGuid();
        command.RequestId = Guid.NewGuid();
        var next = await PrepareRewardAsync(command);
        (await GrantRewardAsync(next.Data)).Data!.Grants.Count.ShouldBe(count);
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardEvent>().CountAsync()).ShouldBe(2);
            (await context.Set<EraRewardGrant>().CountAsync()).ShouldBe(count * 2);
            (await context.Set<EraEquipmentDrop>().CountAsync()).ShouldBe(0);
            (await context.Set<EraEquipmentShare>().CountAsync()).ShouldBe(0);
        });
    }
}
