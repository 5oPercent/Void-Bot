using System.Text.Json.Serialization;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ValidateEraArmorConfigurationRequest : IRequest
{
    public EraArmorCatalogDto Configuration { get; set; } = new EraArmorCatalogDto();
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ValidateEraArmorConfigurationResponse : EraResponse<EraArmorConfigurationValidationDto>
{
}
