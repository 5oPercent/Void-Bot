using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class GetEraWeaponFrameworkRequestValidator : FluentMessageValidator<GetEraWeaponFrameworkRequest>
{
    public GetEraWeaponFrameworkRequestValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
    }
}
