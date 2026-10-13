using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class GrantEraLearnedSkillCommandValidator : FluentMessageValidator<GrantEraLearnedSkillCommand>
{
    public GrantEraLearnedSkillCommandValidator()
    {
        RuleFor(command => command.CharacterId)
            .NotEmpty();
        RuleFor(command => command.SkillId)
            .NotEmpty()
            .MaximumLength(128);
    }
}
