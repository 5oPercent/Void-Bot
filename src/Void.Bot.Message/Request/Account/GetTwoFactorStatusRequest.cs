using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Request.Account;

public sealed class GetTwoFactorStatusRequest : IRequest
{
}

public sealed class GetTwoFactorStatusResponse : VoidBotResponse<TwoFactorStatusDto>
{
}

public sealed class TwoFactorStatusDto
{
    public bool IsEnabled { get; init; }

    public DateTimeOffset? EnabledAt { get; init; }
}
