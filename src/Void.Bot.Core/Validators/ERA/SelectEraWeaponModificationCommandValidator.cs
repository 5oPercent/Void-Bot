using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class SelectEraWeaponModificationCommandValidator : FluentMessageValidator<SelectEraWeaponModificationCommand>
{
    public SelectEraWeaponModificationCommandValidator()
    {
        RuleFor(command => command.CharacterId).NotEmpty();
        RuleFor(command => command.EquipmentId).NotEmpty();
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.CandidateId).NotEmpty();
        RuleFor(command => command.Slot).IsInEnum();
        RuleFor(command => command.ExpectedManifestVersion).GreaterThan(0);
        RuleFor(command => command.ExpectedEquipmentVersion).GreaterThanOrEqualTo(0).LessThan(long.MaxValue);
        RuleFor(command => command.ExpectedItemVersion).GreaterThan(0).LessThan(long.MaxValue);
    }
}
