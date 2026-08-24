using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Email;

public sealed class AuthorizeEmailCommand : ICommand
{
    public bool ForceReauthorization { get; init; }
}

public sealed class AuthorizeEmailResponse : VoidBotResponse<string>
{
}
