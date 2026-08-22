using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Identity;

namespace Void.Bot.Message.Commands.Identity;

public sealed class OAuthRegisterCommand : ICommand
{
    [Required]
    public required string Code { get; init; }

    [Required]
    public required string Name { get; init; }

    [Required]
    public required string Password { get; init; }
}

public sealed class OAuthRegisterResponse : VoidBotResponse<JwtToken>
{
}
