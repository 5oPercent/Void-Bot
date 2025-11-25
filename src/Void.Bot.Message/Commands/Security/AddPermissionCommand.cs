using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Commands.Security;

public class AddPermissionCommand : ICommand
{
    [Required]
    public required PermissionDto Permission { get; set; }
}

public class AddPermissionResponse : VoidBotResponse
{
}