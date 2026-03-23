using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Commands.Security;

public class AddRoleCommand : RoleBaseDto, ICommand
{
}

public class AddRoleResponse : VoidBotResponse
{
}