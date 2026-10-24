using System.Text.Json.Serialization;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ValidateEraWeaponConfigurationRequest : IRequest
{
    public EraWeaponCatalogDto Configuration { get; set; } = new EraWeaponCatalogDto();
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ValidateEraWeaponConfigurationResponse : EraResponse<EraWeaponConfigurationValidationDto>
{
}
