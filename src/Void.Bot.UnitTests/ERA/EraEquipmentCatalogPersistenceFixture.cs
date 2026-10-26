using System.Text.Json;
using AutoMapper;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.UnitTest.Security;

namespace Void.Bot.UnitTest.ERA;

public class EraEquipmentCatalogPersistenceFixture
{
    [Fact]
    public async Task Should_rename_one_armor_template_without_rebuilding_related_tables()
    {
        var previous = new EraArmorCatalogDto
        {
            Templates = new() { new EraArmorTemplateDto { PresentationKey = "armor_test", Name = "Helmet", CompatibleSlots = new() { EraEquipmentSlot.Head } } }
        };
        var current = JsonSerializer.Deserialize<EraArmorCatalogDto>(JsonSerializer.Serialize(previous))!;
        current.Templates[0].Name = "Renamed";
        current.Templates[0].PresentationKey = "armor_core_001";
        var saved = new EraArmorTemplate { Id = previous.Templates[0].Id, Name = previous.Templates[0].Name };
        var unrelated = new EraArmorTemplate { Id = Guid.NewGuid(), Name = "Unrelated" };
        var templates = Substitute.For<IRepository<EraArmorTemplate>>();
        templates.Query().Returns(new AsyncQueryableWrapper<EraArmorTemplate>(new[] { saved, unrelated }));
        var modifiers = Substitute.For<IRepository<EraArmorModStatModifier>>();
        var slots = Substitute.For<IRepository<EraArmorConfigurationSlot>>();
        var pools = Substitute.For<IRepository<EraArmorConfigurationModPool>>();
        var provider = new EraArmorDataProvider(templates, Substitute.For<IRepository<EraArmorModDefinition>>(), modifiers, slots, pools,
            Substitute.For<IRepository<EraArmorModCandidate>>(), Substitute.For<IRepository<EraArmorModSelection>>(),
            Substitute.For<IRepository<EraEquipmentConfiguration>>(), Substitute.For<IRepository<EraEquipmentInstance>>(),
            Substitute.For<IRepository<EraRewardEvent>>(), Substitute.For<IRepository<EraRewardConfiguration>>(),
            Substitute.For<IRepository<EraArmorSlotType>>(), Substitute.For<IRepository<EraArmorSlotConfiguration>>(), Substitute.For<IMapper>());
        await provider.SaveCatalogAsync(previous, current, CancellationToken.None);
        saved.Name.ShouldBe("Renamed");
        saved.PresentationKey.ShouldBe("armor_core_001");
        unrelated.Name.ShouldBe("Unrelated");
        templates.DidNotReceiveWithAnyArgs().Insert(default!);
        templates.DidNotReceiveWithAnyArgs().Delete(default!);
        await modifiers.DidNotReceiveWithAnyArgs().ExecuteDeleteAsync(default!, default);
        await slots.DidNotReceiveWithAnyArgs().ExecuteDeleteAsync(default!, default);
        await pools.DidNotReceiveWithAnyArgs().ExecuteDeleteAsync(default!, default);
        modifiers.DidNotReceiveWithAnyArgs().Insert(default!);
        slots.DidNotReceiveWithAnyArgs().Insert(default!);
        pools.DidNotReceiveWithAnyArgs().Insert(default!);
    }


}
