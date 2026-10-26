using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class SaveEraArmorSlotConfigurationCommandValidator : FluentMessageValidator<SaveEraArmorSlotConfigurationCommand>
{
    public SaveEraArmorSlotConfigurationCommandValidator()
    {
        RuleFor(command => command.ExpectedManifestVersion).GreaterThan(0).LessThan(long.MaxValue);
        RuleFor(command => command.SlotConfiguration).NotNull().ChildRules(definition =>
        {
            definition.RuleFor(value => value.Id).NotEmpty();
            definition.RuleFor(value => value.Slots).NotNull();
            definition.RuleForEach(value => value.Slots).NotNull().ChildRules(slot =>
            {
                slot.RuleFor(value => value.Id).NotEmpty();
            });
        });
    }
}
