using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Account;

namespace Void.Bot.Message.Request.Account;

public class GetUserAccountRequest : IRequest
{
    public Guid? UserId { get; set; }
    
    public string DisplayName { get; set; } // Tips: Cy#0139
}

public class GetUserAccountResponse : VoidBotResponse<UserAccountInfoDto>
{
}