using System.Text.Json.Serialization;
using System.Data;
using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

[TransactionIsolation(IsolationLevel.ReadCommitted)]
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SaveEraArmorSlotConfigurationCommand : ICommand
{
    public long ExpectedManifestVersion { get; set; }
    public EraArmorSlotConfigurationDto SlotConfiguration { get; set; } = new EraArmorSlotConfigurationDto();
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SaveEraArmorSlotConfigurationResponse : EraResponse<EraArmorConfigurationResultDto>
{
}
