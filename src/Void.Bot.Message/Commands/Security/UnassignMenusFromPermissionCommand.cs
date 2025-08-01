using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Security;

public class UnassignMenusFromPermissionCommand : ICommand
{
    public Guid PermissionId { get; set; }
    
    [Required]
    [MinLength(1)]
    public List<Guid> MenuIds { get; set; } = [];
}

public class UnassignMenusFromPermissionResponse : VoidBotResponse
{
}