using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Security;

public class RemoveMenusCascadePermissionCommand : ICommand
{
    [Required]
    [MinLength(1)] 
    public List<Guid> MenuIds { get; set; } = [];
}

public class RemoveMenusCascadePermissionResponse : VoidBotResponse
{
}