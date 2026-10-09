using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.Social;

namespace Void.Bot.Core.Validators.Social;

public class GetAccountBlocksRequestValidator : FluentMessageValidator<GetAccountBlocksRequest>
{
    public GetAccountBlocksRequestValidator()
    {
        RuleFor(message => message.PageIndex).InclusiveBetween(1, 100000);
        RuleFor(message => message.PageSize).InclusiveBetween(1, 100);
    }
}
