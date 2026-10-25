using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class GetEraWeaponSlotConfigurationRequestValidator : FluentMessageValidator<GetEraWeaponSlotConfigurationRequest>
{
    public GetEraWeaponSlotConfigurationRequestValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
    }
}
