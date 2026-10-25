using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class DestroyEraInventoryEquipmentCommandValidator : FluentMessageValidator<DestroyEraInventoryEquipmentCommand>
{
    public DestroyEraInventoryEquipmentCommandValidator()
    {
        RuleFor(command => command.CharacterId).NotEmpty();
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.ExpectedEquipmentVersion).GreaterThanOrEqualTo(0);
        RuleFor(command => command.Items).NotNull().Must(items => items != null &&
            items.Count is > 0 and <= EraEquipmentRules.InventoryCapacity &&
            items.All(item => item != null && item.EquipmentId != Guid.Empty && item.ExpectedItemVersion > 0) &&
            items.Select(item => item.EquipmentId).Distinct().Count() == items.Count);
    }
}
