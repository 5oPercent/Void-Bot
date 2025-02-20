using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Enum;

namespace Void.Bot.Message.Request.Account;

public class GetThirdPartyByThirdPartyRequest : IRequest
{
    public AppPlatformEnum CurrentPlatform { get; set; }
    
    [Required]
    public string ThirdPartyUserId { get; set; }
    
    public AppPlatformEnum TargetPlatform { get; set; }
}

public class GetThirdPartyByThirdPartyResponse : VoidBotResponse<UserThirdPartyDto>
{
}