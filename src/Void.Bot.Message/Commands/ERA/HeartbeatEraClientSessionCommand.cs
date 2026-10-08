using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;
using System.Text.Json.Serialization;

namespace Void.Bot.Message.Commands.ERA;

public class HeartbeatEraClientSessionCommand : ICommand
{
    [JsonIgnore]
    public Guid ClientId { get; set; }
}

public class HeartbeatEraClientSessionResponse : EraResponse<bool>
{
}
