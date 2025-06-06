using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Security;

public class RemoveRoleCascadePermissionCommand : ICommand
{
    [Required]
    [MinLength(1)]
    public List<Guid> RoleIds { get; set; } = [];
}

public class RemoveRoleCascadePermissionResponse : VoidBotResponse
{
}