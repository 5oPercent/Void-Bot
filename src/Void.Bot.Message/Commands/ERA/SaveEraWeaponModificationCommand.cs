using System.Text.Json.Serialization;
using System.Data;
using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

[TransactionIsolation(IsolationLevel.ReadCommitted)]
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SaveEraWeaponModificationCommand : ICommand
{
    public long ExpectedManifestVersion { get; set; }
    public EraWeaponModDefinitionDto Modification { get; set; } = new EraWeaponModDefinitionDto();
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SaveEraWeaponModificationResponse : EraResponse<EraWeaponConfigurationResultDto>
{
}
