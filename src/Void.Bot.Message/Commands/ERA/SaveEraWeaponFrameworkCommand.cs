using System.Text.Json.Serialization;
using System.Data;
using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

[TransactionIsolation(IsolationLevel.ReadCommitted)]
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SaveEraWeaponFrameworkCommand : ICommand
{
    public long ExpectedManifestVersion { get; set; }
    public EraWeaponFrameworkDto Framework { get; set; } = new EraWeaponFrameworkDto();
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SaveEraWeaponFrameworkResponse : EraResponse<EraWeaponConfigurationResultDto>
{
}
