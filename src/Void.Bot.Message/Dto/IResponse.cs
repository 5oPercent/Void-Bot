using System.Net;
using Mediator.Net.Contracts;

namespace Void.Bot.Message.Dto;

public class VoidBotResponse<T> : VoidBotResponse
{
    public T Result { get; set; }
}

public class VoidBotResponse : IResponse
{
    public HttpStatusCode Code { get; set; }
    
    public string Msg { get; set; }
}