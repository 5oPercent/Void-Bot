using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class PickupEraEquipmentDropCommandValidator : FluentMessageValidator<PickupEraEquipmentDropCommand>
{
    public PickupEraEquipmentDropCommandValidator()
    {
        RuleFor(command => command.WorldInstanceId).NotEmpty();
        RuleFor(command => command.DropId).NotEmpty();
        RuleFor(command => command.CharacterId).NotEmpty();
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.ExpectedEquipmentVersion).GreaterThanOrEqualTo(0);
        RuleFor(command => command.ExpectedDropVersion).GreaterThan(0);
        RuleFor(command => command.ExpectedItemVersion).GreaterThan(0);
    }
}
