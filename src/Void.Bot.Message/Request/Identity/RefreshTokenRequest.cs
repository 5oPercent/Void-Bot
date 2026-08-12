using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Identity;

namespace Void.Bot.Message.Request.Identity;

public class RefreshTokenRequest : IRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenResponse : VoidBotResponse<JwtToken>
{
}
