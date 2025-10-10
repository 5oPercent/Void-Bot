using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Security;

public class RemoveUsersCascadeRoleCommand : ICommand
{
    [Required]
    [MinLength(1)]
    public List<Guid> UserIds { get; set; } = [];
}

public class RemoveUsersCascadeRoleResponse : VoidBotResponse
{
}