using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Identity;

namespace Void.Bot.Message.Commands.Identity;

public sealed class OAuthBindCommand : ICommand
{
    [Required]
    public required string Code { get; init; }

    [Required]
    [MinLength(1)]
    public required string Name { get; init; }

    [Required]
    [MinLength(4)]
    [MaxLength(4)]
    public string Suffix { get; init; } = "0000";

    [Required]
    [MinLength(6)]
    public required string Password { get; init; }

    [MinLength(6)]
    [MaxLength(6)]
    public string? TwoFactorCode { get; init; }
}

public sealed class OAuthBindResponse : VoidBotResponse<JwtToken>
{
    public bool RequiresTwoFactor { get; init; }
}
