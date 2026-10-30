using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class DropEraInventoryEquipmentCommandValidator : FluentMessageValidator<DropEraInventoryEquipmentCommand>
{
    public DropEraInventoryEquipmentCommandValidator()
    {
        RuleFor(command => command.CharacterId).NotEmpty();
        RuleFor(command => command.EquipmentInstanceId).NotEmpty();
        RuleFor(command => command.WorldInstanceId).NotEmpty();
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.ExpectedEquipmentVersion).GreaterThanOrEqualTo(0);
        RuleFor(command => command.ExpectedItemVersion).GreaterThan(0);
        RuleFor(command => command.Placement).NotNull().Must(placement => placement != null &&
            double.IsFinite(placement.X) && double.IsFinite(placement.Y) &&
            double.IsFinite(placement.Z) && double.IsFinite(placement.YawRadians));
    }
}
