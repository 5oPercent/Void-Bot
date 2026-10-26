using Mediator.Net.Contracts;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Message.Commands.ERA;

/// <summary>武器与护甲选择固定改装候选时共用的归属、幂等和版本字段。</summary>
public interface IEraEquipmentModificationSelection : ICommand, IRequireEraPlayer
{
    Guid CharacterId { get; set; }
    Guid EquipmentId { get; set; }
    Guid RequestId { get; set; }
    Guid SlotId { get; set; }
    Guid CandidateId { get; set; }
    long ExpectedManifestVersion { get; set; }
    long ExpectedEquipmentVersion { get; set; }
    long ExpectedItemVersion { get; set; }
}
