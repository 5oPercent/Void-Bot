using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class GetEraSkillProfileRequestValidator : FluentMessageValidator<GetEraSkillProfileRequest>
{
    public GetEraSkillProfileRequestValidator()
    {
        RuleFor(request => request.CharacterId)
            .NotEmpty();
    }
}
