using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class SaveEraWeaponFrameworkCommandValidator : FluentMessageValidator<SaveEraWeaponFrameworkCommand>
{
    public SaveEraWeaponFrameworkCommandValidator()
    {
        RuleFor(command => command.ExpectedManifestVersion).GreaterThan(0).LessThan(long.MaxValue);
        RuleFor(command => command.Framework).NotNull().ChildRules(definition =>
        {
            definition.RuleFor(value => value.Id).NotEmpty();
        });
    }
}
