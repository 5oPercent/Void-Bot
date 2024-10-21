using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Account;

namespace Void.Bot.Message.Commands.Account;

public class RegisterUserAccountCommand : ICommand
{
    public string Name { get; set; }
    
    public string Password { get; set; }
}

public class RegisterUserAccountResponse : VoidBotResponse<UserAccountInfoDto>
{
}