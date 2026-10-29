using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class DestroyEraEquipmentDropsCommandValidator : FluentMessageValidator<DestroyEraEquipmentDropsCommand>
{
    public DestroyEraEquipmentDropsCommandValidator()
    {
        RuleFor(command => command.WorldInstanceId).NotEmpty();
        RuleFor(command => command.CharacterId).NotEmpty();
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.ExpectedEquipmentVersion).GreaterThanOrEqualTo(0);
        RuleFor(command => command.Items).NotEmpty();
        RuleForEach(command => command.Items).NotNull().ChildRules(item =>
        {
            item.RuleFor(value => value.DropId).NotEmpty();
            item.RuleFor(value => value.ExpectedDropVersion).GreaterThan(0);
            item.RuleFor(value => value.ExpectedItemVersion).GreaterThan(0);
        });
        RuleFor(command => command.Items).Must(items => items == null ||
            items.Where(item => item != null).Select(item => item.DropId).Distinct().Count() == items.Count);
    }
}
