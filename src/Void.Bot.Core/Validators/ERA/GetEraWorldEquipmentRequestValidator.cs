using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class GetEraWorldEquipmentRequestValidator : FluentMessageValidator<GetEraWorldEquipmentRequest>
{
    public GetEraWorldEquipmentRequestValidator()
    {
        RuleFor(request => request.WorldInstanceId).NotEmpty();
        RuleFor(request => request.MemberId).NotEmpty();
        RuleFor(request => request.AuthorityEpoch).GreaterThan(0);
    }
}
