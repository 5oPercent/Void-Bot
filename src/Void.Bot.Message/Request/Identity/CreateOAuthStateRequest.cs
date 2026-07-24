using Void.Bot.Message.Dto.Identity;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Identity;

namespace Void.Bot.Message.Request.Identity;

public sealed record CreateOAuthStateRequest(
    AppPlatformEnum Platform,
    OAuthStateFlow Flow,
    TimeSpan TimeToLive,
    Guid? InitiatingAccountId = null);

public abstract record CreateOAuthStateResponse
{
    private CreateOAuthStateResponse() { }

    public sealed record Created(OAuthStateRecord Record) : CreateOAuthStateResponse;
    public sealed record Rejected : CreateOAuthStateResponse;
    public sealed record Unavailable : CreateOAuthStateResponse;
}
