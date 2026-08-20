using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Security;

public class DeleteRoleCascadeCommand : ICommand
{
    [Required]
    [MinLength(1)]
    public List<Guid> IdList { get; set; } = [];

    public bool Cascade { get; set; }
}

public class DeleteRoleCascadeResponse : VoidBotResponse
{
}