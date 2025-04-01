using Mediator.Net;

namespace Void.Bot.Core.Services.Discord.DataProvider;

public partial interface IDiscordDataProvider : IScope
{
}

public partial class DiscordDataProvider : IDiscordDataProvider
{
    private readonly IMediator _mediator;

    public DiscordDataProvider(IMediator mediator)
    {
        _mediator = mediator;
    }
}