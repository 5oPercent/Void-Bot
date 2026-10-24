using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class GetEraWeaponStatMappingRequestValidator : FluentMessageValidator<GetEraWeaponStatMappingRequest>
{
    public GetEraWeaponStatMappingRequestValidator()
    {
        RuleFor(request => request.Id).NotEmpty();
    }
}
