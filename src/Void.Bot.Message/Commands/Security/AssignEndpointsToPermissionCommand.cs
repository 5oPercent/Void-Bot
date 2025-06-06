using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Security;

public class AssignEndpointsToPermissionCommand : ICommand
{
    public Guid PermissionId { get; set; }

    [Required]
    [MinLength(1)]
    public List<string> Endpoints { get; set; } = [];
}

public class AssignEndpointsToPermissionResponse : VoidBotResponse
{
}