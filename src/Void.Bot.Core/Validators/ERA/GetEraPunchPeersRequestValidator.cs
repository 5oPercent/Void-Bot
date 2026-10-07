using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class GetEraPunchPeersRequestValidator : FluentMessageValidator<GetEraPunchPeersRequest>
{
    public GetEraPunchPeersRequestValidator()
    {
        RuleFor(message => message.TeamId)
            .NotEmpty();
    }
}
