using System.Text.Json.Serialization;
using System.Data;
using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

[TransactionIsolation(IsolationLevel.ReadCommitted)]
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SaveEraArmorSlotTypeCommand : ICommand
{
    public long ExpectedManifestVersion { get; set; }
    public EraArmorSlotTypeDto SlotType { get; set; } = new EraArmorSlotTypeDto();
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SaveEraArmorSlotTypeResponse : EraResponse<EraArmorConfigurationResultDto>
{
}
