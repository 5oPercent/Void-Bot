using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class GetEraArmorModificationRequestValidator : FluentMessageValidator<GetEraArmorModificationRequest>
{
    public GetEraArmorModificationRequestValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
    }
}
