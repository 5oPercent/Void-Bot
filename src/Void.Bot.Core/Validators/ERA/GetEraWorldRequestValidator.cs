using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class GetEraWorldRequestValidator : FluentMessageValidator<GetEraWorldRequest>
{
    public GetEraWorldRequestValidator()
    {
        RuleFor(request => request.WorldInstanceId).NotEmpty();
    }
}
