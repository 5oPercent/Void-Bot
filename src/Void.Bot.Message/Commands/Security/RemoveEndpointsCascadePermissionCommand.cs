using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Security;

public class RemoveEndpointsCascadePermissionCommand : ICommand
{
    [Required]
    [MinLength(1)] 
    public List<string> Endpoints { get; set; } = [];
}

public class RemoveEndpointsCascadePermissionResponse : VoidBotResponse
{
}