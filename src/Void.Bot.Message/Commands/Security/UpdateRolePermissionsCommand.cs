using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Security;

public class UpdateRolePermissionsCommand : ICommand
{
    public Guid RoleId { get; set; }

    [Required]
    [MinLength(1)]
    public List<Guid> PermissionIds { get; set; } = [];
}

public class UpdateRolePermissionsResponse : VoidBotResponse
{
}