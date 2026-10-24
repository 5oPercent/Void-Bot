using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public sealed class GetEraWorldEquipmentRequest : IRequest, IRequireEraPlayer
{
    public Guid WorldInstanceId { get; set; }
    public Guid MemberId { get; set; }
    public long AuthorityEpoch { get; set; }
}

public sealed class GetEraWorldEquipmentResponse : EraResponse<EraWorldEquipmentDto>
{
}
