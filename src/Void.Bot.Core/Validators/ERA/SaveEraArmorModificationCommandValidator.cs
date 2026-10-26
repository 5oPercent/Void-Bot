using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class SaveEraArmorModificationCommandValidator : FluentMessageValidator<SaveEraArmorModificationCommand>
{
    public SaveEraArmorModificationCommandValidator()
    {
        RuleFor(command => command.ExpectedManifestVersion).GreaterThan(0).LessThan(long.MaxValue);
        RuleFor(command => command.Modification).NotNull().ChildRules(definition =>
        {
            definition.RuleFor(value => value.Id).NotEmpty();
        });
    }
}
