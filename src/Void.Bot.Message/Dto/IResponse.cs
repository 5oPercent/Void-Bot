using System.Net;
using System.Text.Json.Serialization;
using Mediator.Net.Contracts;

namespace Void.Bot.Message.Dto;

public class VoidBotResponse<T> : VoidBotResponse
{
    public T? Data { get; set; }
}

public class VoidBotResponse : IResponse
{
    public HttpStatusCode Code { get; set; }

    public string Message { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorCode { get; set; }
}