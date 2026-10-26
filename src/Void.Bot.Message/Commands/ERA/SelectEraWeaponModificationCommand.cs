using System.Text.Json.Serialization;
using System.Data;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

[TransactionIsolation(IsolationLevel.ReadCommitted)]
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SelectEraWeaponModificationCommand : IEraEquipmentModificationSelection
{
    public Guid CharacterId { get; set; }
    public Guid EquipmentId { get; set; }
    public Guid RequestId { get; set; }
    public Guid SlotId { get; set; }
    public Guid CandidateId { get; set; }
    public long ExpectedManifestVersion { get; set; }
    public long ExpectedEquipmentVersion { get; set; }
    public long ExpectedItemVersion { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SelectEraWeaponModificationResponse : EraEquipmentOperationResponse
{
}
