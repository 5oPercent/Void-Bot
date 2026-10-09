using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class GetEraFriendTargetRequestValidator : FluentMessageValidator<GetEraFriendTargetRequest>
{
    public GetEraFriendTargetRequestValidator()
    {
        RuleFor(message => message.FriendAccountId).NotEmpty();
    }
}
