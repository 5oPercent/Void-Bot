using Microsoft.AspNetCore.Authorization;
using Void.Bot.Core.Data;
using Void.Bot.Core.Services.ERA.Team;

namespace Void.Bot.Api.Hubs;

[Authorize(AuthenticationSchemes = "Bearer")]
public sealed class WebSocialHub : SocialHubBase
{
    protected override bool RequiresEraSession
    {
        get
        {
            return false;
        }
    }

    public WebSocialHub(SocialConnectionRegistry connections,
        VoidBotDbContext database, IEraTeamDataProvider sessions)
        : base(connections, database, sessions)
    {
    }
}
