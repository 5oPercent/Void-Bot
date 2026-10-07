using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

public class DeleteEraCharacterCommand : ICommand
{
    public Guid CharacterId { get; set; }
}

public class DeleteEraCharacterResponse : EraResponse<Guid>
{
}