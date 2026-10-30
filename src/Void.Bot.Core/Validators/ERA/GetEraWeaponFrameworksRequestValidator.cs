using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;
using Void.Bot.Message.Constants.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class GetEraWeaponFrameworksRequestValidator : FluentMessageValidator<GetEraWeaponFrameworksRequest>
{
    public GetEraWeaponFrameworksRequestValidator()
    {
        RuleFor(request => request.PageIndex).GreaterThanOrEqualTo(1);
        RuleFor(request => request.PageSize).InclusiveBetween(1, EraWeaponConstants.MaximumPageSize);
        RuleFor(request => request.Search).MaximumLength(128);
        RuleFor(request => request.PresentationDriverKey).Must(value => value == null || value is "Firearm" or "Bow" or "Crossbow" or "Melee");
    }
}
