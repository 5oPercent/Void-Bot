using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class AdjustEraWarehouseCapacityCommandValidator : FluentMessageValidator<AdjustEraWarehouseCapacityCommand>
{
    public AdjustEraWarehouseCapacityCommandValidator()
    {
        RuleFor(command => command.PlayerId)
            .NotEmpty();
        RuleFor(command => command.RequestId)
            .NotEmpty();
        RuleFor(command => command.ExpectedVersion)
            .GreaterThan(0)
            .LessThan(long.MaxValue);
        RuleFor(command => command.Delta)
            .NotEqual(0);
        RuleFor(command => command.Reason)
            .NotEmpty()
            .MaximumLength(512);
    }
}
