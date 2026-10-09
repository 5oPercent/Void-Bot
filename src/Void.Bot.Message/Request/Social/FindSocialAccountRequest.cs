using Void.Bot.Message.Dto;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto.Social;

namespace Void.Bot.Message.Request.Social;

public class FindSocialAccountRequest : IRequest
{
    public string DisplayName { get; set; } = string.Empty;
}

public class FindSocialAccountResponse : VoidBotResponse<SocialAccountDto>
{
}
