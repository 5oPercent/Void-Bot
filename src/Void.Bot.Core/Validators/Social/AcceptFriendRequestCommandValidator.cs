using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.Social;

namespace Void.Bot.Core.Validators.Social;

public class AcceptFriendRequestCommandValidator : FluentMessageValidator<AcceptFriendRequestCommand>
{
    public AcceptFriendRequestCommandValidator()
    {
        RuleFor(message => message.FriendRequestId).NotEmpty();
    }
}
