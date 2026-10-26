using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class GetEraArmorSlotTypeRequestValidator : FluentMessageValidator<GetEraArmorSlotTypeRequest>
{
    public GetEraArmorSlotTypeRequestValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
    }
}
