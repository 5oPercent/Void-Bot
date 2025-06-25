using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Security;

public class AddRoleConflictMatrixCommand : ICommand
{
    public Guid RoleAId { get; set; }
    
    public Guid RoleBId { get; set; }
}

public class AddRoleConflictMatrixResponse : VoidBotResponse
{
}