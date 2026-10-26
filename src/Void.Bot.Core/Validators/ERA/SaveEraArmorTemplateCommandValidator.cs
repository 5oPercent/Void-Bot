using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class SaveEraArmorTemplateCommandValidator : FluentMessageValidator<SaveEraArmorTemplateCommand>
{
    public SaveEraArmorTemplateCommandValidator()
    {
        RuleFor(command => command.ExpectedManifestVersion).GreaterThan(0).LessThan(long.MaxValue);
        RuleFor(command => command.Template).NotNull().ChildRules(definition =>
        {
            definition.RuleFor(value => value.Id).NotEmpty();
        });
    }
}
