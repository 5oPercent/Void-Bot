using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class GetEraFriendStatusesRequestValidator : FluentMessageValidator<GetEraFriendStatusesRequest>
{
    public GetEraFriendStatusesRequestValidator()
    {
        RuleFor(message => message.AccountIds).NotNull().Must(values => values != null && values.Count <= 100 && values.All(value => value != Guid.Empty));
    }
}
