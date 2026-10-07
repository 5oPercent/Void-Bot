using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Request.ERA;

public class GetEraPunchPeersRequest : IRequest
{
    public Guid TeamId { get; set; }
}

public class GetEraPunchPeersResponse : EraResponse<EraPunchPeersDto>
{
}
