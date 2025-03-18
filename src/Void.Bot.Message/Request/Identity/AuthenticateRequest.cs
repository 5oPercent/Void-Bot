using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Identity;

namespace Void.Bot.Message.Request.Identity;

public class AuthenticateRequest : IRequest
{
    [Required]
    [MinLength(1)]
    public string UserName { get; set; }
    
    [Required]
    [MinLength(4)]
    [MaxLength(4)]
    public string Suffix { get; set; } = "0000";
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; }
}

public class AuthenticateResponse : VoidBotResponse<JwtToken>
{
}