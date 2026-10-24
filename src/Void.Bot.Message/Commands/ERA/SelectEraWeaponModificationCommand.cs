using System.Text.Json.Serialization;
using System.Data;
using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Message.Commands.ERA;

[TransactionIsolation(IsolationLevel.ReadCommitted)]
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SelectEraWeaponModificationCommand : ICommand, IRequireEraPlayer
{
    public Guid CharacterId { get; set; }
    public Guid EquipmentId { get; set; }
    public Guid RequestId { get; set; }
    public EraWeaponModSlot Slot { get; set; }
    public Guid CandidateId { get; set; }
    public long ExpectedManifestVersion { get; set; }
    public long ExpectedEquipmentVersion { get; set; }
    public long ExpectedItemVersion { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SelectEraWeaponModificationResponse : EraEquipmentOperationResponse
{
}
