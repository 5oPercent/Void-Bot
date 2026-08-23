using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Account;

namespace Void.Bot.Message.Commands.Account;

public class ChangeUserAccountNameCommand : ICommand
{
    [JsonIgnore]
    public Guid? UserId { get; set; }

    [MinLength(1)]
    [MaxLength(64)]
    public required string Name { get; set; }
}

public class ChangeUserAccountNameResponse : VoidBotResponse<UserAccountInfoDto>
{
}
