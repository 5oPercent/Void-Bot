using System.Text.Json.Serialization;
using System.Data;
using Mediator.Net.Contracts;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

[TransactionIsolation(IsolationLevel.ReadCommitted)]
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class DeleteEraWeaponSlotConfigurationCommand : ICommand
{
    public long ExpectedManifestVersion { get; set; }
    public Guid Id { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class DeleteEraWeaponSlotConfigurationResponse : EraResponse<EraWeaponConfigurationResultDto>
{
}
