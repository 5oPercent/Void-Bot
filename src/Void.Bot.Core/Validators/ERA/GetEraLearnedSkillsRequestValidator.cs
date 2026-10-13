using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class GetEraLearnedSkillsRequestValidator : FluentMessageValidator<GetEraLearnedSkillsRequest>
{
    public GetEraLearnedSkillsRequestValidator()
    {
        RuleFor(request => request.CharacterId)
            .NotEmpty();
    }
}
