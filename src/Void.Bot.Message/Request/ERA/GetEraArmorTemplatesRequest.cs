using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraArmorTemplatesRequest : PageInputDto, IRequest
{
    public string? Search { get; set; }
}

public sealed class GetEraArmorTemplatesResponse : EraResponse<PagedListDto<EraArmorTemplateDto>>
{
    public long Version { get; set; }
}
