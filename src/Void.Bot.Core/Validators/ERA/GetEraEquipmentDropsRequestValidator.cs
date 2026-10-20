using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class GetEraEquipmentDropsRequestValidator : FluentMessageValidator<GetEraEquipmentDropsRequest>
{
    public GetEraEquipmentDropsRequestValidator()
    {
        RuleFor(request => request.WorldInstanceId).NotEmpty();
    }
}
