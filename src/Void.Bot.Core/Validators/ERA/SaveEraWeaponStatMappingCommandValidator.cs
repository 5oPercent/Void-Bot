using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class SaveEraWeaponStatMappingCommandValidator : FluentMessageValidator<SaveEraWeaponStatMappingCommand>
{
    public SaveEraWeaponStatMappingCommandValidator()
    {
        RuleFor(command => command.ExpectedManifestVersion).GreaterThan(0).LessThan(long.MaxValue);
        RuleFor(command => command.StatMapping).NotNull().ChildRules(definition =>
        {
            definition.RuleFor(value => value.Id).NotEmpty();
        });
    }
}
