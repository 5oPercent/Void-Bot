using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Shouldly;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    [Fact]
    [Trait("Category", "ERARewardIntegration")]
    public async Task Database_should_enforce_business_identity_even_when_source_definition_changes()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync();
        await PrepareRewardAsync(RewardCommand());
        var error = await Should.ThrowAsync<DbUpdateException>(() => ReadDatabaseAsync(async context =>
        {
            var original = await context.Set<EraRewardEvent>().AsNoTracking().SingleAsync();
            var duplicate = JsonSerializer.Deserialize<EraRewardEvent>(JsonSerializer.Serialize(original))!;
            duplicate.Id = Guid.NewGuid();
            duplicate.SourceDefinitionId = "another-definition";
            context.Set<EraRewardEvent>().Add(duplicate);
            await context.SaveChangesAsync();
        }));
        error.InnerException.ShouldBeOfType<MySqlException>().Number.ShouldBe(1062);
        await ReadDatabaseAsync(async context => (await context.Set<EraRewardEvent>().CountAsync()).ShouldBe(1));
    }

    [Fact]
    [Trait("Category", "ERARewardIntegration")]
    public async Task Database_should_reject_duplicate_player_grants_and_request_aliases_from_another_scope()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync();
        var prepared = await PrepareRewardAsync(RewardCommand());
        await GrantRewardAsync(prepared.Data);
        var duplicateGrant = await Should.ThrowAsync<DbUpdateException>(() => ReadDatabaseAsync(async context =>
        {
            var original = await context.Set<EraRewardGrant>().AsNoTracking().SingleAsync();
            var duplicate = JsonSerializer.Deserialize<EraRewardGrant>(JsonSerializer.Serialize(original))!;
            duplicate.Id = Guid.NewGuid();
            context.Set<EraRewardGrant>().Add(duplicate);
            await context.SaveChangesAsync();
        }));
        duplicateGrant.InnerException.ShouldBeOfType<MySqlException>().Number.ShouldBe(1062);
        var wrongScope = await Should.ThrowAsync<DbUpdateException>(() => ReadDatabaseAsync(async context =>
        {
            context.Set<EraRewardRequest>().Add(new EraRewardRequest
            {
                ScopeId = Guid.NewGuid(), RequestId = Guid.NewGuid(), EventId = prepared.Data
            });
            await context.SaveChangesAsync();
        }));
        wrongScope.InnerException.ShouldBeOfType<MySqlException>().Number.ShouldBe(1452);
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardGrant>().CountAsync()).ShouldBe(1);
            (await context.Set<EraRewardRequest>().CountAsync()).ShouldBe(1);
        });
    }
}
