using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Commands.Security;

public class AddMenuCommand : ICommand
{
    [Required]
    public required MenuDto Menu { get; set; }
}

public class AddMenuResponse : VoidBotResponse
{
}