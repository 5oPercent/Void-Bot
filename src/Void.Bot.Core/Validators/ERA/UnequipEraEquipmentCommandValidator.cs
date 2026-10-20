using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class UnequipEraEquipmentCommandValidator : FluentMessageValidator<UnequipEraEquipmentCommand>
{
    public UnequipEraEquipmentCommandValidator()
    {
        RuleFor(command => command.CharacterId).NotEmpty();
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.EquipmentId).NotEmpty();
        RuleFor(command => command.ExpectedEquipmentVersion).GreaterThanOrEqualTo(0);
        RuleFor(command => command.ExpectedItemVersion).GreaterThan(0);
    }
}
