using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class GetEraLearnedResonancesRequestValidator : FluentMessageValidator<GetEraLearnedResonancesRequest>
{
    public GetEraLearnedResonancesRequestValidator()
    {
        RuleFor(request => request.CharacterId)
            .NotEmpty();
    }
}
