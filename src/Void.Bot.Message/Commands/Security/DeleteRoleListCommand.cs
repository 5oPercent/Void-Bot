using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Security;

public class DeleteRoleListCommand : ICommand
{
    [Required]
    [MinLength(1)]
    public List<Guid> IdList { get; set; } = [];
}

public class DeleteRoleListResponse : VoidBotResponse
{
}