using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class CreateEraCharacterCommandValidator : FluentMessageValidator<CreateEraCharacterCommand>
{
    public CreateEraCharacterCommandValidator()
    {
        RuleFor(command => command.RequestId)
            .NotEmpty();
        RuleFor(command => command.CharacterDefinitionId)
            .NotEmpty()
            .MaximumLength(128);
        RuleFor(command => command.DisplayName)
            .NotEmpty()
            .MaximumLength(64);
    }
}
