using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Enum;

namespace Void.Bot.Message.Request.Account;

public class GetUserAccountByThirdPartyRequest : IRequest
{
    public AppPlatformEnum Platform { get; set; }
    
    public required string ThirdPartyUserId { get; set; }
}

public class GetUserAccountByThirdPartyResponse : VoidBotResponse<UserAccountInfoDto>
{
}