using Mediator.Net;
using Void.Bot.Core.Caching;

namespace Void.Bot.Core.Services.Discord.DataProvider;

public partial interface IDiscordDataProvider : IScope
{
}

public partial class DiscordDataProvider : IDiscordDataProvider
{
    private readonly IMediator _mediator;
    private readonly ICacheManager _cacheManager;

    public DiscordDataProvider(IMediator mediator, ICacheManager cacheManager)
    {
        _mediator = mediator;
        _cacheManager = cacheManager;
    }
}