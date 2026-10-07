using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Message.Commands.ERA;

public class CreateEraCharacterCommand : ICommand
{
    /// <summary>同一创建操作重试必须复用此 ID。</summary>
    public Guid RequestId { get; set; }

    public string CharacterDefinitionId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class CreateEraCharacterResponse : EraResponse<EraCharacterDto>
{
}
