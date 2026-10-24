using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class ValidateEraWeaponConfigurationRequestValidator : FluentMessageValidator<ValidateEraWeaponConfigurationRequest>
{
    public ValidateEraWeaponConfigurationRequestValidator()
    {
        RuleFor(request => request.Configuration).NotNull();
    }
}
