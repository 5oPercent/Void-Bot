using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class ValidateEraArmorConfigurationRequestValidator : FluentMessageValidator<ValidateEraArmorConfigurationRequest>
{
    public ValidateEraArmorConfigurationRequestValidator()
    {
        RuleFor(request => request.Configuration).NotNull();
    }
}
