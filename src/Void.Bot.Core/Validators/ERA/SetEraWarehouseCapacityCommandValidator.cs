using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class SetEraWarehouseCapacityCommandValidator : FluentMessageValidator<SetEraWarehouseCapacityCommand>
{
    public SetEraWarehouseCapacityCommandValidator()
    {
        RuleFor(command => command.PlayerId)
            .NotEmpty();
        RuleFor(command => command.RequestId)
            .NotEmpty();
        RuleFor(command => command.ExpectedVersion)
            .GreaterThan(0)
            .LessThan(long.MaxValue);
        RuleFor(command => command.Capacity)
            .GreaterThanOrEqualTo(0);
        RuleFor(command => command.Reason)
            .NotEmpty()
            .MaximumLength(512);
    }
}
