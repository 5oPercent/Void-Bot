using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.Social;

namespace Void.Bot.Core.Validators.Social;

public class FindSocialAccountRequestValidator : FluentMessageValidator<FindSocialAccountRequest>
{
    public FindSocialAccountRequestValidator()
    {
        RuleFor(message => message.DisplayName).NotEmpty().MaximumLength(69).Matches(@"^.+#[0-9]{4}$");
    }
}
