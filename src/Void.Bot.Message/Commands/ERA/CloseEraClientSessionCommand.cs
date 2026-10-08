using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;
using System.Text.Json.Serialization;

namespace Void.Bot.Message.Commands.ERA;

public class CloseEraClientSessionCommand : ICommand
{
    [JsonIgnore]
    public Guid ClientId { get; set; }
}

public class CloseEraClientSessionResponse : EraResponse<bool>
{
}
