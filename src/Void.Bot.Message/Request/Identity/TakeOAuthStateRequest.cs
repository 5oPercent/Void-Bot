using Void.Bot.Message.Dto.Identity;

namespace Void.Bot.Message.Request.Identity;

public sealed record TakeOAuthStateRequest(string State);

public abstract record TakeOAuthStateResponse
{
    private TakeOAuthStateResponse() { }

    public sealed record Found(OAuthStateRecord Record) : TakeOAuthStateResponse;
    public sealed record Missing : TakeOAuthStateResponse;
    public sealed record Unavailable : TakeOAuthStateResponse;
}
