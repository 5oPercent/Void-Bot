using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class DeleteEraCharacterCommandValidator : FluentMessageValidator<DeleteEraCharacterCommand>
{
    public DeleteEraCharacterCommandValidator()
    {
        RuleFor(command => command.CharacterId)
            .NotEmpty();
    }
}