using Microsoft.AspNetCore.Authorization;
using Void.Bot.Core.Data;
using Void.Bot.Core.Services.ERA;

namespace Void.Bot.Api.Hubs;

[Authorize(AuthenticationSchemes = "Bearer")]
public sealed class EraSocialHub : SocialHubBase
{
    protected override bool RequiresEraSession
    {
        get
        {
            return true;
        }
    }

    public EraSocialHub(SocialConnectionRegistry connections,
        VoidBotDbContext database, IEraTeamDataProvider sessions)
        : base(connections, database, sessions)
    {
    }
}
