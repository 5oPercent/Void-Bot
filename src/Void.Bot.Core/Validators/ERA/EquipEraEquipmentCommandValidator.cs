using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class EquipEraEquipmentCommandValidator : FluentMessageValidator<EquipEraEquipmentCommand>
{
    public EquipEraEquipmentCommandValidator()
    {
        RuleFor(command => command.CharacterId).NotEmpty();
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.EquipmentId).NotEmpty();
        RuleFor(command => command.ExpectedEquipmentVersion).GreaterThanOrEqualTo(0);
        RuleFor(command => command.ExpectedItemVersion).GreaterThan(0);
        RuleFor(command => command.ExpectedManifestVersion).GreaterThan(0);
        // 槽位协议与原槽位成对参数交给 Service，保留稳定业务错误码。
    }
}
