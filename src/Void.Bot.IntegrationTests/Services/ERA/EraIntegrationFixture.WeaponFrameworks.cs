using System.Net;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    [Fact]
    public async Task Framework_configuration_should_publish_query_disable_and_protect_template_references()
    {
        await SeedAsync(0);
        var catalog = WeaponCatalog();
        await PublishWeaponsAsync(catalog);
        var id = catalog.Frameworks.Single().Id;
        var list = await RequestAsync((mediator, token) => mediator.RequestAsync<GetEraWeaponFrameworksRequest, GetEraWeaponFrameworksResponse>(
            new GetEraWeaponFrameworksRequest { PageIndex = 1, PageSize = 10, PresentationDriverKey = "Firearm", IsEnabled = true }, token));
        list.Version.ShouldBe(8);
        list.Data!.Items.Single().Id.ShouldBe(id);
        var detail = await RequestAsync((mediator, token) => mediator.RequestAsync<GetEraWeaponFrameworkRequest, GetEraWeaponFrameworkResponse>(new GetEraWeaponFrameworkRequest { Id = id }, token));
        detail.TemplateIds.ShouldBe(new[] { catalog.Templates[0].Id });
        detail.Data!.Capabilities.Single(capability => capability.Kind == EraWeaponCapabilityKind.Hitscan).Hitscan!.MaximumDistance.ShouldBe(30);
        var disabled = await RequestAsync((mediator, token) => mediator.SendAsync<DisableEraWeaponFrameworkCommand, DisableEraWeaponFrameworkResponse>(
            new DisableEraWeaponFrameworkCommand { Id = id, ExpectedManifestVersion = 8 }, token));
        disabled.Data!.Version.ShouldBe(9);
        disabled.Data.Configuration.Frameworks.Single().IsEnabled.ShouldBeFalse();
        disabled.Data.Configuration.Templates.Single().IsGenerationEnabled.ShouldBeTrue();
        var error = await Should.ThrowAsync<EraWeaponException>(() => RequestAsync((mediator, token) =>
            mediator.SendAsync<DeleteEraWeaponFrameworkCommand, DeleteEraWeaponFrameworkResponse>(new DeleteEraWeaponFrameworkCommand { Id = id, ExpectedManifestVersion = 9 }, token)));
        error.Code.ShouldBe(HttpStatusCode.Conflict);
        await ReadDatabaseAsync(async context =>
        {
            var framework = await context.Set<EraWeaponFramework>().SingleAsync();
            framework.IsEnabled.ShouldBeFalse();
            (await context.Set<EraWeaponTemplate>().SingleAsync()).FrameworkId.ShouldBe(id);
            (await context.Set<EraWeaponTemplateStat>().CountAsync()).ShouldBe(9);
            var handling = await context.Set<EraWeaponTemplateStat>().SingleAsync(entry => entry.Stat == EraWeaponStat.Handling);
            handling.EffectAtZero.ShouldBe(1);
            handling.EffectAtHundred.ShouldBe(.35);
            (await context.Set<EraWeaponFrameworkStat>().CountAsync()).ShouldBe(3);
            (await context.Set<EraWeaponFrameworkCapability>().CountAsync()).ShouldBe(5);
            (await context.Set<EraEquipmentConfiguration>().SingleAsync(row => row.Id == ConfigurationId)).Version.ShouldBe(9);
        });
    }
}
