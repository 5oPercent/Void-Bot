using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.Social;

namespace Void.Bot.Core.Validators.Social;

public class GetFriendsRequestValidator : FluentMessageValidator<GetFriendsRequest>
{
    public GetFriendsRequestValidator()
    {
        RuleFor(message => message.PageIndex).InclusiveBetween(1, 100000);
        RuleFor(message => message.PageSize).InclusiveBetween(1, 100);
    }
}
