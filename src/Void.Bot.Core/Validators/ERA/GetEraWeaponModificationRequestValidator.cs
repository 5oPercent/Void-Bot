using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class GetEraWeaponModificationRequestValidator : FluentMessageValidator<GetEraWeaponModificationRequest>
{
    public GetEraWeaponModificationRequestValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
    }
}
