using System.Text.Json.Serialization;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Request.Account;

public class CanChangeUserAccountNameRequest : IRequest
{
    [JsonIgnore]
    public Guid? UserId { get; set; }

    public string Name { get; set; } = string.Empty;
}

public class CanChangeUserAccountNameResponse : VoidBotResponse<bool>
{
}
