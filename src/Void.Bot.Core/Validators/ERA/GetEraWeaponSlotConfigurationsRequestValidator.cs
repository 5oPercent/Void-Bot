using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;
using Void.Bot.Message.Constants.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class GetEraWeaponSlotConfigurationsRequestValidator : FluentMessageValidator<GetEraWeaponSlotConfigurationsRequest>
{
    public GetEraWeaponSlotConfigurationsRequestValidator()
    {
        RuleFor(request => request.PageIndex).GreaterThanOrEqualTo(1);
        RuleFor(request => request.PageSize).InclusiveBetween(1, EraWeaponConstants.MaximumPageSize);
        RuleFor(request => request.Search).MaximumLength(128);
    }
}
