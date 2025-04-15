using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Sync;

public class SynchronizeDestiny2ManifestCommand : ICommand
{
    [Required]
    public List<string> Languages { get; set; } = ["en", "zh-cht", "zh-chs"];
    
    public List<string>? Components { get; set; }
    
    public bool ForceSync { get; set; }
}

public class SynchronizeDestiny2ManifestResponse : VoidBotResponse
{
}