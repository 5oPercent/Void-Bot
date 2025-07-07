using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Account;

namespace Void.Bot.Message.Commands.Account;

public class RegisterUserAccountCommand : ICommand
{
    [Required]
    public required string Name { get; set; }
    
    [Required]
    public required string Password { get; set; }
}

public class RegisterUserAccountResponse : VoidBotResponse<UserAccountInfoDto>
{
}