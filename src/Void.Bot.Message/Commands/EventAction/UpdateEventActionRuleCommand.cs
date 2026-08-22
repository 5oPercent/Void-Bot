using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.EventAction;

namespace Void.Bot.Message.Commands.EventAction;

public class UpdateEventActionRuleCommand : ICommand
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Event { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public int Sequence { get; set; }

    [Required]
    [MinLength(1)]
    public List<ConfiguredEventActionDto> Actions { get; set; } = [];
}

public class UpdateEventActionRuleResponse : VoidBotResponse<EventActionRuleDto>
{
}
