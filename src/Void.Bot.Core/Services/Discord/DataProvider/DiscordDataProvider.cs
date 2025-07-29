using Mediator.Net;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Core.Services.Language;

namespace Void.Bot.Core.Services.Discord.DataProvider;

public partial interface IDiscordDataProvider : IScope
{
}

public partial class DiscordDataProvider : IDiscordDataProvider
{
    private readonly IMediator _mediator;
    private readonly ICacheManager _cacheManager;
    private readonly Destiny2PlayerData _destiny2PlayerData;
    private readonly ILanguageService _languageService;

    public DiscordDataProvider(
        IMediator mediator,
        ICacheManager cacheManager,
        Destiny2PlayerData destiny2PlayerData,
        ILanguageService languageService)
    {
        _mediator = mediator;
        _cacheManager = cacheManager;
        _destiny2PlayerData = destiny2PlayerData;
        _languageService = languageService;
    }
}