using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class GetEraCharacterRequestValidator : FluentMessageValidator<GetEraCharacterRequest>
{
    public GetEraCharacterRequestValidator()
    {
        RuleFor(request => request.CharacterId)
            .NotEmpty();
    }
}
