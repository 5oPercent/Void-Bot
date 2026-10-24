using System.Text.Json.Serialization;
using System.Data;
using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

[TransactionIsolation(IsolationLevel.ReadCommitted)]
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SaveEraWeaponStatMappingCommand : ICommand
{
    public long ExpectedManifestVersion { get; set; }
    public EraWeaponStatMappingDto StatMapping { get; set; } = new EraWeaponStatMappingDto();
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SaveEraWeaponStatMappingResponse : EraResponse<EraWeaponConfigurationResultDto>
{
}
