using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;
using Void.Bot.Message.Constants.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class GetEraArmorSlotConfigurationsRequestValidator : FluentMessageValidator<GetEraArmorSlotConfigurationsRequest>
{
    public GetEraArmorSlotConfigurationsRequestValidator()
    {
        RuleFor(request => request.PageIndex).GreaterThanOrEqualTo(1);
        RuleFor(request => request.PageSize).InclusiveBetween(1, EraArmorConstants.MaximumPageSize);
        RuleFor(request => request.Search).MaximumLength(128);
    }
}
